using SaasEcom.Core.DataServices.Interfaces;
using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Infrastructure.Facades
{
  public class BillingFacade
  {

    public BillingFacade(
      ISubscriptionDataService subscriptions,
      IInvoiceDataService invoices,
      ISubscriptionPlanDataService subscriptionPlans,
      IUserDataService users,
      IPaymentDataService payments,
      IBillingPeriodDataService periods,
      IOneOffChargeDataService oneOffCharges)
    {
      this.subscriptions = subscriptions;
      this.subscriptionPlans = subscriptionPlans;
      this.invoices = invoices;
      this.users = users;
      this.payments = payments;
      this.periods = periods;
      this.oneOffCharges = oneOffCharges;
    }

    private ISubscriptionDataService subscriptions;
    private IInvoiceDataService invoices;
    private ISubscriptionPlanDataService subscriptionPlans;
    private IUserDataService users;
    private IPaymentDataService payments;
    private IBillingPeriodDataService periods;
    IOneOffChargeDataService oneOffCharges;

    // TODO: Implement a re-run / delete run function:
    //delete from LineItems
    //where Invoice_Id in (select id from Invoices where InvoiceRun_Id = 2004)

    //delete from Invoices
    //where InvoiceRun_Id = 2004

    //declare @id int
    //select @id = max(id) from Invoices

    //DBCC CHECKIDENT ('Invoices', reseed, @id)

    public async Task<IList<Invoice>> GenerateInvoices(int runId)
    {
      var run = await invoices.GetInvoiceRunAsync(runId);
      if (run.Closed)
        throw new InvalidOperationException("Cannot run invoices for a closed period.");
      List<Invoice> result = new List<Invoice>();
      // Get all the customers
      List<SaasEcomUser> customers = await users.GetAllAsync();
      foreach (SaasEcomUser user in customers)
      {
        // Check for existing invoices for the customer for the period
        List<Invoice> inv = await invoices.ListForCustomerAsync(user.Id);
        if (inv.Any(i => i.InvoiceRun_Id.HasValue && i.InvoiceRun_Id.Value == run.Id))
          continue;

        Invoice invoice = null;
        invoice = new Invoice();
        invoice.Customer = user;
        invoice.Date = DateTime.Now.Date;
        invoice.LineItems = new List<Invoice.LineItem>();
        invoice.PeriodStart = run.PeriodStart;
        invoice.PeriodEnd = run.PeriodEnd;
        invoice.BillingAddress = (BillingAddress)user.BillingAddress.Clone();
        invoice.InvoiceRun_Id = run.Id;

        // Loop through the active subs
        foreach (Subscription sub in (await subscriptions.UserActiveSubscriptionsAsync(user.Id))
            .Where(s => s.SubscriptionPlan.Interval == run.Interval))
        {
          // TODO: possible to have differnt tax rates per line - but no way to represent this in database.
          invoice.TaxPercent = sub.TaxPercent;

          // Is the sub due?
          if ((sub.Start.HasValue && sub.Start.Value <= run.PeriodEnd)
            && (!sub.End.HasValue || sub.End.Value > run.PeriodStart))
          {
            // Get the subscription charge - may be pro-rated
            DateTime startDate = sub.Start.HasValue && sub.Start.Value > run.PeriodStart
                ? sub.Start.Value
                : run.PeriodStart;
            DateTime endDate = sub.End.HasValue && sub.End.Value < run.PeriodEnd
                ? sub.End.Value
                : run.PeriodEnd;
            bool proRata = startDate != run.PeriodStart || endDate != run.PeriodEnd;
            double ratio = !proRata ? 100
                : (endDate - startDate).TotalDays / (run.PeriodEnd - run.PeriodStart).TotalDays * 100;

            int amount = (int)(sub.SubscriptionPlan.Price * ratio);
            Invoice.LineItem itm = new Invoice.LineItem
            {
              Amount = amount,
              Currency = sub.SubscriptionPlan.Currency,
              Period = new Invoice.Period { Start = startDate, End = endDate },
              Proration = proRata,
              Quantity = sub.Quantity,
              Type = sub.SubscriptionPlan.Name,
              Plan = new Invoice.Plan
              {
                AmountInCents = amount,
                Created = sub.Start,
                Currency = sub.SubscriptionPlan.Currency,
                Interval = sub.SubscriptionPlan.Interval.ToString(),
                IntervalCount = 1,
                Name = $"{sub.SubscriptionPlan.Name}{(proRata ? $" ({sub.SubscriptionPlan.Price.ToString("C")} prorata {(ratio / 100).ToString("P0")})": "")}",
                SubscriptionPlanId = sub.SubscriptionPlanId,
                TrialPeriodDays = (sub.TrialEnd.HasValue && sub.TrialEnd.HasValue
                  ? (int?)(sub.TrialEnd.Value - sub.TrialStart.Value).TotalDays
                  : (int?)null)
              }
            };

            invoice.LineItems.Add(itm);
          }

        }

        // Get any one-off charges for the period - old style subscription hack
        foreach (Subscription oneOff in await subscriptions.UserOneOffChargesAsync(
            user.Id, run.PeriodStart, run.PeriodEnd))
        {
          invoice.TaxPercent = oneOff.TaxPercent;
          Invoice.LineItem oneOffitm = new Invoice.LineItem
          {
            Amount = (int)(oneOff.SubscriptionPlan.Price * 100),
            Currency = oneOff.SubscriptionPlan.Currency,
            Period = new Invoice.Period { Start = oneOff.Start, End = oneOff.End },
            Proration = false,
            Quantity = oneOff.Quantity,
            Type = oneOff.SubscriptionPlan.Name,
            Plan = new Invoice.Plan
            {
              AmountInCents = (int)(oneOff.SubscriptionPlan.Price * 100),
              Created = oneOff.Start,
              Currency = oneOff.SubscriptionPlan.Currency,
              Interval = oneOff.SubscriptionPlan.Interval.ToString(),
              IntervalCount = 1,
              Name = oneOff.SubscriptionPlan.Name,
              SubscriptionPlanId = oneOff.SubscriptionPlanId
            }
          };

          invoice.LineItems.Add(oneOffitm);
        }

        // Get any one-off charges for the period - new style actual one-offs
        var charges = await oneOffCharges.ListUninvoicedChargesForUserAsync(user.Id);
        foreach (OneOffCharge charge in charges)
        {
          invoice.TaxPercent = charge.TaxPercent;
          Invoice.LineItem chargeItm = new Invoice.LineItem
          {
            Amount = (int)(charge.Amount),
            Currency = null,
            Period = new Invoice.Period { Start = charge.Date, End = charge.Date },
            Proration = false,
            Quantity = 1,
            Type = charge.Category,
            Plan = new Invoice.Plan
            {
              AmountInCents = charge.Amount,
              Created = charge.Date,
              Currency = null,
              Interval = "OneOff",
              IntervalCount = 1,
              Name = charge.Description,
              SubscriptionPlanId = null
            }
          };
          invoice.LineItems.Add(chargeItm);
        }

        if (invoice.LineItems.Count > 0)
        {
          // Calculate invoice totals
          invoice.Total = 0;
          invoice.Tax = 0;
          invoice.Subtotal = 0;
          foreach (Invoice.LineItem cur in invoice.LineItems)
          {
            int value = cur.Amount.Value * cur.Quantity.Value;
            invoice.Subtotal += value;
          }
          invoice.Tax = (int)(invoice.Subtotal * invoice.TaxPercent.Value / 100);
          invoice.Total = invoice.Subtotal + invoice.Tax;

          // Calculate what's owing
          invoice.StartingBalance =
              (await invoices.ListForCustomerAsync(user.Id))
              .Where(i => !i.Forgiven)
              .Sum(i => i.Total) -
              ((await payments.ListForCustomerAsync(user.Id)).Sum(p => p.Amount));
          invoice.EndingBalance = invoice.StartingBalance + invoice.Total;
          invoice.AmountDue = invoice.EndingBalance;

          // Save the invoice
          await invoices.CreateOrUpdateAsync(invoice);
          result.Add(invoice);

          // Update the one off charges
          foreach(OneOffCharge charge in charges)
          {
            charge.InvoiceId = invoice.Id;
            await oneOffCharges.SaveAsync(charge);
          }
        }
      }

      return result;
    }

    public async Task<Invoice> GetInvoice(int id)
    {
      return await invoices.InvoiceAsync(id);
    }

    private static DateTime CalculateEndDate(DateTime startDate, Subscription sub)
    {
      DateTime endDate = startDate;
      switch (sub.SubscriptionPlan.Interval)
      {
        case SubscriptionInterval.Weekly:
          endDate = startDate.AddDays(6);
          break;
        case SubscriptionInterval.Monthly:
          endDate = startDate.AddMonths(1).AddDays(-1);
          break;
        case SubscriptionInterval.EveryThreeMonths:
          endDate = startDate.AddMonths(3).AddDays(-1);
          break;
        case SubscriptionInterval.EverySixMonths:
          endDate = startDate.AddMonths(6).AddDays(-1);
          break;
        case SubscriptionInterval.Yearly:
          endDate = startDate.AddYears(1).AddDays(-1);
          break;
        default:
          throw new InvalidOperationException(String.Format("Unexpected suscription interval:{0}", sub.SubscriptionPlan.Interval.ToString()));
      }
      return endDate;
    }

    public async Task<IEnumerable<InvoiceRun>> ListInvoiceRunsAsync(SubscriptionInterval interval)
    {
      // Get the list of invoice runs
      var result = (await invoices.ListInvoiceRunsAsync(interval, InvoiceRunType.Both))
          .OrderByDescending(r => r.PeriodStart)
          .ToList();
      var cur = result.FirstOrDefault();
      if (cur != null)
      {
        // Check it is current
        if (cur.PeriodEnd <= DateTime.Now && cur.Closed)
          cur = null;
      }

      // Make sure we have a current period
      if (cur == null)
      {
        cur = await CurrentPeriodAsync(interval);
        result.Insert(0, cur);
      }

      return result.ToArray();
    }

    public async Task<InvoiceRun> CurrentPeriodAsync(SubscriptionInterval interval)
    {
      // See if there is aleady an open period
      IEnumerable<InvoiceRun> runs = await invoices.ListInvoiceRunsAsync(interval, InvoiceRunType.Open);
      if (runs.Count() > 0)
        return runs.OrderByDescending(r => r.PeriodEnd).First();

      // Check to see if current period is already closed
      runs = await invoices.ListInvoiceRunsAsync(interval, InvoiceRunType.Closed);
      InvoiceRun result = runs.OrderByDescending(r => r.PeriodEnd).FirstOrDefault();
      if (result == null || result.PeriodEnd < DateTime.Now)
      {
        BillingPeriod period = await periods.GetAsync(interval);
        if (result == null)
        {
          // No invoice run - create the first
          result = new InvoiceRun { Interval = interval, PeriodStart = DateTime.Now.Date };
          switch (period.Interval)
          {
            case SubscriptionInterval.Monthly:
              result.PeriodStart = result.PeriodStart.AddDays(-result.PeriodStart.Day).AddDays(period.StartDay);
              if (period.RunNextPeriod)
                result.PeriodStart = result.PeriodStart.AddMonths(-1);
              result.PeriodEnd = result.PeriodStart.AddMonths(1);
              break;
            case SubscriptionInterval.Weekly:
              result.PeriodStart = result.PeriodStart.AddDays(-((int)result.PeriodStart.DayOfWeek)).AddDays(period.StartDay);
              if (period.RunNextPeriod)
                result.PeriodStart = result.PeriodStart.AddDays(-7);
              result.PeriodEnd = result.PeriodStart.AddDays(7);
              break;
            default:
              throw new NotImplementedException();
          }
        }
        else
        {
          // Closed invoice run - create the next
          result = new InvoiceRun { Interval = interval, PeriodStart = result.PeriodEnd };
          switch (period.Interval)
          {
            case SubscriptionInterval.Monthly:
              result.PeriodEnd = result.PeriodStart.AddMonths(1);
              break;
            case SubscriptionInterval.Weekly:
              result.PeriodEnd = result.PeriodStart.AddDays(7);
              break;
            case SubscriptionInterval.EveryThreeMonths:
              result.PeriodEnd = result.PeriodStart.AddMonths(3);
              break;
            case SubscriptionInterval.EverySixMonths:
              result.PeriodEnd = result.PeriodStart.AddMonths(6);
              break;
            case SubscriptionInterval.Yearly:
              result.PeriodEnd = result.PeriodStart.AddYears(1);
              break;
            default:
              throw new NotImplementedException();
          }

        }

        return await invoices.CreateOrUpdateRunAsync(result);
      }
      else
      {
        // Current period is closed, return it.
        return result;
      }
    }

    public async Task<List<Invoice>> ListInvoicesForRunAsync(int id)
    {
      return await invoices.ListInvoicesForRunAsync(id);
    }

    public async Task<List<Invoice>> GetInvoicesAsync(IEnumerable<int> ids)
    {
      return await this.invoices.GetInvoicesAsync(ids);
    }

    public async Task ClosePeriodAsync(int id)
    {
      var run = await invoices.GetInvoiceRunAsync(id);
      run.Closed = true;
      await invoices.CreateOrUpdateRunAsync(run);
    }

    public async Task InvoiceSent(Invoice invoice)
    {
      invoice.Sent = true;
      await invoices.CreateOrUpdateAsync(invoice);
    }

    public async Task<InvoiceRun> GetInvoiceRunAsync(int runId)
    {
      return await invoices.GetInvoiceRunAsync(runId);
    }

    public async Task<List<Invoice>> ListUnpaidInvoices()
    {

      return await invoices.ListUnpaidInvoices();
    }

    public async Task AllocateInvoicePaymentAsync(int invoiceId, int paymentId, int amount)
    {
      Payment pmt = await payments.GetAsync(paymentId);
      if (pmt.Reconciliations == null)
        pmt.Reconciliations = new List<Reconciliation>();
      pmt.Reconciliations.Add(new Reconciliation { Amount = amount, InvoiceId = invoiceId, PaymentId = paymentId });
      await payments.SaveAsync(pmt);
      Invoice invoice = await invoices.InvoiceAsync(invoiceId);
      double total = 0;
      if (invoice.Reconciliations != null && invoice.Reconciliations.Count > 0)
        total = invoice.Reconciliations.Sum(r => r.Amount);
      bool paidNow = total >= invoice.Total;
      if (invoice.Paid != paidNow)
      {
        invoice.Paid = paidNow;
        await invoices.CreateOrUpdateAsync(invoice);
      }
    }

    public async Task<int> ReconcileInvoicesAsync()
    {
      int result = 0;
      // Get all the invoices that have not been paid or forgiven
      List<Invoice> invs = await invoices.ListUnpaidInvoices();

      // Iterate through them
      foreach (Invoice invoice in invs)
      {
        // Get the payments for that customer
        List<Payment> pmts = await payments.ListForCustomerAsync(invoice.Customer.Id);
        pmts = pmts.Where(p => p.Reconciliations == null || p.Reconciliations.Count == 0 ||
            p.Reconciliations.Sum(r => r.Amount) < p.Amount).ToList();

        if (invoice.Reconciliations == null)
          invoice.Reconciliations = new List<Reconciliation>();

        // Alocate un allocated until all paid or nothing left
        int sum = invoice.Reconciliations.Count == 0 ? 0 : invoice.Reconciliations.Sum(r => r.Amount);
        int remainder = invoice.Total.Value - sum;
        while (remainder > 0 && pmts.Count > 0)
        {
          Payment candidate = pmts.First();
          int freeAmt = candidate.Amount - (candidate.Reconciliations.Count > 0
              ? candidate.Reconciliations.Sum(r => r.Amount)
              : 0);
          Reconciliation rec = new Reconciliation
          {
            InvoiceId = invoice.Id,
            PaymentId = candidate.Id,
            Amount = remainder < freeAmt ? remainder : freeAmt
          };
          invoice.Reconciliations.Add(rec);
          candidate.Reconciliations.Add(rec);
          remainder -= rec.Amount;
          invoice.Paid = remainder <= 0;
          if (invoice.Paid == true)
            result++;
          if (candidate.Reconciliations.Sum(r => r.Amount) >= candidate.Amount)
            pmts.RemoveAt(0);
          await invoices.CreateOrUpdateAsync(invoice);
        }


      }

      return result;
    }
  }
}

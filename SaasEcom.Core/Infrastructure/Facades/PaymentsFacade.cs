using Microsoft.AspNet.Identity;
using SaasEcom.Core.DataServices.Interfaces;
using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Infrastructure.Facades
{
  public class PaymentsFacade
  {
    private IPaymentDataService payments;
    private IUserDataService users;
    private IIdentityMessageService messageSvc;
    private ITransactionLoader getter;
    private IPaymentParser parser;

    public PaymentsFacade(
        IPaymentDataService payments,
        IUserDataService users,
        IIdentityMessageService messageSvc,
        ITransactionLoader getter,
        IPaymentParser parser
        )
    {
      this.payments = payments;
      this.users = users;
      this.messageSvc = messageSvc;
      this.getter = getter;
      this.parser = parser;
    }

    public async Task<bool> ImportPaymentAsync(Payment payment)
    {
      return await payments.Import(payment);
    }


    public async Task<IList<Payment>> ListForPeriodAsync(DateTime? start, DateTime? end)
    {
      return await payments.ListForPeriodAsync(start, end);
    }

    public async Task<int> MatchPayments()
    {
      // Get all the unmatched payments
      List<Payment> unmatched = await payments.ListUnmatchedAsync();
      // Get the customers
      List<SaasEcomUser> customers = await users.GetAllAsync();

      // Try to match them together
      int matched = 0;
      foreach (Payment pmnt in unmatched)
      {
        SaasEcomUser user = customers.FirstOrDefault((u) =>
          {
            var acct = u.AccountNumber.ToUpper();
            return (pmnt.Description + pmnt.Particulars).ToUpper().Contains(u.AccountNumber) ||
            pmnt.Reference.ToUpper().Contains(u.AccountNumber);
          });
        if (user != null)
        {
          pmnt.Customer = user;
          pmnt.CustomerId = user.Id;
          await payments.SaveAsync(pmnt);
          matched++;
        }
      }

      // Return the result
      return await Task.FromResult(matched);
    }

    public async Task<Payment> GetPaymentAsync(int id)
    {
      return await payments.GetAsync(id);
    }

    public async Task SavePaymentAsync(Payment pmnt)
    {
      await payments.SaveAsync(pmnt);
    }

    public async Task<List<Payment>> ListForCustomerAsync(string id)
    {
      return await payments.ListForCustomerAsync(id);
    }

    public async Task<DateTime> GetLastPaymentImportDateAsync()
    {
      Payment payment = await payments.GetLastImported();
      return payment.Date;
    }

    public async Task<int> AcknowledgePaymentsAsync()
    {
      int result = 0;
      // Get unacknowledged payments
      foreach (Payment pmt in await payments.ListUnacknowledgedAsync())
      {
        // Send a thank you for each of them
        double amt = ((double)pmt.Amount) / 100;
        string message = String.Format(
@"Dear {0},
Thank you for your payment of {1:C}.
It has been credited to your account.
Regards,
The Keyryx Team.
",
          pmt.Customer.KnownAs ?? pmt.Customer.FirstName,
          amt
          );


        messageSvc.Send(new IdentityMessage
          {
            Subject = String.Format("Payment of {0:C} received", amt),
            Body = message,
            Destination = pmt.Customer.Email
          });

        // Save the acknowledgement
        pmt.Acknowledged = true;
        await payments.SaveAsync(pmt);
        result++;
      }
      return result;
    }

    public async Task<ImportResult> ImportPaymentsAsync(DateTime? start, DateTime? end)
    {
      if (!end.HasValue)
      {
        end = DateTime.Now;
      }

      if(!start.HasValue)
      {
        // Get the date of the last payment imported
        Payment last = await payments.GetLastImported();
        if (last != null)
          start = last.Date;
        else
          start = DateTime.Now.AddYears(-1);
      }

      ImportResult result = new ImportResult();
    
      using (var strm = getter.GetTransactions(start.Value, end.Value))
      {
        using (StreamReader reader = new StreamReader(strm))
        {
          string line = null;
          while ((line = reader.ReadLine()) != null)
          {
            // Parse the payment
            Payment payment = parser.Parse(line);
            if (payment != null)
            {
              // Post them to the database
              if (await payments.Import(payment))
                result.Imported++;
              else
                result.Duplicates++;
            }
            else
            {
              result.Skipped++;
            }
          }
        }
      }

      return result;
    }

    public class ImportResult
    {
      public int Imported { get; set; }
      public int Duplicates { get; set; }
      public int Skipped { get; set; }
    }
  }
}

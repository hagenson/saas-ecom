using SaasEcom.Core.DataServices.Interfaces;
using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Infrastructure.Facades
{
    public class AccountFacade
    {
        private IInvoiceDataService invoiceSvc;
        private IPaymentDataService paymentSvc;
        private IBillingPeriodDataService billperiodSvc;
        private IUserDataService userSvc;
        private ISettingsDataService settings;

        public AccountFacade(
            IInvoiceDataService invoiceSvc,
            IPaymentDataService paymentSvc,
            IBillingPeriodDataService billperiodSvc,
            IUserDataService userSvc,
            ISettingsDataService settings)
        {
            this.invoiceSvc = invoiceSvc;
            this.paymentSvc = paymentSvc;
            this.billperiodSvc = billperiodSvc;
            this.userSvc = userSvc;
            this.settings = settings;
        }

        public async Task<AccountSummary> GetAccountSummary(string userId)
        {
            AccountSummary result = new AccountSummary();
            result.Id = userId;
            var payments = await paymentSvc.ListForCustomerAsync(userId);
            var invoices = await invoiceSvc.ListForCustomerAsync(userId); 
            var tot =
                payments.Sum(p => p.Amount) -
                (invoices
                    .Where(i => !i.Forgiven)
                    .Select(i => i.Total.HasValue ? i.Total.Value : 0)
                    .Sum(t => t));
            result.Balance = ((double)tot) / 100;
            var lastInvoice = invoices.LastOrDefault();
            if (lastInvoice != null)
            {
                result.LastInvoiceAmount = lastInvoice.Total.HasValue ? ((double)lastInvoice.Total.Value) / 100 : 0;
                result.LastInvoiceDate = lastInvoice.Date;
                string intStr = await settings.GetValueAsync("billing.interval");
                SubscriptionInterval interval = SubscriptionInterval.Monthly;
                if (intStr != null)
                    Enum.TryParse(intStr, out interval);
                var period = await billperiodSvc.GetAsync(interval);
                DateTime run = new DateTime(
                    result.LastInvoiceDate.Value.Year,
                    result.LastInvoiceDate.Value.Month,
                    period.RunDay
                    );
                result.LastInvoiceDue = run.AddDays(period.DueDays);
            }

            var lastPayment = payments.LastOrDefault();
            if(lastPayment != null)
            {
                result.LastPaymentAmount = ((double)lastPayment.Amount) / 100;
                result.LastPaymentDate = lastPayment.Date;
            }

            var user = await userSvc.GetAsync(userId);
            result.Name = user.FirstName + " " + user.LastName;
            result.CurrentSubValue = user.Subscriptions
              .Where(s => !s.End.HasValue || s.End.Value > DateTime.Now)
              .OrderByDescending(s => s.Start)
              .Sum(s => s.SubscriptionPlan.Price);
            var startSub = user.Subscriptions
              .Where(s => s.Start.HasValue)
              .OrderBy(s => s.Start)
              .FirstOrDefault();
            if (startSub != null)
              result.StartDate = startSub.Start.Value;
            return result;
        }
    }
}

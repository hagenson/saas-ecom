using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.DataServices.Interfaces
{
    public interface IBillingPeriodDataService
    {
        Task<BillingPeriod> GetAsync(SubscriptionInterval interval);

        Task SaveAsync(BillingPeriod period);

    }
}

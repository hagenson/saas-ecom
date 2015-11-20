using SaasEcom.Core.Infrastructure.PaymentProcessor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Infrastructure.PaymentProcessor.Internal
{
    /// <summary>
    /// Implements the ISubscriptionProvider using the internal database.
    /// </summary>
    public class InternalSubscriptionProvider : ISubscriptionProvider
    {
        public string SubscribeUser(Models.SaasEcomUser user, string planId, int trialInDays = 0, decimal taxPercent = 0)
        {
            return null;
        }

        public string SubscribeUser(Models.SaasEcomUser user, string planId, DateTime? trialEnds, decimal taxPercent = 0)
        {
            return null;
        }

        public Task<List<Models.Subscription>> UserSubscriptionsAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public DateTime EndSubscription(string userStripeId, string subStripeId, bool cancelAtPeriodEnd = false)
        {
            return DateTime.Now;
        }

        public bool UpdateSubscription(string customerId, string subStripeId, string newPlanId, bool proRate)
        {
            return true;
        }

        public bool UpdateSubscriptionTax(string customerId, string subStripeId, decimal taxPercent = 0)
        {
            return true;
        }

        public object SubscribeUserNaturalMonth(Models.SaasEcomUser user, string planId, DateTime? billingAnchorCycle, decimal taxPercent)
        {
            return null;
        }
    }
}

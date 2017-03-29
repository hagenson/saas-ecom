using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Models
{
    /// <summary>
    /// Subscription Interval.
    /// </summary>
    public enum SubscriptionInterval
    {
        /// <summary>
        /// One off items that are billed with the next subscription invoice.
        /// </summary>
        [Display(ResourceType = typeof(Resources.SaasEcom), Name = "SubscriptionInterval_Oneoff_Oneoff")]
        OneOff = 0,
        /// <summary>
        /// Monthly
        /// </summary>
        [Display(ResourceType = typeof(Resources.SaasEcom), Name = "SubscriptionInterval_Monthly_Monthly")]
        Monthly = 1,

        /// <summary>
        /// Yearly
        /// </summary>
        [Display(ResourceType = typeof(Resources.SaasEcom), Name = "SubscriptionInterval_Yearly_Yearly")]
        Yearly = 2,

        /// <summary>
        /// Weekly
        /// </summary>
        [Display(ResourceType = typeof(Resources.SaasEcom), Name = "SubscriptionInterval_Weekly_Weekly")]
        Weekly = 3,

        /// <summary>
        /// Every 6 months
        /// </summary>
        [Display(ResourceType = typeof(Resources.SaasEcom), Name = "SubscriptionInterval_EverySixMonths_Every_6_months")]
        EverySixMonths = 4,

        /// <summary>
        /// Every 3 months
        /// </summary>
        [Display(ResourceType = typeof(Resources.SaasEcom), Name = "SubscriptionInterval_EveryThreeMonths_Every_3_months")]
        EveryThreeMonths = 5
    }
}

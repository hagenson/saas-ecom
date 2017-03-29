using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Models
{
    /// <summary>
    /// Defines the properties of the different billing periods.
    /// </summary>
    public class BillingPeriod
    {
        /// <summary>
        /// Gets and sets the interval that this billing period information belongs to.
        /// </summary>
        [Key]
        public SubscriptionInterval Interval { get; set; }

        /// <summary>
        /// The day of the month/week etc the billing period starts
        /// </summary>
        public int StartDay { get; set; }

        /// <summary>
        /// Gets and sets the day of the current or next period that invoices are generated,
        /// depending on <see cref="RunNextPeriod"/>
        /// </summary>
        public int RunDay { get; set; }

        /// <summary>
        /// True if invoices are to be run in the next period, False to run them in the same period.
        /// </summary>
        public bool RunNextPeriod { get; set; }

        /// <summary>
        /// Gets the number of days after an invoice is generated that it is due.
        /// </summary>
        public int DueDays { get; set; }


        public void GetPeriodBounds(DateTime refDate, out DateTime startDate, out DateTime endDate)
        {
            switch (Interval)
            {
                case SubscriptionInterval.Monthly:
                    startDate = refDate.Date.AddDays(-refDate.Day).AddDays(StartDay);
                    endDate = startDate.AddMonths(1);
                    break;
                case SubscriptionInterval.Weekly:
                    startDate = refDate.Date.AddDays(-((int)refDate.DayOfWeek)).AddDays(StartDay);
                    endDate = startDate.AddDays(7);
                    break;
                default:
                    throw new NotImplementedException();
            }

        }
    }
}

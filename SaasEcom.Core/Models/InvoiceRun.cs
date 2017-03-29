using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Models
{
    public class InvoiceRun
    {
        public int Id { get; set; }

        public DateTime PeriodStart { get; set;}

        public DateTime PeriodEnd { get; set;}

        public SubscriptionInterval Interval { get; set; }

        public int Sequence { get; set; }

        public bool Closed { get; set; }
    }
}

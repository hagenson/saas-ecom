using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Infrastructure.Facades
{
    public class AccountSummary
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public double Balance { get; set; }

        public double? LastPaymentAmount { get; set; }

        public DateTime? LastPaymentDate { get; set; }

        public double? LastInvoiceAmount { get; set; }

        public DateTime? LastInvoiceDate { get; set; }

        public DateTime? LastInvoiceDue { get; set; }
    }

}

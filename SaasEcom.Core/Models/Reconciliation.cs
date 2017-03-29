using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Models
{
    public class Reconciliation
    {
        public int Id { get; set; }

        public int PaymentId { get; set; }

        public int InvoiceId { get; set; }

        public int Amount { get; set; }

        [ForeignKey("PaymentId")]
        public Payment Payment { get; set; }

        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }
    }
}

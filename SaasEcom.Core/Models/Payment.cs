using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Models
{
    public class Payment: ITransaction
    {
        public int Id { get; set; }

        public string Reference { get; set; }

        public string Description { get; set; }
        
        public string Particulars { get; set; }

        public string CustomerId { get; set; }

        public int? Balance { get; set; }

        public DateTime Date { get; set; }

        public int Amount { get; set; }

        public ICollection<Reconciliation> Reconciliations { get; set; }

        [ForeignKey("CustomerId")]
        public SaasEcomUser Customer { get; set; }

        public bool Acknowledged { get; set; }
    }
}

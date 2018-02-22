using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Models
{
  public class OneOffCharge
  {
    public int Id { get; set; }
    public DateTime? Date { get; set; }
    public int Amount { get; set; }
    public decimal TaxPercent { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string Reference { get; set; }
    public int? InvoiceId { get; set; }
    public string CustomerId { get; set; }
  }
}

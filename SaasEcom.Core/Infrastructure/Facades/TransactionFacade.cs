using SaasEcom.Core.DataServices.Interfaces;
using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Infrastructure.Facades
{
  public class TransactionFacade
  {
    private IPaymentDataService paymentSvc;
    private IInvoiceDataService invoiceSvc;
    
    public TransactionFacade(IPaymentDataService paymentSvc, IInvoiceDataService invoiceSvc)
    {
      this.paymentSvc = paymentSvc;
      this.invoiceSvc = invoiceSvc;
    }

    public async Task<IEnumerable<ITransaction>> ListTransactions(string userId)
    {
      return (await paymentSvc.ListForCustomerAsync(userId))
        .Cast<ITransaction>()
        .Concat(
          (await invoiceSvc.ListForCustomerAsync(userId))
//          .Where(i => !i.Forgiven)
          .Cast<ITransaction>()
        ).OrderByDescending(t => t.Date);
    }
  }
}

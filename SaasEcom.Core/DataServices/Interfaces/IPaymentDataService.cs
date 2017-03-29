using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.DataServices.Interfaces
{
    public interface IPaymentDataService
    {       
        Task<bool> Import(Payment payment);

        Task<IList<Payment>> ListForPeriodAsync(DateTime? start, DateTime? end);

        Task<List<Payment>> ListUnmatchedAsync();

        Task SaveAsync(Payment pmnt);

        Task<Payment> GetAsync(int id);

        Task<List<Payment>> ListForCustomerAsync(string id);

        Task<Payment> GetLastImported();

        Task<IEnumerable<Payment>> ListUnacknowledgedAsync();
    }
}

using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.DataServices.Interfaces
{
  public interface IOneOffChargeDataService
  {
    Task<List<OneOffCharge>> ListUninvoicedChargesForUserAsync(string userId);

    Task<List<OneOffCharge>> ListUninvoicedChargesForAllUsers();

    Task SaveAsync(OneOffCharge charge);

    Task Remove(int chargeId); 

  }
}

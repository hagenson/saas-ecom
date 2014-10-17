﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SaasEcom.Core.Models;

namespace SaasEcom.Core.DataServices.Interfaces
{
    public interface ICardDataService
    {
        Task<IList<CreditCard>> GetAllAsync(string customerId);
        Task<CreditCard> FindAsync(string customerId, int? cardId, bool noTracking = false);
        Task AddAsync(CreditCard creditcard);
        Task UpdateAsync(string customerId, CreditCard creditcard);
        Task DeleteAsync(string customerId, int cardId);
        Task<bool> AnyAsync(int? cardId, string customerId);
    }
}

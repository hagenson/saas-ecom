using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.DataServices.Interfaces
{
    public interface IUserDataService
    {
        Task<List<SaasEcomUser>> GetAllAsync();

        Task<SaasEcomUser> GetAsync(string userId);

        Task SaveAsync(SaasEcomUser user);

        Task<SaasEcomUser> GetByIpAsync(string ip);
    }
}

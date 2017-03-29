using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.DataServices.Interfaces
{
    public interface ISettingsDataService
    {
        Task<string> GetValueAsync(string key);

        Task SetValueAsync(string key, string value);
    }
}

using SaasEcom.Core.DataServices.Interfaces;
using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.DataServices.Storage
{
    public class SettingsDataService<TContext, TUser> : ISettingsDataService
        where TContext : IDbContext<TUser>
        where TUser : class
    {
        private TContext context;

        public SettingsDataService(TContext context)
        {
            this.context = context;
        }

        public async Task<string> GetValueAsync(string key)
        {
            var entry = await context.Settings.Where(s => s.Key == key)
                .FirstOrDefaultAsync();
            return entry != null ? entry.Value : null;
        }

        public async Task SetValueAsync(string key, string value)
        {
            var entry = await context.Settings.Where(s => s.Key == key)
                .FirstOrDefaultAsync();
            if (value == null)
            {
                if (entry != null)
                {
                    context.Settings.Remove(entry);
                    await context.SaveChangesAsync();
                }
            }
            else
            {
                if (entry == null)
                {
                    entry = new Setting { Key = key, Value = value };
                    context.Settings.Add(entry);
                }
                else
                {
                    entry.Value = value;
                }
                await context.SaveChangesAsync();
            }
        }
    }
}

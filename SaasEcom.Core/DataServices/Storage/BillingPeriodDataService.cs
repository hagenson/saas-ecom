using SaasEcom.Core.DataServices.Interfaces;
using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.DataServices.Storage
{
    public class BillingPeriodDataService<TContext, TUser> : IBillingPeriodDataService
        where TContext : IDbContext<TUser>
        where TUser : SaasEcomUser
    {
        private readonly TContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardDataService{TContext, TUser}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public BillingPeriodDataService(TContext context)
        {
            this.dbContext = context;
        }

        public async Task<BillingPeriod> GetAsync(SubscriptionInterval interval)
        {
            return await dbContext.BillingPeriods.FindAsync(interval);
        }

        public async Task SaveAsync(BillingPeriod period)
        {
            BillingPeriod dbp = await dbContext.BillingPeriods.FindAsync(period);
            if (dbp == null)
            {
                dbp = new BillingPeriod();
                dbContext.BillingPeriods.Add(dbp);
            }

            dbp.Interval = period.Interval;
            dbp.DueDays = period.DueDays;
            dbp.RunDay = period.RunDay;
            await dbContext.SaveChangesAsync();
        }

    }
}

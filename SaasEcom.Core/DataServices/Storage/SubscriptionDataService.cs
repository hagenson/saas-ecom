using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SaasEcom.Core.DataServices.Interfaces;
using SaasEcom.Core.Models;

namespace SaasEcom.Core.DataServices.Storage
{
    /// <summary>
    /// Implementation for CRUD related to subscriptions in the database.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    public class SubscriptionDataService<TContext, TUser> : ISubscriptionDataService
        where TContext : IDbContext<TUser>
        where TUser : class
    {
        private readonly TContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionDataService{TContext, TUser}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SubscriptionDataService(TContext context)
        {
            this._dbContext = context;
        }

        /// <summary>
        /// Finds the by identifier.
        /// </summary>
        /// <param name="stripeSubscriptionId">The stripe subscription identifier.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Subscription FindById(string stripeSubscriptionId)
        {
            return _dbContext.Subscriptions.FirstOrDefault(s => s.StripeId == stripeSubscriptionId);
        }

        public Task<Subscription> GetByIdAsync(int id)
        {
            return _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Subscribes the user asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="trialPeriodInDays">The trial period in days.</param>
        /// <param name="taxPercent">The tax percent.</param>
        /// <param name="stripeId">The stripe identifier.</param>
        /// <returns>
        /// The subscription
        /// </returns>
        /// <exception cref="System.ArgumentException"></exception>
        public async Task<Subscription> SubscribeUserAsync(SaasEcomUser user, string planId, int? trialPeriodInDays = null, decimal taxPercent = 0, string stripeId = null)
        {
            var plan = await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == planId);

            if (plan == null)
            {
                throw new ArgumentException(string.Format("There's no plan with Id: {0}", planId));
            }

            var s = new Subscription
            {
                Start = DateTime.UtcNow,
                End = null,
                TrialEnd = DateTime.UtcNow.AddDays(trialPeriodInDays ?? plan.TrialPeriodInDays),
                TrialStart = DateTime.UtcNow,
                UserId = user.Id,
                SubscriptionPlan = plan,
                Status = trialPeriodInDays == null ? "active" : "trialing",
                TaxPercent = taxPercent,
                StripeId = stripeId,
                Quantity = 1
            };

            _dbContext.Subscriptions.Add(s);
            await _dbContext.SaveChangesAsync();

            return s;
        }

        /// <summary>
        /// Subscribes the user asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="trialPeriodEnds">The trial period ends.</param>
        /// <param name="taxPercent">The tax percent.</param>
        /// <param name="stripeId">The stripe identifier.</param>
        /// <returns>
        /// The subscription
        /// </returns>
        /// <exception cref="System.ArgumentException"></exception>
        public async Task<Subscription> SubscribeUserAsync(SaasEcomUser user, string planId, DateTime? trialPeriodEnds = null, decimal taxPercent = 0, string stripeId = null)
        {
            var plan = await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == planId);

            if (plan == null)
            {
                throw new ArgumentException(string.Format("There's no plan with Id: {0}", planId));
            }

            var s = new Subscription
            {
                Start = DateTime.UtcNow,
                End = null,
                TrialEnd = trialPeriodEnds ?? DateTime.UtcNow.AddDays(plan.TrialPeriodInDays),
                TrialStart = DateTime.UtcNow,
                UserId = user.Id,
                SubscriptionPlan = plan,
                Status = trialPeriodEnds == null ? "active" : "trialing",
                TaxPercent = taxPercent,
                StripeId = stripeId,
                Quantity = 1
            };

            _dbContext.Subscriptions.Add(s);
            await _dbContext.SaveChangesAsync();

            return s;
        }

        public async Task<Subscription> SubscribeUserAtDateAsync(string userId, string planId, DateTime startDate, decimal taxPercent = 0, string stripeId = null)
        {
            var plan = await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == planId);

            if (plan == null)
            {
                throw new ArgumentException(string.Format("There's no plan with Id: {0}", planId));
            }

            var s = new Subscription
            {
                Start = startDate,
                End = null,
                UserId = userId,
                SubscriptionPlan = plan,
                Status = "active",
                TaxPercent = taxPercent,
                StripeId = stripeId,
                Quantity = 1
            };

            _dbContext.Subscriptions.Add(s);
            await _dbContext.SaveChangesAsync();

            return s;
        }

        /// <summary>
        /// Get the User's active subscription asynchronous. Only the first (valid if your customers can have only 1 subscription at a time).
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        /// The subscription
        /// </returns>
        public async Task<Subscription> UserActiveSubscriptionAsync(string userId)
        {
            return (await UserActiveSubscriptionsAsync(userId)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the User's subscriptions asynchronous.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public async Task<List<Subscription>> UserSubscriptionsAsync(string userId)
        {
            return await _dbContext.Subscriptions.Where(s => s.User.Id == userId
                && s.SubscriptionPlan.Interval > SubscriptionInterval.OneOff).Select(s => s).ToListAsync();
        }

        /// <summary>
        /// Get the User's active subscriptions asynchronous.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        /// The list of Subscriptions
        /// </returns>
        public async Task<List<Subscription>> UserActiveSubscriptionsAsync(string userId)
        {
            var periodStart = DateTime.Today;
            periodStart = periodStart.AddDays(1 - periodStart.Day);
            return await _dbContext.Subscriptions
                .Where(s => s.User.Id == userId)
                .Where(s => s.End == null || s.End > periodStart)
                .Where(s => s.Start <= periodStart)
                .Where(s => s.SubscriptionPlan.Interval > SubscriptionInterval.OneOff)
                .Include(s => s.SubscriptionPlan.Properties)
                .Select(s => s).ToListAsync();
        }

        /// <summary>
        /// Ends the subscription asynchronous.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="subscriptionEnDateTime">The subscription en date time.</param>
        /// <param name="reasonToCancel">The reason to cancel.</param>
        /// <returns></returns>
        public async Task EndSubscriptionAsync(int subscriptionId, DateTime subscriptionEnDateTime, string reasonToCancel)
        {
            var dbSub = await _dbContext.Subscriptions.FindAsync(subscriptionId);
            dbSub.End = subscriptionEnDateTime;
            dbSub.ReasonToCancel = reasonToCancel;
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the subscription asynchronous.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns></returns>
        public async Task UpdateSubscriptionAsync(Subscription subscription)
        {
            _dbContext.Entry(subscription).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }


        /// <summary>
        /// Updates the subscription tax.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="taxPercent">The tax percent.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task UpdateSubscriptionTax(string subscriptionId, decimal taxPercent)
        {
            var subscription = await _dbContext.Subscriptions.Where(s => s.StripeId == subscriptionId).FirstOrDefaultAsync();
            subscription.TaxPercent = taxPercent;
            await _dbContext.SaveChangesAsync();
        }


        /// <summary>
        /// Deletes the subscriptions asynchronous.
        /// </summary>
        /// <param name="subscriptionId">The user identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task DeleteSubscriptionsAsync(string subscriptionId)
        {
            int id = int.Parse(subscriptionId);
            foreach (var subscription in _dbContext.Subscriptions.Where(s => s.Id == id).Select(s =>s))
            {
                _dbContext.Subscriptions.Remove(subscription);
            }

            await _dbContext.SaveChangesAsync();
        }


        public async Task<List<Subscription>> UserOneOffChargesAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Subscriptions
                .Where(s => s.User.Id == userId && s.Status != "canceled" && s.Status != "unpaid")
                .Where(s => s.SubscriptionPlan.Interval == SubscriptionInterval.OneOff)
                .Where(s => s.Start >= startDate && s.Start <= endDate)
                .Include(s => s.SubscriptionPlan.Properties)
                .Select(s => s).ToListAsync();
        }
    }
}

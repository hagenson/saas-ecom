using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SaasEcom.Core.DataServices.Interfaces;
using SaasEcom.Core.Models;

namespace SaasEcom.Core.DataServices.Storage
{
    /// <summary>
    /// Implementation for CRUD related to invoices in the database.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    public class InvoiceDataService<TContext, TUser> : IInvoiceDataService
        where TContext : IDbContext<TUser>
        where TUser : SaasEcomUser
    {
        private readonly TContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceDataService{TContext, TUser}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public InvoiceDataService(TContext context)
        {
            this._dbContext = context;
        }

        /// <summary>
        /// Returns all the invoice given a user identifier.
        /// </summary>
        /// <param name="customerId">The user identifier.</param>
        /// <returns>List of invoices</returns>
        public async Task<List<Invoice>> ListForCustomerAsync(string customerId)
        {
            return await _dbContext.Invoices
                .Where(i => i.Customer.Id == customerId)
                .Include(x => x.Reconciliations)
                .Select(s => s).ToListAsync();
        }

        /// <summary>
        /// Gets the invoice given a users identifier and the invoice identifier.
        /// </summary>
        /// <param name="invoiceId">The invoice identifier.</param>
        /// <returns>
        /// The invoice
        /// </returns>
        public async Task<Invoice> InvoiceAsync(int invoiceId)
        {
            return await _dbContext.Invoices
                .Where(i => i.Id == invoiceId).Select(s => s)
                .Include(x => x.Reconciliations)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Creates the or update asynchronous.
        /// </summary>
        /// <param name="invoice">The invoice.</param>
        /// <returns>
        /// int
        /// </returns>
        public async Task<int> CreateOrUpdateAsync(Invoice invoice)
        {
            var res = -1;

            var dbInvoice = _dbContext.Invoices.Find(invoice.Id);

            if (dbInvoice == null)
            {
                if (invoice.Customer == null)
                {

                    // Set user Id
                    invoice.Customer = await _dbContext.Users.Where(u => u.StripeCustomerId == invoice.StripeCustomerId).FirstOrDefaultAsync();
                }

                if (invoice.Customer != null)
                {
                    _dbContext.Invoices.Add(invoice);
                    res = await _dbContext.SaveChangesAsync();
                }
            }
            else
            {
                dbInvoice.Paid = invoice.Paid;
                dbInvoice.Attempted = invoice.Attempted;
                dbInvoice.AttemptCount = invoice.AttemptCount;
                dbInvoice.NextPaymentAttempt = invoice.NextPaymentAttempt;
                dbInvoice.Closed = invoice.Closed;
                res = await _dbContext.SaveChangesAsync();
            }

            return res;
        }

        /// <summary>
        /// Gets all the invoices asynchronous.
        /// </summary>
        /// <returns>
        /// List of invoices.
        /// </returns>
        public async Task<List<Invoice>> GetInvoicesAsync()
        {
            var invoices = await _dbContext.Invoices.Include(i => i.Customer).Select(i => i).ToListAsync();

            return invoices;
        }


        public async Task<List<InvoiceRun>> ListInvoiceRunsAsync(SubscriptionInterval interval, InvoiceRunType types)
        {
            var result = await _dbContext.InvoiceRuns
                .OrderByDescending(r => r.PeriodEnd)
                .ToListAsync();
            if (types != InvoiceRunType.Both)
                result = result
                    .Where(r => (types.HasFlag(InvoiceRunType.Open) && !r.Closed) || (types.HasFlag(InvoiceRunType.Closed) && r.Closed))
                    .ToList();
            return result;                
        }

        
        public async Task<InvoiceRun> GetInvoiceRunAsync(int id)
        {
            return await _dbContext.InvoiceRuns
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<InvoiceRun> CreateOrUpdateRunAsync(InvoiceRun run)
        {
            if (run.Id == 0)
            {
                // Set the sequence number
                run.Sequence = await _dbContext.InvoiceRuns
                    .OrderByDescending(r => r.Sequence)
                    .Select(r => r.Sequence)
                    .FirstOrDefaultAsync() + 1;
                _dbContext.InvoiceRuns.Add(run);
                await _dbContext.SaveChangesAsync();
                return await _dbContext.InvoiceRuns
                    .Where(r => r.Id == run.Id)
                    .FirstAsync();
            }
            else
            {
                InvoiceRun cur = await _dbContext.InvoiceRuns
                    .Where(r => r.Id == run.Id)
                    .FirstAsync();
                cur.Closed = run.Closed;
                cur.Interval = run.Interval;
                cur.PeriodEnd = run.PeriodEnd;
                cur.PeriodStart = run.PeriodStart;
                cur.Sequence = run.Sequence;
                await _dbContext.SaveChangesAsync();
                return cur;
            }
        }

        public async Task CloseInvoiceRunAsync(int invoiceRunId)
        {
            InvoiceRun run = await _dbContext.InvoiceRuns
                .Where(r => r.Id == invoiceRunId)
                .FirstAsync();
            run.Closed = true;
            await _dbContext.SaveChangesAsync();
                
        }

        public async Task<List<Invoice>> ListInvoicesForRunAsync(int invoiceRunId)
        {
        return await _dbContext.Invoices
          .Where(i => i.InvoiceRun_Id == invoiceRunId)
          .Include(x => x.Reconciliations)
          .ToListAsync();
        }

        public async Task<List<Invoice>> GetInvoicesAsync(IEnumerable<int> ids)
        {
            List<Invoice> result = new List<Invoice>();
            foreach(int id in ids)
            {
                result.Add(await _dbContext.Invoices.Where(i => i.Id == id).Include(x => x.Reconciliations).SingleAsync());
            }
            return result;
        }

        public async Task<List<Invoice>> ListUnpaidInvoices()
        {
            return await _dbContext.Invoices
                .Where(i => i.Paid != true)
                .Where(i => i.Forgiven != true)
                .Include(x => x.Reconciliations)
                .ToListAsync();
        }
    }
}

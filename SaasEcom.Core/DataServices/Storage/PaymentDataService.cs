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
  public class PaymentDataService<TContext, TUser> : IPaymentDataService
    where TContext : IDbContext<TUser>
    where TUser : class
  {
    private TContext context;

    public PaymentDataService(TContext context)
    {
      this.context = context;
    }

    public async Task<bool> Import(Payment payment)
    {
      // Check to see if the payment already exists
      if (await context.Payments.Where(p =>
          p.Date == payment.Date
          && p.Amount == payment.Amount
          && p.Description == payment.Description
          && p.Particulars == payment.Particulars
          && p.Reference == payment.Reference
          && p.Balance == payment.Balance)
          .AnyAsync())
      {
        return false;
      }
      else
      {
        // Add it to the list of payments
        context.Payments.Add(payment);
        await context.SaveChangesAsync();
        return true;
      }
    }


    public async Task<IList<Payment>> ListForPeriodAsync(DateTime? start, DateTime? end)
    {
      return await context.Payments.Where(p =>
          (!start.HasValue || p.Date >= start.Value)
              && (!end.HasValue || p.Date < end.Value))
          .Include(x => x.Customer)
          .Include(x => x.Reconciliations)
          .ToListAsync();
    }

    public async Task<List<Payment>> ListForCustomerAsync(string customerId)
    {
      return await context.Payments.Where(p =>
          p.CustomerId == customerId)
          .Include(x => x.Customer)
          .Include(x => x.Reconciliations)
          .ToListAsync();
    }

    public async Task<List<Payment>> ListUnmatchedAsync()
    {
      return await context.Payments.Where(x =>
          x.CustomerId == null || x.CustomerId == "")
          .Include(x => x.Reconciliations)
          .ToListAsync();
    }

    public async Task SaveAsync(Payment payment)
    {
      if (payment.Id != 0)
      {
        Payment cur = await context.Payments.FirstOrDefaultAsync(x => x.Id == payment.Id);
        if (cur != payment)
        {
          cur.Amount = payment.Amount;
          cur.Balance = payment.Balance;
          cur.Customer = payment.Customer;
          cur.CustomerId = payment.CustomerId;
          cur.Date = payment.Date;
          cur.Description = payment.Description;
          cur.Particulars = payment.Particulars;
          cur.Reference = payment.Reference;
        }
      }
      else
      {
        context.Payments.Add(payment);
      }

      await context.SaveChangesAsync();
    }


    public async Task<Payment> GetAsync(int id)
    {
      return await context.Payments.Where(p => p.Id == id)
          .Include(p => p.Customer)
          .FirstOrDefaultAsync();
    }

    public async Task<Payment> GetLastImported()
    {
      return await context.Payments
          .OrderByDescending(p => p.Date)
          .Where(p => p.Balance != null)
          .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Payment>> ListUnacknowledgedAsync()
    {
      return await context.Payments
          .Where(p => !p.Acknowledged && p.Balance != null && p.CustomerId != null)
          .Include(p => p.Customer)
          .ToListAsync();
    }
  }
}
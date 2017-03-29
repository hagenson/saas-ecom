using AutoMapper;
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
  public class OneOffChargeDataService<TContext, TUser> : IOneOffChargeDataService
    where TContext : IDbContext<TUser>
    where TUser : SaasEcomUser
  {
    private TContext context;
    private IMapper mapper;

    public OneOffChargeDataService(TContext context, IMapper mapper)
    {
      this.context = context;
      this.mapper = mapper;
    }

    public async Task<List<OneOffCharge>> ListUninvoicedChargesForUserAsync(string userId)
    {
      return await context.OneOffCharges.Where(c => c.CustomerId == userId && c.InvoiceId == null)
        .ToListAsync();
    }

    public async Task SaveAsync(OneOffCharge charge)
    {
      if (charge.Id == 0)
      {
        context.OneOffCharges.Add(charge);
      }
      else
      {
        var cur = await context.OneOffCharges.FirstAsync(x => x.Id == charge.Id);
        mapper.Map(charge, cur);
      }

      await context.SaveChangesAsync();
     
    }
  }
}

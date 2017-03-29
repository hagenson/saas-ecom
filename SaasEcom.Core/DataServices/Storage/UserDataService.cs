using SaasEcom.Core.DataServices.Interfaces;
using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SaasEcom.Core.DataServices.Storage
{
  public class UserDataService<TContext, TUser> : IUserDataService
    where TContext : IDbContext<TUser>
    where TUser : class
  {

    public UserDataService(TContext db)
    {
      this.db = db;
    }

    private TContext db;

    public async Task<List<SaasEcomUser>> GetAllAsync()
    {
      return await db.Users.Cast<SaasEcomUser>().ToListAsync();
    }


    public async Task<SaasEcomUser> GetAsync(string userId)
    {
      return await db.Users.Cast<SaasEcomUser>()
          .Where(u => u.Id == userId)
          .SingleAsync();
    }

    public async Task SaveAsync(SaasEcomUser user)
    {
      SaasEcomUser dst = null;
      if (user.Id != null)
      {
        dst = await db.Users.Cast<SaasEcomUser>()
            .Where(u => u.Id == user.Id)
            .SingleAsync();
        foreach (PropertyInfo prop in dst.GetType().GetProperties())
        {
          if (prop.PropertyType.IsPrimitive || prop.PropertyType.IsEnum || typeof(string).IsAssignableFrom(prop.PropertyType))
          {
            object val = prop.GetValue(user);
            prop.SetValue(dst, val);
          }
        }
        if (dst.SecurityStamp == null)
          dst.SecurityStamp = Guid.NewGuid().ToString();
      }
      else
      {
        user.Id = user.FirstName + " " + user.LastName;

        if (user.SecurityStamp == null)
          user.SecurityStamp = Guid.NewGuid().ToString();

        if (user.AccountNumber == null)
        {
          // Get a new account number
          var nextNo = db.Settings.FirstOrDefault(s => s.Key == accountNoKey);
          if (nextNo == null)
          {
            nextNo = new Setting { Key = accountNoKey, Value = "1" };
            db.Settings.Add(nextNo);
          }
          int accNo = int.Parse(nextNo.Value);

          var noFormat = db.Settings.FirstOrDefault(s => s.Key == accountFmtKey);
          if (noFormat == null)
          {
            noFormat = new Setting { Key = accountFmtKey, Value = "{0:000000}" };
            db.Settings.Add(noFormat);
          }
          user.AccountNumber = String.Format(noFormat.Value, accNo);
          accNo++;
          nextNo.Value = accNo.ToString();
        }
        db.Users.Add(user as TUser);
      }

      await db.SaveChangesAsync();
    }

    private const string accountNoKey = "user.nextAccountNumber";
    private const string accountFmtKey = "user.accountNumberFormat";

  }
}

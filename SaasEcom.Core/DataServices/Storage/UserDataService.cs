using SaasEcom.Core.DataServices.Interfaces;
using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Net;

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
          .SingleOrDefaultAsync();
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

    public async Task<SaasEcomUser> GetByIpAsync(string ip)
    {
      IPAddress addr = IPAddress.Parse(ip);
      IPAddress fullMask = IPAddress.Parse("255.255.255.255");

      var users = await db.Users.Cast<SaasEcomUser>().ToListAsync();
      return users.FirstOrDefault(u =>
      {
        if (!String.IsNullOrWhiteSpace(u.IPAddress))
        {
          try
          {
            IPAddress cur;
            IPAddress mask;
            string[] parts = u.IPAddress.Split('/');
            if (parts.Length > 1)
            {
              cur = IPAddress.Parse(parts[0]);
              long netmask = long.Parse(parts[1]);
              netmask = 32 - netmask;
              netmask = (long)Math.Pow(2, netmask) - 1;
              mask = new IPAddress(netmask);
            }
            else
            {
              cur = IPAddress.Parse(u.IPAddress);
              mask = fullMask;
            }

            var left = GetNetworkAddress(addr, mask);
            var right = GetNetworkAddress(cur, mask);
            return left.Equals(right);
          }
          catch
          {
            return false;
          }
        }
        return false;
      });
    }

    private static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
    {
      byte[] ipAdressBytes = address.GetAddressBytes();
      byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

      if (ipAdressBytes.Length != subnetMaskBytes.Length)
        throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

      byte[] broadcastAddress = new byte[ipAdressBytes.Length];
      for (int i = 0; i < broadcastAddress.Length; i++)
      {
        broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
      }
      return new IPAddress(broadcastAddress);
    }

    private const string accountNoKey = "user.nextAccountNumber";
    private const string accountFmtKey = "user.accountNumberFormat";

  }
}

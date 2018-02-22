using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SaasEcom.Core.DataServices.Interfaces;
using SaasEcom.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Infrastructure.Facades
{
  public class UserFacade
  {
    public UserFacade(DbContext db, IUserDataService users)
    {
      this.db = db;
      this.users = users;
    }

    private IUserDataService users;

    private DbContext db;

    public async Task<List<SaasEcomUser>> ListUsersAsync()
    {
      return await users.GetAllAsync();
    }

    public async Task SetPassword(string userId, string newPassword)
    {
      SaasEcomUser user = await users.GetAsync(userId);
      UserStore<SaasEcomUser> us = new UserStore<SaasEcomUser>(db);
      UserManager<SaasEcomUser> um = new UserManager<SaasEcomUser>(us);
      user.PasswordHash = um.PasswordHasher.HashPassword(newPassword);
      await users.SaveAsync(user);
    }

    public async Task Save(SaasEcomUser user)
    {
      await users.SaveAsync(user);
    }

    public async Task<List<SaasEcomUser>> ListOverdueUsersAsync()
    {
      return await users.GetAllAsync();
    }
  }
}

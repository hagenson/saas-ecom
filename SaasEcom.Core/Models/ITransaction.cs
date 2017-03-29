using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.Models
{
  public interface ITransaction
  {
    int Id { get; }

    int Amount { get; }

    DateTime Date { get; }
  }
}

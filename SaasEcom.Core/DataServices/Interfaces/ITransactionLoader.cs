using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.DataServices.Interfaces
{
  public interface ITransactionLoader
  {
    Stream GetTransactions(DateTime from, DateTime to);
  }
}

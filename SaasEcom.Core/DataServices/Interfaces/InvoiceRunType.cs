using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core.DataServices.Interfaces
{
    [Flags]
    public enum InvoiceRunType
    {
        Open = 1,
        Closed = 2,
        Both = Open | Closed
    }
}

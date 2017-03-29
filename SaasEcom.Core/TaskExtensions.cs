using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasEcom.Core
{
    public static class TaskExtensions
    {
        public static T Await<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Collectors.Helpers
{
    public  interface IGenericAsyncSolver<TIn, TOut>
    {
        public Task<TOut?> Solve(TIn item);
    }
}

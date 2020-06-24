using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthesisAPI.Utilities
{
    /// <summary>
    /// A type intended for use as a return, encapsulates both success and failure states
    /// </summary>
    /// <typeparam name="V">Value type of the return</typeparam>
    /// <typeparam name="V>Possible error type of the function</typeparam>
    public class Result<V, E> where E: Exception
    {

        public Result(V val) => value = val;

        public Result(E err)
        {
            error = err;
            isError = true;
        }

        public static implicit operator V(Result<V, E> res)
        {
            if (!res.isError)
                return res.value;
            throw res.error;
        }

        public static implicit operator E(Result<V, E> res)
        {
            if (!res.isError)
                throw new Exception("Result is not an error type");
            return res.error;
        }

        public Result<V2, E> MapResult<V2>(Func<V, V2> f)
        {
            if(isError)
            {
                return new Result<V2,E>(error);
            } 
            return new Result<V2,E>(f(value));
        }

        public Result<V, E2> MapError<E2>(Func<E, E2> f) where E2 : Exception
        {
            if(!isError)
            {
                return new Result<V,E2>(value);
            }
            return new Result<V,E2>(f(error));
        }

        public Result<V2, E2> Map<V2, E2>(Func<V, V2> f, Func<E, E2> g) where E2 : Exception => 
            MapResult(f).MapError(g);

        private readonly bool isError;
        private readonly V value = default!;
        private readonly E error = default!;
    }
}

﻿using SynthesisAPI.VirtualFileSystem;
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
    /// <typeparam name="TValue">Value type of the return</typeparam>
    /// <typeparam name="TError">Possible error type of the function</typeparam>
    public class Result<TValue, TError> where TError: Exception
    {

        public Result(TValue val) => value = val;

        public Result(TError err)
        {
            error = err;
            isError = true;
        }

        public TValue GetResult()
        {
            if (!isError)
                return value;
            throw error;
        }

        public TError GetError()
        {
            if (!isError)
                throw new SynthesisException("Result is not an error type");
            return error;
        }

        public static implicit operator TValue(Result<TValue, TError> res)
        {
            return res.GetResult();
        }

        public static implicit operator TError(Result<TValue, TError> res)
        {
            return res.GetError();
        }

        public Result<TNewValue, TError> MapResult<TNewValue>(Func<TValue, TNewValue> f)
        {
            if(isError)
            {
                return new Result<TNewValue,TError>(error);
            }
            return new Result<TNewValue,TError>(f(value));
        }

        public Result<TValue, TNewError> MapError<TNewError>(Func<TError, TNewError> f) where TNewError : Exception
        {
            if(!isError)
            {
                return new Result<TValue,TNewError>(value);
            }
            return new Result<TValue,TNewError>(f(error));
        }

        public Result<TNewValue, TNewError> Map<TNewValue, TNewError>(Func<TValue, TNewValue> f,
            Func<TError, TNewError> g) where TNewError : Exception =>
            MapResult(f).MapError(g);

        public readonly bool isError;
        private readonly TValue value = default!;
        private readonly TError error = default!;
    }
}

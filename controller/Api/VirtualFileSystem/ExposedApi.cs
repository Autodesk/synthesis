using System;

namespace SynthesisAPI.Utilities
{
    /// <summary>
    /// Marks a function as exposed in the API. 
    /// 
    /// Be sure to mark the API call source as external before calling any internal functions
    /// 
    /// For example:
    /// internal void FunctionInner() { }
    /// 
    /// [ExposedApi]
    /// public void Function() {
    ///     using var _ = ApiCallSource.ExternalCall();
    ///     FunctionInner();
    /// }
    /// </summary>
    public class ExposedApi : Attribute { }
}

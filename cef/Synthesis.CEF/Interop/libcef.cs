namespace Synthesis.CEF.Interop {
    internal static unsafe partial class libcef {
        internal const string DllName = "libcef";
        internal const CallingConvention CEF_CALL = CallingConvention.Cdecl;

        [DllImport(DllName, EntryPoint = "cef_api_hash", CallingConvention = CEF_CALL)]
        public static extern sbyte* api_hash(int entry);
    }
}

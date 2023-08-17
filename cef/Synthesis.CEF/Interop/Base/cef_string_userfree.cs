// Not auto generated but taken directly from CefGlue.

namespace Synthesis.CEF.Interop {
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// It is sometimes necessary for the system to allocate string structures with
    /// the expectation that the user will free them. The userfree types act as a
    /// hint that the user is responsible for freeing the structure.
    /// </summary>
    /// <remarks>
    /// <c>cef_string_userfree*</c> === <c>cef_string_userfree_t</c>.
    /// </remarks>
    internal unsafe struct cef_string_userfree {
        public static string ToString(cef_string_userfree *str) {
            if (str != null) {
                var result = cef_string_t.ToString((cef_string_t *) str);
                libcef.string_userfree_free(str);
                return result;
            }

            return null;
        }
    }
}

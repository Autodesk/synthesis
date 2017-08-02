using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GopherAPI.STL
{
    public struct STLFile
    {
        public readonly Facet[] Facets;
        public readonly System.Drawing.Color Color;
        public readonly bool IsDefault;

        public STLFile(Facet[] facets, System.Drawing.Color color, bool isDefault)
        {
            Facets = facets;
            Color = color;
            IsDefault = isDefault;
        }
    }
}

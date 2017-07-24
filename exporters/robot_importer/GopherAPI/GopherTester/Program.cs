using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GopherAPI.Reader;
using GopherAPI.STL;
using GopherAPI.Properties;
using GopherAPI.Writer;
using GopherAPI.Other;
using System.Drawing;
using GopherAPI;
using System.IO;
using System.Diagnostics;

namespace GopherTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Field field = new Field();

            List<Facet> facets = new List<Facet>();
            for(int i = 0; i < 10; i++)
            {
                facets.Add(new Facet(new Vec3(10, 32, 49), new Vec3(324, 231, 23), new Vec3(12, 34, 123), new Vec3(32, 123, 43)));
            }
            List<STLAttribute> attributes = new List<STLAttribute> { new STLAttribute(AttribType.BOX_COLLIDER, 2, 50.0f, false, null, 10f, 15f, 155, null) };
            List<Joint> joints = new List<Joint> { };

            Mesh mesh = new Mesh(10, facets.ToArray(), SystemColors.GrayText, false, 2, new TransformationMatrix());
            field.Meshes.Add(mesh);
            field.Attributes.AddRange(attributes);

            GopherScribe.WriteField(field, new FileStream(@"C:\users\t_howab\desktop\field1.field", FileMode.CreateNew));

            FieldReader reader = new FieldReader(@"C:\users\t_howab\desktop\field1.field");

            reader.PreProcess();


        }
    }
}
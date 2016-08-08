using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Simulation_RD.Scenes
{
    class MenuScene : GameWindow
    {

        public MenuScene() : base(1500, 768, new OpenTK.Graphics.GraphicsMode(), "RnD Stuff")
        {
            VSync = VSyncMode.Off;
        }

        protected override void OnLoad(EventArgs e)
        {

        }
    }
}

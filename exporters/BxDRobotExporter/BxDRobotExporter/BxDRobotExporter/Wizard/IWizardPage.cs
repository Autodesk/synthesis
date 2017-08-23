using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BxDRobotExporter.Wizard
{
    public delegate void InvalidatePageEventHandler(Type PageType);

    public interface IWizardPage
    {
        void OnNext();
        void Initialize();

        bool Initialized { get; set; }

        event Action DeactivateNext;
        event Action ActivateNext;

        event InvalidatePageEventHandler InvalidatePage;
    }
}

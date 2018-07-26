using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BxDRobotExporter.Wizard
{
    public delegate void InvalidatePageEventHandler(Type PageType);

    /// <summary>
    /// Interface implemented by every page of the wizard
    /// </summary>
    public interface IWizardPage
    {
        /// <summary>
        /// Called when the next button is clicked
        /// </summary>
        void OnNext();
        /// <summary>
        /// Called the first time a page is shown.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Gets or sets whether <see cref="Initialize()"/> should be called
        /// </summary>
        bool Initialized { get; set; }

        event Action DeactivateNext;
        event Action ActivateNext;

        event InvalidatePageEventHandler InvalidatePage;
    }
}

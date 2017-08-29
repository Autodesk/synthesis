using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BxDRobotExporter.Wizard
{
    /// <summary>
    /// Primary <see cref="Control"/> of the whole wizard and the only object in <see cref="WizardForm.components"/>
    /// </summary>
    public partial class WizardPageControl : UserControl
    {
        /// <summary>
        /// Invoked when the <see cref="WizardNavigator.WizardNavigatorState"/> is set to <see cref="WizardNavigator.WizardNavigatorState.FinishEnabled"/> and <see cref="WizardNavigator.NextButton"/> is clicked.
        /// </summary>
        public event Action FinishClicked;

        public WizardPageControl()
        {
            InitializeComponent();

            this.WizardNavigator.NextButton.Click += NextButton_Click;
            this.WizardNavigator.BackButton.Click += BackButton_Click;
        }

        /// <summary>
        /// Invoked when <see cref="WizardNavigator.BackButton"/> is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, EventArgs e)
        {
            if (ActivePageIndex > 0)
            {
                ActivePageIndex--;
                for (int i = 1; i < Controls.Count; i++)
                {
                    if (i == ActivePageIndex + 1)
                        Controls[i].Visible = true;
                    else
                        Controls[i].Visible = false;
                }
                WizardNavigator.UpdateState(defaultNavigatorStates[ActivePageIndex + 1]);
                if (!((IWizardPage)(this.Controls[ActivePageIndex + 1])).Initialized)
                    ((IWizardPage)(this.Controls[ActivePageIndex + 1])).Initialize();
            }
        }

        /// <summary>
        /// Invoked when <see cref="WizardNavigator.NextButton"/> is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextButton_Click(object sender, EventArgs e)
        {
            try
            {
                ((IWizardPage)(this.Controls[ActivePageIndex + 1])).OnNext();
                if (ActivePageIndex < Controls.Count - 1 && WizardNavigator.NextButton.Text == "Next >" || WizardNavigator.NextButton.Text == "Start >")
                {
                    ActivePageIndex++;
                    for (int i = 1; i < Controls.Count; i++)
                    {
                        if (i == ActivePageIndex + 1)
                            Controls[i].Visible = true;
                        else
                            Controls[i].Visible = false;
                    }
                    WizardNavigator.UpdateState(defaultNavigatorStates[ActivePageIndex + 1]);
                    if (!((IWizardPage)(this.Controls[ActivePageIndex + 1])).Initialized)
                        ((IWizardPage)(this.Controls[ActivePageIndex + 1])).Initialize();
                }
                else if (WizardNavigator.NextButton.Text == "Finish")
                    FinishClicked?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
        }

        private int ActivePageIndex = 0;

        /// <summary>
        /// Dictionary containing all of the page indices and their respective default navigator states.
        /// </summary>
        private Dictionary<int, WizardNavigator.WizardNavigatorState> defaultNavigatorStates = new Dictionary<int, WizardNavigator.WizardNavigatorState>();

        /// <summary>
        /// Adds a page to the <see cref="WizardPageControl"/>. Throws an exception if the given <see cref="UserControl"/> does not implement <see cref="IWizardPage"/>
        /// </summary>
        /// <param name="page"></param>
        /// <param name="defaultState"></param>
        public void Add(UserControl page, WizardNavigator.WizardNavigatorState defaultState = WizardNavigator.WizardNavigatorState.Clean)
        {
            if (!(page is IWizardPage))
                throw new ArgumentException("ERROR: Given page does not extend IWizardPage", "page");
            page.Location = new Point(0, 0);
            page.Top = 0;
            page.Left = 0;
            page.Visible = false;
            page.BackColor = Color.Transparent;
            defaultNavigatorStates.Add(Controls.Count, defaultState);
            Controls.Add(page);

            ((IWizardPage)page).InvalidatePage += WizardPageControl_InvalidatePage;
        }

        /// <summary>
        /// Adds several pages to the <see cref="WizardPageControl"/>. Throws an exception if the given <see cref="UserControl"/>s do not implement <see cref="IWizardPage"/>
        /// </summary>
        /// <param name="pages"></param>
        /// <param name="defaultStates"></param>
        public void AddRange(UserControl[] pages, WizardNavigator.WizardNavigatorState[] defaultStates = null)
        {
            foreach (UserControl page in pages)
            {
                if (!(page is IWizardPage))
                    throw new ArgumentException("ERROR: Given page does not extend IWizardPage", "page");
                page.Location = new Point(0, 0);
                page.Top = 0;
                page.Left = 0;
                page.Visible = false;
                if (page.BackColor == Control.DefaultBackColor) page.BackColor = Color.Transparent;
                if (defaultStates == null)
                    defaultNavigatorStates.Add(Controls.Count, WizardNavigator.WizardNavigatorState.Clean);
                else
                    defaultNavigatorStates.Add(Controls.Count, defaultStates[pages.ToList().IndexOf(page)]);
                Controls.Add(page);
                ((IWizardPage)page).InvalidatePage += WizardPageControl_InvalidatePage;

            }
        }

        /// <summary>
        /// Invalidates a page of the given type so that <see cref="IWizardPage.Initialize"/> will be invoked next time it is visible.
        /// </summary>
        /// <param name="PageType"></param>
        private void WizardPageControl_InvalidatePage(Type PageType)
        {
            if (PageType != null)
            {
                foreach (var page in Controls)
                {
                    if (page.GetType() == PageType)
                    {
                        ((IWizardPage)page).Initialized = false;
                    }
                }
            }
        }

        /// <summary>
        /// Loads the first page and begins the wizard.
        /// </summary>
        public void BeginWizard()
        {
            Controls[1].Visible = true;
            WizardNavigator.UpdateState(defaultNavigatorStates[1]);
        }
    }
}
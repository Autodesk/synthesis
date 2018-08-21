using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InternalFieldExporter.Wizard
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

        private List<UserControl> pageControls = new List<UserControl>();

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
                // Hide current page
                pageControls[ActivePageIndex].Visible = false;

                ActivePageIndex--;

                // Initialize the next page
                if (!((IWizardPage)(pageControls[ActivePageIndex])).Initialized)
                    ((IWizardPage)(pageControls[ActivePageIndex])).Initialize();

                // Show previous page
                pageControls[ActivePageIndex].Visible = true;
                WizardNavigator.UpdateState(defaultNavigatorStates[ActivePageIndex]);
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
                ((IWizardPage)(pageControls[ActivePageIndex])).OnNext();

                if (ActivePageIndex < pageControls.Count - 1 && (WizardNavigator.NextButton.Text == "Next" || WizardNavigator.NextButton.Text == "Start"))
                {
                    // Hide current page
                    pageControls[ActivePageIndex].Visible = false;

                    ActivePageIndex++;

                    // Initialize the next page
                    if (!((IWizardPage)(pageControls[ActivePageIndex])).Initialized)
                        ((IWizardPage)(pageControls[ActivePageIndex])).Initialize();

                    // Show next page
                    pageControls[ActivePageIndex].Visible = true;
                    WizardNavigator.UpdateState(defaultNavigatorStates[ActivePageIndex]);
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

        /// <summary>
        /// When set to true, makes the next button skip the next form, or finish if only one form follows.
        /// </summary>
        public bool EndEarly
        {
            set
            {
                if (value)
                    WizardNavigator.UpdateState(WizardNavigator.WizardNavigatorState.FinishEnabled | defaultNavigatorStates[ActivePageIndex]);
                else
                    WizardNavigator.UpdateState(defaultNavigatorStates[ActivePageIndex]);
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
            page.Dock = DockStyle.Fill;

            MainLayout.Controls.Add(page);
            MainLayout.SetRow(page, 0);
            MainLayout.SetColumn(page, 0);

            pageControls.Add(page);
            defaultNavigatorStates.Add(pageControls.Count - 1, defaultState);

            ((IWizardPage)page).InvalidatePage += WizardPageControl_InvalidatePage;
        }

        /// <summary>
        /// Adds several pages to the <see cref="WizardPageControl"/>. Throws an exception if the given <see cref="UserControl"/>s do not implement <see cref="IWizardPage"/>
        /// </summary>
        /// <param name="pages"></param>
        /// <param name="defaultStates"></param>
        public void AddRange(UserControl[] pages, WizardNavigator.WizardNavigatorState[] defaultStates = null)
        {
            for (int page = 0; page < pages.Length; page++)
            {
                Add(pages[page], defaultStates[page]);
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
                foreach (UserControl page in pageControls)
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
            // Initialize first page
            if (!((IWizardPage)(pageControls[0])).Initialized)
                ((IWizardPage)(pageControls[0])).Initialize();

            // Open first page
            pageControls[0].Visible = true;
            WizardNavigator.UpdateState(defaultNavigatorStates[0]);
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inventor;

namespace FieldExporter.Components
{
    public partial class FieldMetaForm : UserControl
    {
        bool selectingPoints = false;
        InteractionEvents interactionEvents;
        SelectEvents selectEvents;
        List<dynamic> occurrences = new List<dynamic>();

        public FieldMetaForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Enables interaction events with Inventor.
        /// </summary>
        public void EnableInteractionEvents()
        {
            if (Program.INVENTOR_APPLICATION.ActiveDocument == Program.ASSEMBLY_DOCUMENT)
            {
                try
                {
                    interactionEvents = Program.INVENTOR_APPLICATION.CommandManager.CreateInteractionEvents();
                    interactionEvents.OnActivate += interactionEvents_OnActivate;
                    interactionEvents.Start();

                    selectCoordinatesButton.Text = "Done";

                    selectingPoints = true;
                }
                catch
                {
                    MessageBox.Show("Cannot enter select mode.", "Document not found.");
                }
            }
            else
            {
                MessageBox.Show("Can only enter select mode for " + Program.ASSEMBLY_DOCUMENT.DisplayName);
            }
        }

        /// <summary>
        /// Disables interaction events with Inventor.
        /// </summary>
        public void DisableInteractionEvents()
        {
            interactionEvents.Stop();
            Program.ASSEMBLY_DOCUMENT.SelectSet.Clear();

            selectCoordinatesButton.Text = "Select Points";
            selectingPoints = false;
        }

        /// <summary>
        /// Enables select events when interaction events are activated.
        /// </summary>
        private void interactionEvents_OnActivate()
        {
            selectEvents = interactionEvents.SelectEvents;
            selectEvents.AddSelectionFilter(SelectionFilterEnum.kAllPointEntities);
            selectEvents.OnSelect += selectEvents_OnSelect;
        }
        
        private void selectEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {}

        private void getPointSelections()
        {
            Cursor.Current = Cursors.WaitCursor;
            Program.LockInventor();
            selectCoordinatesButton.Enabled = false;

            occurrences.Clear();
            foreach (dynamic selection in selectEvents.SelectedEntities)
                occurrences.Add(selection);

            selectCoordinatesButton.Enabled = true;
            Program.UnlockInventor();
            Cursor.Current = Cursors.Default;
            DisableInteractionEvents();
        }

        private void selectCoordinatesButton_Click(object sender, EventArgs e)
        {
            if (selectingPoints)
            {
                getPointSelections();
            }
            else
            {
                EnableInteractionEvents();
            }
        }
    }
}

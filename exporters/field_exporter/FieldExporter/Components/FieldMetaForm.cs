using Inventor;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FieldExporter.Components
{
    public partial class FieldMetaForm : UserControl
    {
        bool selectingPoints = false;
        InteractionEvents interactionEvents;
        SelectEvents selectEvents;
        static List<BXDVector3> spawnpoints = new List<BXDVector3>();

        public FieldMetaForm()
        {
            InitializeComponent();
        }

        public static BXDVector3[] getSpawnpoints()
        {
            return spawnpoints.ToArray();
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
            selectEvents.AddSelectionFilter(SelectionFilterEnum.kSketchPointFilter);
            selectEvents.OnSelect += selectEvents_OnSelect;
        }
        
        private void selectEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {}

        private void getPointSelections()
        {
            Cursor.Current = Cursors.WaitCursor;
            Program.LockInventor();
            selectCoordinatesButton.Enabled = false;

            spawnpoints.Clear();

            foreach (dynamic selectedEntity in selectEvents.SelectedEntities)
                if (selectedEntity is SketchPoint point)
                    spawnpoints.Add(new BXDVector3(point.Geometry3d.X, point.Geometry3d.Y + 50, point.Geometry3d.Z));

            selectCoordinatesButton.Enabled = true;
            Program.UnlockInventor();
            Cursor.Current = Cursors.Default;
            DisableInteractionEvents();
        }

        private void updatePointView()
        {
            spawnPointList.Items.Clear();

            foreach (BXDVector3 point in spawnpoints)
            {
                spawnPointList.Items.Add(point.ToString());
            }
        }

        private void selectCoordinatesButton_Click(object sender, EventArgs e)
        {
            if (selectingPoints)
            {
                getPointSelections();
                updatePointView();
            }
            else
            {
                EnableInteractionEvents();
            }
        }
    }
}

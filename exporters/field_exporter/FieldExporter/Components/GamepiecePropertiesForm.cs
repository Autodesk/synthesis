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
    public partial class GamepiecePropertiesForm : UserControl
    {
        public GamepiecePropertiesForm()
        {
            InitializeComponent();
        }

        public bool IsGamepiece
        {
            get => gamepieceCheckBox.Checked;
            set => gamepieceCheckBox.Checked = value;
        }

        /// <summary>
        /// Returns the configured gamepiece settings, or null if not a gamepiece.
        /// </summary>
        /// <returns></returns>
        public Exporter.Gamepiece GetGamepiece(string id)
        {
            if (gamepieceCheckBox.Checked)
            {
                if (holdingLimitCheckBox.Checked)
                    return new Exporter.Gamepiece(id, Spawnpoint, (uint)holdingLimitUpDown.Value);
                else
                    return new Exporter.Gamepiece(id, Spawnpoint);
            }
            else
                return null;
        }

        private BXDVector3 _spawnpoint;
        BXDVector3 Spawnpoint
        {
            get => _spawnpoint;
            set
            {
                if (InvokeRequired)
                {
                    Invoke((Action<BXDVector3>)((BXDVector3 v) => Spawnpoint = v), value);
                    return;
                }

                _spawnpoint = value;
                spawnpointLabel.Text = "Spawnpoint: [" + value.x.ToString("N1") + ',' + value.y.ToString("N1") + ',' + value.z.ToString("N1") + ']';
            }
        }
        
        InteractionEvents interactionEvents;
        SelectEvents selectEvents;

        private void interactionEvents_OnActivate()
        {
            selectEvents = interactionEvents.SelectEvents;
            selectEvents.AddSelectionFilter(SelectionFilterEnum.kSketchPointFilter);
            selectEvents.OnSelect += selectEvents_OnSelect;
        }

        private void selectEvents_OnSelect(ObjectsEnumerator justSelectedEntities, SelectionDeviceEnum selectionDevice, Inventor.Point modelPosition, Point2d viewPosition, Inventor.View view)
        {
            foreach (dynamic selectedEntity in justSelectedEntities)
            {
                if (selectedEntity is SketchPoint point)
                {
                    Spawnpoint = new BXDVector3(point.Geometry3d.X, point.Geometry3d.Y, point.Geometry3d.Z);
                    DisableInteractionEvents();
                    break;
                }
            }
        }

        bool selectingSpawnpoint = false;

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

                    selectSpawnpointButton.Text = "Cancel";
                    selectingSpawnpoint = true;
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
            selectingSpawnpoint = false;

            if (InvokeRequired)
                Invoke((Action)(() => selectSpawnpointButton.Text = "Select"));
            else
                selectSpawnpointButton.Text = "Select";
        }

        private void selectSpawnpointButton_Click(object sender, EventArgs e)
        {
            if (!selectingSpawnpoint)
            {
                EnableInteractionEvents();
            }
            else
            {
                DisableInteractionEvents();
            }
        }

        private void gamepieceCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (gamepieceCheckBox.Checked)
            {
                spawnpointLabel.Enabled = true;
                selectSpawnpointButton.Enabled = true;
                holdingLimitCheckBox.Enabled = true;
            }
            else
            {
                if (selectingSpawnpoint)
                    DisableInteractionEvents();

                Spawnpoint = new BXDVector3(0, 0, 0);
                spawnpointLabel.Enabled = false;
                selectSpawnpointButton.Enabled = false;

                holdingLimitCheckBox.Checked = false;
                holdingLimitCheckBox.Enabled = false;
            }
        }

        private void holdingLimitCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (holdingLimitCheckBox.Checked)
            {
                holdingLimitUpDown.Enabled = true;
                holdingLimitUpDown.Value = 1;
                holdingLimitUpDown.Minimum = 1;
            }
            else
            {
                holdingLimitUpDown.Enabled = false;
                holdingLimitUpDown.Minimum = 0;
                holdingLimitUpDown.Value = 0;
            }
        }
    }
}

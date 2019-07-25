using System;
using System.Windows.Forms;

namespace BxDRobotExporter.Utilities
{
    internal static class WinFormsUtils
    {
        /// <summary>
        /// Adds a control to a new row at the end of the table.
        /// </summary>
        /// <param name="control">Control to append to table.</param>
        /// <param name="table">Table to add control to.</param>
        /// <param name="rowStyle">Style of the new row. Autosized if left null.</param>
        public static void AddControlToNewTableRow(Control control, TableLayoutPanel table, RowStyle rowStyle = null)
        {
            if (rowStyle == null)
            {
                rowStyle = new RowStyle();
                rowStyle.SizeType = SizeType.AutoSize;
            }

            table.RowCount++;
            table.RowStyles.Add(rowStyle);
            table.Controls.Add(control);
            table.SetRow(control, table.RowCount - 1);
            table.SetColumn(control, 0);
        }

        /// <summary>
        /// Disables scroll selection on NumericUpDown and ComboBox type
        /// </summary>
        public static void DisableScrollSelection(Control control) // TODO: WinForms util class
        {
            if (control is NumericUpDown || control is ComboBox)
            {
                control.MouseWheel += (o, e) => ((HandledMouseEventArgs) e).Handled = true;
            }

            foreach (Control subControl in control.Controls)
            {
                DisableScrollSelection(subControl);
            }
        }

        public static void AddChangeListener(Control control, EventHandler @event) // TODO: WinForms util class
        {
            switch (control)
            {
                case NumericUpDown down:
                    down.ValueChanged += @event;
                    break;
                case ComboBox box:
                    box.SelectedIndexChanged += @event;
                    break;
                case CheckBox box:
                    box.CheckedChanged += @event;
                    break;
                case RadioButton box:
                    box.CheckedChanged += @event;
                    break;
            }
        }
    }
}
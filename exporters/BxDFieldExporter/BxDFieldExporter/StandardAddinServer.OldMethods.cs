using System;
using System.Runtime.InteropServices;
using Inventor;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Timers;
using ExportProcess;
using System.Diagnostics;
using FieldExporter;

namespace BxDFieldExporter
{
    public partial class StandardAddInServer
    {
        ////Removes an assembly from the arraylist of the Component
        //public void RemoveAssembly_OnExecute(NameValueMap Context)
        //{
        //    SetAllButtons(true);
        //    try
        //    {
        //        runOnce = true;
        //        bool found = false;
        //        bool selected = false;
        //        foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// looks over all of the nodes
        //        {
        //            if (node.Selected)
        //            {
        //                foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
        //                {
        //                    if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
        //                    {
        //                        selected = true;// if one node is selected then we can add the new sub assembly
        //                    }
        //                }
        //            }
        //        }
        //        if (selected)// if there is a selected node then we can add a part to it
        //        {
        //            done = false;
        //            while (!done)
        //            {
        //                ComponentOccurrence joint = null;
        //                AssemblyDocument asmDoc = (AssemblyDocument)InventorApplication.ActiveDocument;
        //                joint = (ComponentOccurrence)InventorApplication.CommandManager.Pick// have the user select a leaf occurrence or part
        //                          (SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select an assembly to remove");
        //                if (joint != null)
        //                {
        //                    foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
        //                    {
        //                        if (node.Selected)// find the selected node
        //                        {
        //                            foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
        //                            {
        //                                if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
        //                                {
        //                                    if (t.CompOccs.Contains(joint))
        //                                    {// if the occurence is in the list the allow the remove
        //                                        t.CompOccs.Remove(joint);// add the occurence to the arraylist
        //                                        InventorApplication.ActiveDocument.SelectSet.Clear();
        //                                        node.DoSelect();
        //                                        found = true;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                if (!found)
        //                {
        //                    MessageBox.Show("Warning, assembly not found in item component");// if the assembly wasn't found in the component then tell the user
        //                }
        //                runOnce = false;
        //                found = false;
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("Please select a browser node to remove an assembly from");// if the user didn't select a browser node then tell them
        //        }
        //        runOnce = true;
        //    }
        //    catch (Exception)
        //    {

        //    }
        //    SetAllButtons(true);
        //}

        ////Removes a part from the arraylist of the Component
        //public void RemovePart_OnExecute(NameValueMap Context)
        //{
        //    SetAllButtons(false);
        //    try
        //    {
        //        runOnce = true;
        //        bool found = false;
        //        bool selected = false;
        //        foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// looks over all of the nodes
        //        {
        //            if (node.Selected)
        //            {
        //                foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
        //                {
        //                    if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
        //                    {
        //                        selected = true;// if one node is selected then we can add the new sub assembly
        //                    }
        //                }
        //            }
        //        }
        //        if (selected)// if there is a selected node then we can add a part to it
        //        {
        //            done = false;
        //            while (!done)
        //            {
        //                ComponentOccurrence joint = null;
        //                AssemblyDocument asmDoc = (AssemblyDocument)
        //                             InventorApplication.ActiveDocument;
        //                joint = (ComponentOccurrence)InventorApplication.CommandManager.Pick// have the user select a leaf occurrence or part
        //                          (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a part to remove");
        //                if (joint != null)
        //                {
        //                    foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
        //                    {
        //                        if (node.Selected)// find the selected node
        //                        {
        //                            foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
        //                            {
        //                                if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
        //                                {
        //                                    if (t.CompOccs.Contains(joint))// if the occurence is in the list the allow the remove
        //                                    {
        //                                        t.CompOccs.Remove(joint);// add the occurence to the arraylist
        //                                        InventorApplication.ActiveDocument.SelectSet.Clear();
        //                                        node.DoSelect();
        //                                        found = true;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                if (!found)
        //                {
        //                    MessageBox.Show("Warning, part not found in item component");// if the part wasn't found in the component then tell the user
        //                }
        //                found = false;
        //                runOnce = false;
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("Please select a browser node to remove a part from");// if the user didn't select a browser node then tell them
        //        }
        //        runOnce = true;
        //    }
        //    catch (Exception)
        //    {

        //    }
        //    SetAllButtons(true);
        //}

        ////Old Code for adding a single assembly
        //public void AddNewAssembly_OnExecute(NameValueMap Context)
        //{
        //    try
        //    {
        //        runOnce = true;
        //        done = false;
        //        bool selected = false;
        //        foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// looks over all of the nodes
        //        {
        //            if (node.Selected)
        //            {
        //                foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
        //                {
        //                    if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
        //                    {
        //                        selected = true;// if one node is selected then we can add the new sub assembly
        //                    }
        //                }
        //            }
        //        }
        //        if (selected)// if there is a selected node then we can add a part to it
        //        {
        //            while (!done)
        //            {
        //                ComponentOccurrence joint = null;
        //                AssemblyDocument asmDoc = (AssemblyDocument)
        //                         InventorApplication.ActiveDocument;
        //                joint = (ComponentOccurrence)InventorApplication.CommandManager.Pick// have the user select a leaf occurrence or part
        //                          (SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select an assembly to add");
        //                if (joint != null)
        //                {
        //                    foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
        //                    {
        //                        if (node.Selected)// find the selected node
        //                        {
        //                            foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
        //                            {
        //                                if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
        //                                {
        //                                    t.compOcc.Add(joint);// add the occurence to the arraylist
        //                                    InventorApplication.ActiveDocument.SelectSet.Clear();
        //                                    node.DoSelect();
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                runOnce = false;
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("Please select a browser node to add a part to");// if the user didn't select a browser node then tell them
        //        }
        //        runOnce = false;
        //    }
        //    catch (Exception)
        //    {

        //    }
        //    runOnce = true;

        //}

        ////Old Code for adding a single part
        //public void AddNewSubAssembly_OnExecute(NameValueMap Context)
        //{
        //    try
        //    {
        //        runOnce = true;
        //        done = false;
        //        bool selected = false;
        //        foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// looks over all of the nodes
        //        {
        //            if (node.Selected)
        //            {
        //                foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
        //                {
        //                    if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
        //                    {
        //                        selected = true;// if one node is selected then we can add the new sub assembly

        //                    }
        //                }
        //            }
        //        }
        //        if (selected)// if there is a selected node then we can add a part to it
        //        {
        //            while (!done)
        //            {
        //                ComponentOccurrence joint = null;
        //                AssemblyDocument asmDoc = (AssemblyDocument)
        //                             InventorApplication.ActiveDocument;
        //                joint = (ComponentOccurrence)InventorApplication.CommandManager.Pick// have the user select a leaf occurrence or part
        //                          (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a part to add");
        //                if (joint != null)
        //                {
        //                    foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
        //                    {
        //                        if (node.Selected)// find the selected node
        //                        {
        //                            foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
        //                            {
        //                                if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
        //                                {
        //                                    t.compOcc.Add(joint);// add the occurence to the arraylist
        //                                    InventorApplication.ActiveDocument.SelectSet.Clear();
        //                                    node.DoSelect();
        //                                }
        //                            }
        //                        }
        //                    }
        //                    runOnce = false;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("Please select a browser node to add a part to");// if the user didn't select a browser node then tell them
        //        }
        //        runOnce = true;
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        ////Cancels the export
        //public void cancelExporter_OnExecute(Inventor.NameValueMap Context)
        //{
        //    try
        //    {
        //        inExportView = false;// tell the event reactors to not react because we are no longer in export mode
        //        writeFieldComponentNames();// write the browser folder names to the property sets so we can read them next time the program is run
        //        foreach (FieldDataComponent data in FieldComponents)
        //        {// looks at all the components in fieldComponent
        //            writeSaveFieldComponent(data);// writes the saved data to the property set
        //        }
        //        FieldComponents = new ArrayList();// clear the arraylist of components
        //        foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
        //        {// looks at all the nodes under the top node
        //            node.Delete();// deletes the nodes
        //        }
        //        oPane.Visible = false;// hide the browser pane because we aren't exporting anymore
        //        addNewComponent.Enabled = false;
        //        editComponent.Enabled = false;
        //        removeComponent.Enabled = false;
        //        addAssembly.Enabled = false;
        //        beginExporter.Enabled = true;// sets the correct buttons states
        //        cancelExport.Enabled = false;
        //        exportField.Enabled = false;
        //        addPart.Enabled = false;
        //        removeAssembly.Enabled = false;
        //        removePart.Enabled = false;
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.ToString());
        //    }
        //}
    }
}

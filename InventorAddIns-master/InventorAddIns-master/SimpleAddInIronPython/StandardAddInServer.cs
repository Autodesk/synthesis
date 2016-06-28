using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using Inventor;
using Microsoft.Win32;

namespace SimpleAddIn
{
	/// <summary>
	/// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
	/// that all Inventor AddIns are required to implement. The communication between Inventor and
	/// the AddIn is via the methods on this interface.
	/// </summary>

	[GuidAttribute("963308E2-D850-466D-A1C5-503A2E171552")]
	public class AddInServer : Inventor.ApplicationAddInServer
	{
		#region Data Members
		
		//Inventor application object
		Inventor.Application m_inventorApplication;

		//buttons
		private AddSlotOptionButton m_addSlotOptionButton;
		private DrawSlotButton m_drawSlotButton;
		private ToggleSlotStateButton m_toggleSlotStateButton;			

		//combo-boxes
		private ComboBoxDefinition m_slotWidthComboBoxDefinition;
		private ComboBoxDefinition m_slotHeightComboBoxDefinition;

        //user interface event
        private UserInterfaceEvents m_userInterfaceEvents;

        // ribbon panel
        RibbonPanel m_partSketchSlotRibbonPanel;

		//event handler delegates
		private Inventor.ComboBoxDefinitionSink_OnSelectEventHandler SlotWidthComboBox_OnSelectEventDelegate;
		private Inventor.ComboBoxDefinitionSink_OnSelectEventHandler SlotHeightComboBox_OnSelectEventDelegate;

        private Inventor.UserInterfaceEventsSink_OnResetCommandBarsEventHandler UserInterfaceEventsSink_OnResetCommandBarsEventDelegate;
        private Inventor.UserInterfaceEventsSink_OnResetEnvironmentsEventHandler UserInterfaceEventsSink_OnResetEnvironmentsEventDelegate;
        private Inventor.UserInterfaceEventsSink_OnResetRibbonInterfaceEventHandler UserInterfaceEventsSink_OnResetRibbonInterfaceEventDelegate;

		#endregion

		public AddInServer()
		{
		}

		#region ApplicationAddInServer Members

		public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
		{
			try
			{
				//the Activate method is called by Inventor when it loads the addin
				//the AddInSiteObject provides access to the Inventor Application object
				//the FirstTime flag indicates if the addin is loaded for the first time

				//initialize AddIn members
                m_inventorApplication = addInSiteObject.Application;
                Button.InventorApplication = m_inventorApplication;

                //initialize event delegates
                m_userInterfaceEvents = m_inventorApplication.UserInterfaceManager.UserInterfaceEvents;

                UserInterfaceEventsSink_OnResetCommandBarsEventDelegate = new UserInterfaceEventsSink_OnResetCommandBarsEventHandler(UserInterfaceEvents_OnResetCommandBars);
                m_userInterfaceEvents.OnResetCommandBars += UserInterfaceEventsSink_OnResetCommandBarsEventDelegate;

                UserInterfaceEventsSink_OnResetEnvironmentsEventDelegate = new UserInterfaceEventsSink_OnResetEnvironmentsEventHandler(UserInterfaceEvents_OnResetEnvironments);
                m_userInterfaceEvents.OnResetEnvironments += UserInterfaceEventsSink_OnResetEnvironmentsEventDelegate;

                UserInterfaceEventsSink_OnResetRibbonInterfaceEventDelegate = new UserInterfaceEventsSink_OnResetRibbonInterfaceEventHandler(UserInterfaceEvents_OnResetRibbonInterface);
                m_userInterfaceEvents.OnResetRibbonInterface += UserInterfaceEventsSink_OnResetRibbonInterfaceEventDelegate;
                
				//load image icons for UI items
				Icon addSlotOptionIcon = new Icon(this.GetType(), "AddSlotOption.ico");
				Icon drawSlotIcon =  new Icon(this.GetType(), "DrawSlot.ico");
				Icon toggleSlotStateIcon = new Icon(this.GetType(), "ToggleSlotState.ico");	
	
				//retrieve the GUID for this class
                GuidAttribute addInCLSID; 
                addInCLSID = (GuidAttribute)GuidAttribute.GetCustomAttribute(typeof(AddInServer), typeof(GuidAttribute));
                string addInCLSIDString;
                addInCLSIDString = "{" + addInCLSID.Value + "}";

				//create the comboboxes
                m_slotWidthComboBoxDefinition = m_inventorApplication.CommandManager.ControlDefinitions.AddComboBoxDefinition("Slot Width", "Autodesk:SimpleAddIn:SlotWidthCboBox", CommandTypesEnum.kShapeEditCmdType, 100, addInCLSIDString, "Slot width", "Slot width", Type.Missing, Type.Missing, ButtonDisplayEnum.kDisplayTextInLearningMode);
                m_slotHeightComboBoxDefinition = m_inventorApplication.CommandManager.ControlDefinitions.AddComboBoxDefinition("Slot Height", "Autodesk:SimpleAddIn:SlotHeightCboBox", CommandTypesEnum.kShapeEditCmdType, 100, addInCLSIDString, "Slot height", "Slot height", Type.Missing, Type.Missing, ButtonDisplayEnum.kDisplayTextInLearningMode);
	    
				//add some initial items to the comboboxes
				m_slotWidthComboBoxDefinition.AddItem("1 cm", 0);
				m_slotWidthComboBoxDefinition.AddItem("2 cm", 0);
                m_slotWidthComboBoxDefinition.AddItem("3 cm", 0);
                m_slotWidthComboBoxDefinition.AddItem("4 cm", 0);
                m_slotWidthComboBoxDefinition.AddItem("5 cm", 0);
                m_slotWidthComboBoxDefinition.ListIndex = 1;
                m_slotWidthComboBoxDefinition.ToolTipText = m_slotWidthComboBoxDefinition.Text;
                m_slotWidthComboBoxDefinition.DescriptionText = "Slot width: " + m_slotWidthComboBoxDefinition.Text;

                SlotWidthComboBox_OnSelectEventDelegate = new ComboBoxDefinitionSink_OnSelectEventHandler(SlotWidthComboBox_OnSelect);
                m_slotWidthComboBoxDefinition.OnSelect += SlotWidthComboBox_OnSelectEventDelegate;

                m_slotHeightComboBoxDefinition.AddItem("1 cm", 0);
                m_slotHeightComboBoxDefinition.AddItem("2 cm", 0);
                m_slotHeightComboBoxDefinition.AddItem("3 cm", 0);
                m_slotHeightComboBoxDefinition.AddItem("4 cm", 0);
                m_slotHeightComboBoxDefinition.AddItem("5 cm", 0);
                m_slotHeightComboBoxDefinition.ListIndex = 1;
                m_slotHeightComboBoxDefinition.ToolTipText = m_slotHeightComboBoxDefinition.Text;
                m_slotHeightComboBoxDefinition.DescriptionText = "Slot height: " + m_slotHeightComboBoxDefinition.Text;

                SlotHeightComboBox_OnSelectEventDelegate = new ComboBoxDefinitionSink_OnSelectEventHandler(SlotHeightComboBox_OnSelect);
                m_slotHeightComboBoxDefinition.OnSelect += SlotHeightComboBox_OnSelectEventDelegate;

				//create buttons
				m_addSlotOptionButton = new AddSlotOptionButton(
					"Add Slot width/height", "Autodesk:SimpleAddIn:AddSlotOptionCmdBtn", CommandTypesEnum.kShapeEditCmdType, 
					addInCLSIDString, "Adds option for slot width/height", 
					"Add slot option", addSlotOptionIcon, addSlotOptionIcon, ButtonDisplayEnum.kDisplayTextInLearningMode);

				m_drawSlotButton = new DrawSlotButton(
                    "Draw Slot", "Autodesk:SimpleAddIn:DrawSlotCmdBtn", CommandTypesEnum.kShapeEditCmdType,
					addInCLSIDString, "Create slot sketch graphics",
					"Draw Slot", drawSlotIcon, drawSlotIcon, ButtonDisplayEnum.kDisplayTextInLearningMode);

                m_toggleSlotStateButton = new ToggleSlotStateButton(
                    "Toggle Slot State", "Autodesk:SimpleAddIn:ToggleSlotStateCmdBtn", CommandTypesEnum.kShapeEditCmdType,
                    addInCLSIDString, "Enables/Disables state of slot command",
                    "Toggle Slot State", toggleSlotStateIcon, toggleSlotStateIcon, ButtonDisplayEnum.kDisplayTextInLearningMode);

                //create the command category
                CommandCategory slotCmdCategory = m_inventorApplication.CommandManager.CommandCategories.Add("Slot", "Autodesk:SimpleAddIn:SlotCmdCat", addInCLSIDString);

                slotCmdCategory.Add(m_slotWidthComboBoxDefinition);
                slotCmdCategory.Add(m_slotHeightComboBoxDefinition);
                slotCmdCategory.Add(m_addSlotOptionButton.ButtonDefinition);
                slotCmdCategory.Add(m_drawSlotButton.ButtonDefinition);
                slotCmdCategory.Add(m_toggleSlotStateButton.ButtonDefinition);
                
				if (firstTime == true)
				{
					//access user interface manager
					UserInterfaceManager userInterfaceManager;
                    userInterfaceManager = m_inventorApplication.UserInterfaceManager;

                    InterfaceStyleEnum interfaceStyle;
                    interfaceStyle = userInterfaceManager.InterfaceStyle;

                    //create the UI for classic interface
                    if (interfaceStyle == InterfaceStyleEnum.kClassicInterface)
                    {
                        //create toolbar
                        CommandBar slotCommandBar;
                        slotCommandBar = userInterfaceManager.CommandBars.Add("Slot", "Autodesk:SimpleAddIn:SlotToolbar", CommandBarTypeEnum.kRegularCommandBar, addInCLSIDString);

                        //add comboboxes to toolbar
                        slotCommandBar.Controls.AddComboBox(m_slotWidthComboBoxDefinition, 0);
                        slotCommandBar.Controls.AddComboBox(m_slotHeightComboBoxDefinition, 0);

                        //add buttons to toolbar
                        slotCommandBar.Controls.AddButton(m_addSlotOptionButton.ButtonDefinition, 0);
                        slotCommandBar.Controls.AddButton(m_drawSlotButton.ButtonDefinition, 0);
                        slotCommandBar.Controls.AddButton(m_toggleSlotStateButton.ButtonDefinition, 0);

                        //Get the 2d sketch environment base object
                        Inventor.Environment partSketchEnvironment;
                        partSketchEnvironment = userInterfaceManager.Environments["PMxPartSketchEnvironment"];

                        //make this command bar accessible in the panel menu for the 2d sketch environment.
                        partSketchEnvironment.PanelBar.CommandBarList.Add(slotCommandBar);
                    }
                    //create the UI for ribbon interface
                    else
                    {
                        //get the ribbon associated with part document
                        Inventor.Ribbons ribbons;
                        ribbons = userInterfaceManager.Ribbons;

                        Inventor.Ribbon partRibbon;
                        partRibbon = ribbons["Part"];

                        //get the tabls associated with part ribbon
                        RibbonTabs ribbonTabs;
                        ribbonTabs = partRibbon.RibbonTabs;

                        RibbonTab partSketchRibbonTab;
                        partSketchRibbonTab = ribbonTabs["id_TabSketch"];

                        //create a new panel with the tab
                        RibbonPanels ribbonPanels;
                        ribbonPanels = partSketchRibbonTab.RibbonPanels;

                        m_partSketchSlotRibbonPanel = ribbonPanels.Add("Slot", "Autodesk:SimpleAddIn:SlotRibbonPanel", "{DB59D9A7-EE4C-434A-BB5A-F93E8866E872}", "",  false);

                        //add controls to the slot panel
                        CommandControls partSketchSlotRibbonPanelCtrls;
                        partSketchSlotRibbonPanelCtrls = m_partSketchSlotRibbonPanel.CommandControls;

                        //add the combo boxes to the ribbon panel  
                        CommandControl slotWidthCmdCboBoxCmdCtrl;
                        slotWidthCmdCboBoxCmdCtrl = partSketchSlotRibbonPanelCtrls.AddComboBox(m_slotWidthComboBoxDefinition, "", false);

                        CommandControl slotHeightCmdCboBoxCmdCtrl;
                        slotHeightCmdCboBoxCmdCtrl = partSketchSlotRibbonPanelCtrls.AddComboBox(m_slotHeightComboBoxDefinition, "", false);

                        //add the buttons to the ribbon panel
                        CommandControl drawSlotCmdBtnCmdCtrl;
                        drawSlotCmdBtnCmdCtrl = partSketchSlotRibbonPanelCtrls.AddButton(m_drawSlotButton.ButtonDefinition, false, true, "", false);


                        CommandControl slotOptionCmdBtnCmdCtrl;
                        slotOptionCmdBtnCmdCtrl = partSketchSlotRibbonPanelCtrls.AddButton(m_addSlotOptionButton.ButtonDefinition, false, true, "", false);

                        CommandControl toggleSlotStateCmdBtnCmdCtrl;
                        toggleSlotStateCmdBtnCmdCtrl = partSketchSlotRibbonPanelCtrls.AddButton(m_toggleSlotStateButton.ButtonDefinition, false,true, "", false);
                        
                    }
				}

				MessageBox.Show ("To access the commands of the sample addin, activate a 2d sketch of a part \n document and select the \"AddInSlot\" toolbar within the panel menu");
			}
			catch(Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}

		public void Deactivate()
		{
			//the Deactivate method is called by Inventor when the AddIn is unloaded
			//the AddIn will be unloaded either manually by the user or
			//when the Inventor session is terminated
		
			try
			{
                m_userInterfaceEvents.OnResetCommandBars -= UserInterfaceEventsSink_OnResetCommandBarsEventDelegate;
                m_userInterfaceEvents.OnResetEnvironments -= UserInterfaceEventsSink_OnResetEnvironmentsEventDelegate;

                UserInterfaceEventsSink_OnResetCommandBarsEventDelegate = null;
                UserInterfaceEventsSink_OnResetEnvironmentsEventDelegate = null;
                m_userInterfaceEvents = null;
                if (m_partSketchSlotRibbonPanel != null )
                {
                    m_partSketchSlotRibbonPanel.Delete();
                }

				//release inventor Application object
				Marshal.ReleaseComObject(m_inventorApplication);
                m_inventorApplication = null;

				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
			catch(Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}

		public void ExecuteCommand(int CommandID)
		{
			//this method was used to notify when an AddIn command was executed
			//the CommandID parameter identifies the command that was executed
    
			//Note:this method is now obsolete, you should use the new
			//ControlDefinition objects to implement commands, they have
			//their own event sinks to notify when the command is executed
		}

		public object Automation
		{
			//if you want to return an interface to another client of this addin,
			//implement that interface in a class and return that class object 
			//through this property

			get
			{
				return null;
			}
		}

        private void UserInterfaceEvents_OnResetCommandBars(ObjectsEnumerator commandBars, NameValueMap context)
        {
            try
            {
                CommandBar commandBar;
                for (int commandBarCt = 1; commandBarCt <= commandBars.Count; commandBarCt++)
                {
                    commandBar = (Inventor.CommandBar)commandBars[commandBarCt];
                    if (commandBar.InternalName == "Autodesk:SimpleAddIn:SlotToolbar")
                    {
                        //add comboboxes to toolbar
                        commandBar.Controls.AddComboBox(m_slotWidthComboBoxDefinition, 0);
                        commandBar.Controls.AddComboBox(m_slotHeightComboBoxDefinition, 0);

                        //add buttons to toolbar
                        commandBar.Controls.AddButton(m_addSlotOptionButton.ButtonDefinition, 0);
                        commandBar.Controls.AddButton(m_drawSlotButton.ButtonDefinition, 0);
                        commandBar.Controls.AddButton(m_toggleSlotStateButton.ButtonDefinition, 0);

                        return;           
                    }
                }               
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void UserInterfaceEvents_OnResetEnvironments(ObjectsEnumerator environments, NameValueMap context)
        {
            try
            {                
                Inventor.Environment environment;
                for (int environmentCt = 1; environmentCt <= environments.Count; environmentCt++)
                {
                    environment = (Inventor.Environment)environments[environmentCt];
                    if (environment.InternalName == "PMxPartSketchEnvironment")
                    {
                        //make this command bar accessible in the panel menu for the 2d sketch environment.
                        environment.PanelBar.CommandBarList.Add(m_inventorApplication.UserInterfaceManager.CommandBars["Autodesk:SimpleAddIn:SlotToolbar"]);

                        return;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void UserInterfaceEvents_OnResetRibbonInterface(NameValueMap context)
        {
            try
            {

                UserInterfaceManager userInterfaceManager;
                userInterfaceManager = m_inventorApplication.UserInterfaceManager;

                //get the ribbon associated with part document
                Inventor.Ribbons ribbons;
                ribbons = userInterfaceManager.Ribbons;

                Inventor.Ribbon partRibbon;
                partRibbon = ribbons["Part"];

                //get the tabls associated with part ribbon
                RibbonTabs ribbonTabs;
                ribbonTabs = partRibbon.RibbonTabs;

                RibbonTab partSketchRibbonTab;
                partSketchRibbonTab = ribbonTabs["id_TabSketch"];

                //create a new panel with the tab
                RibbonPanels ribbonPanels;
                ribbonPanels = partSketchRibbonTab.RibbonPanels;

                m_partSketchSlotRibbonPanel = ribbonPanels.Add("Slot", "Autodesk:SimpleAddIn:SlotRibbonPanel", 
                                                             "{DB59D9A7-EE4C-434A-BB5A-F93E8866E872}", "", false);

                //add controls to the slot panel
                CommandControls partSketchSlotRibbonPanelCtrls;
                partSketchSlotRibbonPanelCtrls = m_partSketchSlotRibbonPanel.CommandControls;

                //add the combo boxes to the ribbon panel  
                CommandControl slotWidthCmdCboBoxCmdCtrl;
                slotWidthCmdCboBoxCmdCtrl = partSketchSlotRibbonPanelCtrls.AddComboBox(m_slotWidthComboBoxDefinition, "", false);

                CommandControl slotHeightCmdCboBoxCmdCtrl;
                slotHeightCmdCboBoxCmdCtrl = partSketchSlotRibbonPanelCtrls.AddComboBox(m_slotHeightComboBoxDefinition, "", false);

                //add the buttons to the ribbon panel
                CommandControl drawSlotCmdBtnCmdCtrl;
                drawSlotCmdBtnCmdCtrl = partSketchSlotRibbonPanelCtrls.AddButton(m_drawSlotButton.ButtonDefinition, false, true, "", false);

                CommandControl slotOptionCmdBtnCmdCtrl;
                slotOptionCmdBtnCmdCtrl = partSketchSlotRibbonPanelCtrls.AddButton(m_addSlotOptionButton.ButtonDefinition, false, true, "", false);

                CommandControl toggleSlotStateCmdBtnCmdCtrl;
                toggleSlotStateCmdBtnCmdCtrl = partSketchSlotRibbonPanelCtrls.AddButton(m_toggleSlotStateButton.ButtonDefinition, false, true, "", false);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

		private void SlotWidthComboBox_OnSelect(NameValueMap context)
		{
            m_slotWidthComboBoxDefinition.ToolTipText = m_slotWidthComboBoxDefinition.Text;
            m_slotWidthComboBoxDefinition.DescriptionText = "Slot width: " + m_slotWidthComboBoxDefinition.Text;
		}

		private void SlotHeightComboBox_OnSelect(NameValueMap context)
		{
            m_slotHeightComboBoxDefinition.ToolTipText = m_slotHeightComboBoxDefinition.Text;
            m_slotHeightComboBoxDefinition.DescriptionText = "Slot height: " + m_slotHeightComboBoxDefinition.Text;
		}

		#endregion
	}
}

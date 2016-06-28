 using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;

using Inventor;

public partial class Form1
{
	private Inventor.Application mApp;
	//Illustrates how to create a custom class that holds our combobox items
	//The key to do that is to override the "ToString" method
	private class ComboAddinsItem
	{

		private string _Name;

		private  string     _clsId;
		public string Name {
			get { return _Name; }
		}

		public string clsId {
			get { return _clsId; }
		}

		public ComboAddinsItem(string name, string clsId)
		{
			_Name = name;
			_clsId = clsId;
		}

		public override string ToString()
		{
			return _Name;
		}

	}



	private void Form1_Load(object sender, System.EventArgs e)
	{

		try {
			mApp = (Inventor.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application");
             

            foreach (ApplicationAddIn oAddIn in mApp.ApplicationAddIns)
            {
				try {
					//Fills up at runtime our addins combobox
					if ((oAddIn.AddInType == ApplicationAddInTypeEnum.kTranslationApplicationAddIn)) {
						ComboAddinsItem newItem = new ComboAddinsItem(oAddIn.DisplayName, oAddIn.ClassIdString);
						ComboAddIns.Items.Add(newItem);
					}

				} catch {
					//Exception can be thrown if "AddInType", "DisplayName" or "ClassIdString" properties
					//are not implemented. For a custom addin for example.
					continue;
				}

			}

			ComboAddIns.SelectedIndex = 0;


		} catch (Exception ex) {
			System.Windows.Forms.MessageBox.Show("Error: Inventor must be running...");
			Button1.Enabled = false;
			return;

		}

	}



	public Form1()
	{
		Load += Form1_Load;
		// This call is required by the Windows Form Designer.
		InitializeComponent();

		// Add any initialization after the InitializeComponent() call.
		OptionsView.View = System.Windows.Forms.View.Details;

	}


	public bool GetTranslatorSaveAsOptions(string TranslatorClsId, ref NameValueMap options)
	{
		bool functionReturnValue = false;

        TranslatorAddIn oTranslator = (TranslatorAddIn)mApp.ApplicationAddIns.ItemById[TranslatorClsId];

		if (oTranslator == null) {
			functionReturnValue = false;
			return functionReturnValue;
		}

		oTranslator.Activate();

		if ((oTranslator.AddInType != ApplicationAddInTypeEnum.kTranslationApplicationAddIn)) {
			//Not a translator addin...
			functionReturnValue = false;
			return functionReturnValue;
		}

		//Gets application translation context and set type to UnspecifiedIOMechanism
		TranslationContext Context = mApp.TransientObjects.CreateTranslationContext();
		Context.Type = IOMechanismEnum.kUnspecifiedIOMechanism;

		options = mApp.TransientObjects.CreateNameValueMap();

		object SourceObject = mApp.ActiveDocument;

		//Checks whether the translator has 'SaveCopyAs' options
		try {
			functionReturnValue = oTranslator.get_HasSaveCopyAsOptions(SourceObject, Context, options);
		} catch {
			functionReturnValue = false;
		}
		return functionReturnValue;
	}


	public bool GetTranslatorOpenOptions(string TranslatorClsId, ref NameValueMap options)
	{
		bool functionReturnValue = false;

        TranslatorAddIn oTranslator = (TranslatorAddIn)mApp.ApplicationAddIns.ItemById[TranslatorClsId];

		if (oTranslator == null) {
			functionReturnValue = false;
			return functionReturnValue;
		}

		oTranslator.Activate();

		if ((oTranslator.AddInType != ApplicationAddInTypeEnum.kTranslationApplicationAddIn)) {
			//Not a translator addin...
			functionReturnValue = false;
			return functionReturnValue;
		}

		DataMedium Medium = mApp.TransientObjects.CreateDataMedium();
		Medium.FileName = "C:\\Temp\\File.xxx";
		Medium.MediumType = MediumTypeEnum.kFileNameMedium;

		TranslationContext Context = mApp.TransientObjects.CreateTranslationContext();

		options = mApp.TransientObjects.CreateNameValueMap();

		try {
			functionReturnValue = oTranslator.get_HasOpenOptions(Medium, Context, options);
		} catch {
			functionReturnValue = false;
		}
		return functionReturnValue;

	}



	private void Button1_Click(System.Object sender, System.EventArgs e)
	{
		NameValueMap options = null;

		OptionsView.Items.Clear();

        ComboAddinsItem item = (ComboAddinsItem)ComboAddIns.SelectedItem;


		if ((GetTranslatorSaveAsOptions(item.clsId, ref options))) {
			ListViewGroup saveGroup = OptionsView.Groups["GroupSaveOpts"];

			int idx = 0;

			for (idx = 1; idx <= options.Count; idx++) {
				ListViewItem listviewItem = default(ListViewItem);

				listviewItem = OptionsView.Items.Add(options.Name[idx]);
				listviewItem.SubItems.Add(options.Value[options.Name[idx]].ToString());

				listviewItem.Group = saveGroup;
			}

		}


		if ((GetTranslatorOpenOptions(item.clsId, ref options))) {
			ListViewGroup openGroup = OptionsView.Groups["GroupOpenOpts"];

			int idx = 0;

			for (idx = 1; idx <= options.Count; idx++) {
				ListViewItem listviewItem = default(ListViewItem);

				listviewItem = OptionsView.Items.Add(options.Name[idx]);
				listviewItem.SubItems.Add(options.Value[options.Name[idx]].ToString());

				listviewItem.Group = openGroup;
			}

		}

	}

  


}
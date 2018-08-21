##BxDRobotExporter

How to Register/Unregister addin with Inventor
	=======================
  1) Install Inventor on you machine
  
  2) follow the instructions here: http://help.autodesk.com/view/INVNTOR/2018/ENU/?guid=GUID-6FD7AA08-1E43-43FC-971B-5F20E56C8846 to setup Inventor
    dev tools
  
	3) Build Project;

	2) Copy add-in dll file to one of following locations: 
		a) Anywhere, then *.addin file <Assembly> setting should be updated to the full path including the dll name
		b) Inventor <InstallPath>\bin\ folder, then *.addin file <Assembly> setting should be the dll name only: <AddInName>.dll
		c) Inventor <InstallPath>\bin\XX folder, then *.addin file <Assembly> setting shoule be a relative path: XX\<AddInName>.dll

	4) Copy.addin manifest file to one of following locations:
		a) Inventor Version Dependent
		Windows XP:
			C:\Documents and Settings\All Users\Application Data\Autodesk\Inventor [version]\Addins\
		Windows7/Vista:
			C:\ProgramData\Autodesk\Inventor [version]\Addins\

		b) Inventor Version Independent
		Windows XP:
			C:\Documents and Settings\All Users\Application Data\Autodesk\Inventor Addins\
		Windows7/Vista:
			C:\ProgramData\Autodesk\Inventor Addins\

		c) Per User Override
		Windows XP:
			C:\Documents and Settings\<user>\Application Data\Autodesk\Inventor [version]\Addins\
		Windows7/Vista:
			C:\Users\<user>\AppData\Roaming\Autodesk\Inventor [version]\Addins\

	5) Startup Inventor, the AddIn should be loaded

	To unregister the AddIn, remove the Autodesk.<AddInName>.Inventor.addin from above mentioned .addin manifest file locations directly.

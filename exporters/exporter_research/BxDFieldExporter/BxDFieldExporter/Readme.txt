	How to Register/Unregister 
	=======================

	1) Build Project;

	2) Copy add-in dll file to one of following locations: 
		a) Anywhere, then *.addin file <Assembly> setting should be updated to the full path including the dll name
		b) Inventor <InstallPath>\bin\ folder, then *.addin file <Assembly> setting should be the dll name only: <AddInName>.dll
		c) Inventor <InstallPath>\bin\XX folder, then *.addin file <Assembly> setting shoule be a relative path: XX\<AddInName>.dll

	3) Copy.addin manifest file to one of following locations:
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

	4) Startup Inventor, the AddIn should be loaded

	To unregister the AddIn, remove the Autodesk.<AddInName>.Inventor.addin from above mentioned .addin manifest file locations directly.

  
  
  SimpleAddIn Sample
  =======================
  
  DESCRIPTION
  This sample demonstrates a minimal implementation of an Add-In.  
  
  This sample corresponds to the Add-In section of the Developer's Guide found in the programming 
  online help.  See that section for details about how Add-Ins work and how to create an Add-In.

  How to run this sample:
  1) Copy Autodesk.SimpleAddIn.Inventor.addin into ...\Autodesk\Inventor 20XX\Addins folder.
     For XP: C:\Documents and Settings\All Users\Application Data\Autodesk\Inventor 20XX\Addins.
     For Vista/Win7: C:\ProgramData\Autodesk\Inventor 20XX\Addins.

  2) Copy bin\Release\SimpleAddIn.dll into Inventor bin folder(For example: C:\Program Files\Autodesk\Inventor 20XX\Bin).

  3) Startup Inventor, the AddIn should be loaded, and on ribbon UI if you open a part document and activate a sketch and you can see the "Slot" panel on "Sketch" tab.
  
  The two combo boxes allow you to specify the height and width of the slot 
  sketch. If you run the "Draw Slot" command and a sketch is active, it will draw a shape in the 
  active sketch. If you run the "Add slot width/height" command it will add one more entry in the 
  combo boxes for width and height. The "Toggle Slot State"command toggles the "Draw Slot" command.


  Language/Compiler: VC# (.NET)
  Server: Inventor.

 
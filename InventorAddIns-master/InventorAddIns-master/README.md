InventorAddIns
==============

This repository will house various code examples related to the Autodesk Inventor API.

Inventor 2013 targets .NET 4.0, and as a result of not updating SDK sample code, most 
examples will not compile.  Two common issues you may run into in your addin code are 
addressed in the SimpleAddIn.  

SimpleAddInIronPython
==============
###Embedding a python engine:
This version of the addin is the same as the original "SimpleAddIn" with changes made to the "DrawSlotButton"
C# class, the addition of an additional class "Engine", and an additional python file that contains the logic 
that was previously contained in the "DrawSlotButton" class.  

You will need to have IronPython installed for this to work.  You may need to adjust the references depending
on the IronPython install directory.  

In general, the case for doing this is to speed up the development cycle.  It is a big pain to start and restart
Inventor over and over.  The approach is that the UI integration is handled in C#, and that the main program logic
is factored out into the python component.  In this example the C# parts and the python parts are not very losely
coupled.  It is a good idea to seperate these two aspects as much as possible.  Because the python code is being 
compiled at runtime, it is possible to execute the add-in, make a change to the python code, and then run the add-in
again without the need to shut down Inventor or compile again.

SimpleAddIn
==============
###Post-build events:
The first has to do with the post-build events command line.  The project as-is already
has this corrected, I'm pointing this out because it is not obvious (to me at least).

As shipped with Inventor 2013 sample projects were last built with, wait for it, MSVS 2008.  
Under project properties (or similar, depending on your IDE),under "Post-build event 
command line" remove all of what is currently there and replace it with the following:

<PostBuildEvent xmlns="http://schemas.microsoft.com/developer/msbuild/2003">call "C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\mt.exe" -manifest "$(ProjectDir)PartsList for DrawingView.X.manifest" -outputresource:"$(TargetPath)";#2</PostBuildEvent>

###AddIn Icons:
The projects in the sdk that integrate with Inventor's UI need to take an image and 
turn it into an icon.  The way this was done in the samples no longer works in 
.NET 4.0.  The people at the Mod the Machine blog have a blog post related to this 
that says the following:

"Having to reference Microsoft.VisualBasic.Compatibility and use 
Microsoft.VisualBasic.Compatibility.VB6.Support.IconToIPicture() to create an IPictureDisp 
works, but I always thought there should be a better way to create the image for the user 
interface. Also in a future release of Inventor there is a good chance that it will support 
.NET framework 4 and warning messages occur using IconToPicture(). (Inventor 2012 uses .NET 3.5)."

I commented out the old lines, so you should be able to compare.  There is an additional C# class that
makes this work that is adapted from the VB example.  I think it should not be a requirement of someone
developing using Inventor's API to write 90 lines of code to have icons on buttons.  In future versions it 
would be nice if some of the boiler plate work required to make an add-in could be abstracted more.  


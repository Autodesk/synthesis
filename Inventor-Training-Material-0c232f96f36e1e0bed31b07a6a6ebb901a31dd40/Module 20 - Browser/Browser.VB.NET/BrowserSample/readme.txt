This is a registry addin

1. build the addin
2. on 32bits OS, it will be registered automatically
   On 64bits OS, you need to run in command prompt window
    regasm (64bits)  BrowserSample.dll
    
3. Open Inventor and a part document
4. draw some extrude features and revolved features.
5. in the first panel of the first tab, there are 3 buttons

   HierarchyPane: this will add a custom pane named "My Pane". It adds
                  three custom nodes 				  
				  Top Node
					Node2
					Node3
				  and one built-in nodes from "Model" Pane.

    DoBrowserEvents: this will start the event of browserpane. Once it starts, 
					double clicking any custom node will pop out a message box.						

					click Node2, all extrude features will be highlighted.

					click Node3, all revolved features will be highlighted.

					click this button again will stop the event

   ActiveXPane:  this will add a custom pane named "MyActiveXPane" in which there
				 is one button

  
    

  

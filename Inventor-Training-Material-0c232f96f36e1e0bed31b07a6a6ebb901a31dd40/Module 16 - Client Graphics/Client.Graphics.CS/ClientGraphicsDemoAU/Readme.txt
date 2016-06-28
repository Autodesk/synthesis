
Samples Description:

This is a C# addin, which can be accessed from the Inventor Ribbon bar, Tab “Tools”. 
It contains several modules, each of them illustrates one functionality of the ClientGraphics API:

>	Arc.cs: An interaction example that draws dynamically a CurveGraphics based on an arc.

>	ClientFeatureGraphics.cs: Displays a TriangleGraphics attached to a ClientFeature. The graphics stored inside the ClientFeature are persisted across sessions.

>	Component.cs: A more advanced example illustrating the use of ComponentGraphics and MouseEvents. The user can select a part or assembly to insert inside the active part, a Non-Parametric base feature is then created.

>	CurveGraphic.cs: Displays a CurveGraphics based on an edge selected by the user on the model.

>	DrawingGraphics.cs: Illutrates use of ClientGraphics in Drawing Sheet, using Interaction and MouseEvents to draw a moving symbol on the drawing.

>	DynamicSimulationControl.cs: Performed dynamic simulation inside an assembly using transacted or non-transacted ClientGraphics. The dynamic simulation engine is powered by Bullet, a professional free 3D Game Multiphysics Library, visit http://bulletphysics.org for more details about Bullet.

>	GraphicsDimension.cs: Illutrates use of ClientGraphics to store ClientGraphics based dimensions working in Part or Assembly documents.

>	InteractionLineStrip.cs: An interaction example using a LinStripsGraphics and MouseEvents.

>	LineStrip.cs: Displays a LineStripGraphics between three vertices selected by the user on the model.

>	SimpleInteraction.cs: A simple interaction example using a TriangleGraphics and MouseEvents.

>	SimpleInteractionMng.cs: A simple interaction example using a LineGraphics and MouseEvents.

>	SimpleLine.cs: Displays a LineGraphics between two vertices selected by the user on the model.

>	SimplePrimitives.cs: A series of simple demos for various graphic primitives.

>	SliceGraphics.cs: Illustrates use of graphic node slicing functionality.


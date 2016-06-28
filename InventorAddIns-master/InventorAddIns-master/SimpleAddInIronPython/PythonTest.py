import clr
import Inventor 
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Drawing")
clr.AddReference("System.Collections")
import System.Windows.Forms
import System.Drawing
import System.Collections.Generic


class PythonTest:
	def __init__(self,planarSketch,oApp,slotHeight,slotWidth):
		System.Windows.Forms.MessageBox.Show("this","that",System.Windows.Forms.MessageBoxButtons.OK)
		transientGeometry = oApp.TransientGeometry
		#start a transaction so the slot will be within a single undo step
		createSlotTransaction = oApp.TransactionManager.StartTransaction(oApp.ActiveDocument, "Create Slot")
		#Change this 'divideBy' value after Inventor is running to feel the awesomeness of making changes to a program
		#and not having to close Inventor, compile, restart, etc.
		divideBy = 3.0
		#draw the lines and arcs that make up the shape of the slot
		lines = []
		arcs = []
		lines.append(planarSketch.SketchLines.AddByTwoPoints(transientGeometry.CreatePoint2d(0, 0), transientGeometry.CreatePoint2d(slotWidth, 0)))
		arcs.append(planarSketch.SketchArcs.AddByCenterStartEndPoint(transientGeometry.CreatePoint2d(slotWidth, slotHeight/divideBy), lines[0].EndSketchPoint, transientGeometry.CreatePoint2d(slotWidth, slotHeight), True))
		lines.append(planarSketch.SketchLines.AddByTwoPoints(arcs[0].EndSketchPoint, transientGeometry.CreatePoint2d(0, slotHeight)))
		arcs.append(planarSketch.SketchArcs.AddByCenterStartEndPoint(transientGeometry.CreatePoint2d(0, slotHeight/divideBy), lines[1].EndSketchPoint, lines[0].StartSketchPoint, True))
		#create the tangent constraints between the lines and arcs
		planarSketch.GeometricConstraints.AddTangent(lines[0], arcs[0], None)
		planarSketch.GeometricConstraints.AddTangent(lines[1], arcs[0], None)
		planarSketch.GeometricConstraints.AddTangent(lines[1], arcs[1], None)
		planarSketch.GeometricConstraints.AddTangent(lines[0], arcs[1], None)
		#create a parallel constraint between the two lines
		planarSketch.GeometricConstraints.AddParallel(lines[0], lines[1], True, True)
		#end the transaction
		createSlotTransaction.End()
		
		
		
thisTest = PythonTest(oPlanarSketch,oApp,slotHeight,slotWidth)
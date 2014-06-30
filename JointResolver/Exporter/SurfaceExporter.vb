Imports Inventor
Imports System.IO

' Not thread safe.
Class SurfaceExporter
    Private Const MAX_VERTICIES As Integer = 8192 * 128

    Private tmpVerts(MAX_VERTICIES * 3) As Double
    Private tmpNorms(MAX_VERTICIES * 3) As Double
    Private tmpIndicies(MAX_VERTICIES * 3) As Integer
    Private tmpVertCount As Integer
    Private tmpFacetCount As Integer

    Public verts(MAX_VERTICIES * 3) As Double
    Public norms(MAX_VERTICIES * 3) As Double
    Public indicies(MAX_VERTICIES * 3) As Integer
    Public vertCount As Integer
    Public facetCount As Integer

    ' Tolerances
    Private tolerances(10) As Double
    Private tmpToleranceCount As Integer

    ' Facets are relative to world space!
    Public Sub AddFacets(ByRef surf As SurfaceBody, Optional ByVal bestResolution As Boolean = False)
        surf.GetExistingFacetTolerances(tmpToleranceCount, tolerances)
            Dim bestIndex As Integer = -1
            For i As Integer = 0 To tmpToleranceCount - 1
                If bestIndex < 0 OrElse ((tolerances(i) < tolerances(bestIndex)) = bestResolution) Then bestIndex = i ' Highest tolerance
            Next i
            Console.WriteLine("Exporting " & surf.Parent.Name & "." & surf.Name & " with tolerance " & tolerances(bestIndex))
            surf.GetExistingFacets(tolerances(bestIndex), tmpVertCount, tmpFacetCount, tmpVerts, tmpNorms, tmpIndicies)
            If tmpVertCount = 0 Then
                Console.WriteLine("Calculate values")
                surf.CalculateFacets(tolerances(bestIndex), tmpVertCount, tmpFacetCount, tmpVerts, tmpNorms, tmpIndicies)
            End If
            Array.Copy(tmpVerts, 0, verts, vertCount * 3, tmpVertCount * 3)
            Array.Copy(tmpNorms, 0, norms, facetCount * 3, tmpVertCount * 3)

            ' Now we must manually copy the indicies
            Dim indxOffset As Integer = facetCount * 3
            For i As Integer = 0 To (tmpFacetCount * 3) - 1
                indicies(i + indxOffset) = tmpIndicies(i) + vertCount
            Next i
            facetCount += tmpFacetCount
            vertCount += tmpVertCount
    End Sub

    Public Sub Reset()
        vertCount = 0
        facetCount = 0
    End Sub

    Public Sub ExportAll(ByRef occ As ComponentOccurrence)
        If Not (occ.Visible) Then Return
        For Each surf As SurfaceBody In occ.SurfaceBodies
            AddFacets(surf, True)
        Next surf
        For Each item As ComponentOccurrence In occ.SubOccurrences
            ExportAll(item)
        Next item
    End Sub

    Public Sub ExportAll(ByRef occs As ComponentOccurrences)
        For Each occ As ComponentOccurrence In occs
            ExportAll(occ)
        Next
    End Sub

    Public Sub ExportAll(ByRef occs As List(Of ComponentOccurrence))
        For Each occ As ComponentOccurrence In occs
            ExportAll(occ)
        Next
    End Sub

    Public Sub WriteSTL(path As String)
        Dim writer As StreamWriter = New StreamWriter(path)
        writer.WriteLine("solid " & "bleh")
        For i As Integer = 0 To facetCount - 1
            Dim offset As Integer = i * 3
            For j As Integer = 0 To 2
                Dim oj As Integer = indicies(offset + j) * 3 - 3
                If j = 0 Then
                    writer.WriteLine("facet normal " & Str(norms(oj)) & " " & Str(norms(oj + 1)) & " " & Str(norms(oj + 2)))
                    writer.WriteLine("outer loop")
                End If
                writer.WriteLine("vertex " & Str(verts(oj)) + " " + Str(verts(oj + 1)) + " " + Str(verts(oj + 2)))
            Next j
            writer.WriteLine("endloop")
            writer.WriteLine("endfacet")
        Next i
        writer.WriteLine("endsolid")
        writer.Close()
    End Sub
End Class

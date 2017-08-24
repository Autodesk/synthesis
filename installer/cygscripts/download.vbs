Option Explicit

download WScript.Arguments(0)

Sub download(ByVal URL)
	Dim pwd, name, objXMLHTTP
	pwd = WScript.CreateObject("WScript.Shell").CurrentDirectory
	name = pwd & "\" & Right(URL, Len(URL) - InStrRev(URL, "/" ))
	Set objXMLHTTP = CreateObject("MSXML2.XMLHTTP")
	objXMLHTTP.Open "GET", URL, false
	objXMLHTTP.Send()
	If objXMLHTTP.Status = 200 Then
		Dim objStream, objFSO
		Set objStream = CreateObject("ADODB.Stream")
		objStream.Open
		objStream.Type = 1
		objStream.Write objXMLHTTP.ResponseBody
		objStream.Position = 0
		Set objFSO = CreateObject("Scripting.FileSystemObject")
		If objFSO.FileExists(name) Then objFSO.DeleteFile name
		Set objFSO = Nothing
		objStream.SaveToFile name
		objStream.Close
		Set objStream = Nothing
	End if
	Set objXMLHTTP = Nothing
End Sub
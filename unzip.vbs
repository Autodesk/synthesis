Option Explicit

unzip WScript.Arguments(0), 256

Sub unzip(ByVal source, ByVal options)
	Dim pwd, destination, objFS, objShell, objSource, objTarget
	pwd = WScript.CreateObject("WScript.Shell").CurrentDirectory
	source = pwd & "\" & source
	destination = pwd & "\" & CreateObject("Scripting.FileSystemObject").GetBaseName(source)
	Set objFS = CreateObject("Scripting.FileSystemObject")
	If Not objFS.FolderExists(destination) Then objFS.CreateFolder(destination)
	Set objShell = CreateObject("Shell.Application")
	Set objSource = objShell.NameSpace(source).Items()
	Set objTarget = objShell.NameSpace(destination)
	objTarget.CopyHere objSource, options
	Dim objFile, objFiles
	Set objFiles = objFS.GetFolder(destination).Files
	For Each objFile in objFiles
		If UCase(objFS.GetExtensionName(objFile.Name)) = "ZIP" Then
			WScript.CreateObject("WScript.Shell").CurrentDirectory = destination
			unzip objFile.Name, options
			WScript.CreateObject("WScript.Shell").CurrentDirectory = pwd
		End If
	Next
	Set objFile = Nothing
	Set objFiles = Nothing
	Set objSource = Nothing
	Set objTarget = Nothing
	Set objFS = Nothing
	Set objShell = Nothing
End Sub
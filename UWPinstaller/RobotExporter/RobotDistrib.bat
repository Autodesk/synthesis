IF EXIST "%APPDATA%\Autodesk\Inventor 2018\Addins" (
	XCOPY "Autodesk.BxDRobotExporter.Inventor.addin" "%APPDATA%\Autodesk\Inventor 2018\Addins" /Y /R /Q
)

IF EXIST "%APPDATA%\Autodesk\Inventor 2017\Addins" (
	XCOPY "Autodesk.BxDRobotExporter.Inventor.addin" "%APPDATA%\Autodesk\Inventor 2017\Addins" /Y /R /Q
)

EXIT 0
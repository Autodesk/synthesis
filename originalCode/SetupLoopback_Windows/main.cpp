#include <windows.h>
#include <tchar.h>
#include <stdlib.h>
#include <stdio.h>
#include <setupapi.h>
#include <regstr.h>
#include <infstr.h>
#include <cfgmgr32.h>
#include <string.h>
#include <malloc.h>
#include <newdev.h>
#include <objbase.h>
#include <strsafe.h>
#include <Iphlpapi.h>

#define EXIT_OK      (0)
#define EXIT_REBOOT  (1)
#define EXIT_FAIL    (2)
#define EXIT_USAGE   (3)

#pragma comment(lib, "IPHLPAPI.lib")

#define UPDATEDRIVERFORPLUGANDPLAYDEVICES "UpdateDriverForPlugAndPlayDevicesA"
#define SETUPUNINSTALLOEMINF "SetupUninstallOEMInfA"

typedef BOOL (WINAPI *UpdateDriverForPlugAndPlayDevicesProto)(_In_opt_ HWND hwndParent,
															  _In_ LPCTSTR HardwareId,
															  _In_ LPCTSTR FullInfPath,
															  _In_ DWORD InstallFlags,
															  _Out_opt_ PBOOL bRebootRequired
															  );



void printError(DWORD dw = 0xFFFFFFF) {
	LPVOID lpMsgBuf;
	LPVOID lpDisplayBuf;
	if (dw == 0xFFFFFFF) {
		dw = GetLastError();
	}
	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER | 
		FORMAT_MESSAGE_FROM_SYSTEM |
		FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		dw,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR) &lpMsgBuf,
		0, NULL );
	printf("Error #%u:\t%s\n", dw, lpMsgBuf);
}

int cmdInstall(LPCTSTR hwid, LPCTSTR inf)
{
	HDEVINFO DeviceInfoSet = INVALID_HANDLE_VALUE;
	SP_DEVINFO_DATA DeviceInfoData;
	GUID ClassGUID;
	TCHAR ClassName[MAX_CLASS_NAME_LEN];
	TCHAR hwIdList[LINE_LEN+4];
	int failcode = EXIT_FAIL;

	ZeroMemory(hwIdList,sizeof(hwIdList));
	if (FAILED(StringCchCopy(hwIdList,LINE_LEN,hwid))) {
		printf("Strcpyfail\n");
		goto final;
	}

	if (!SetupDiGetINFClass(inf,&ClassGUID,ClassName,sizeof(ClassName)/sizeof(ClassName[0]),0))
	{
		printError();
		goto final;
	}

	DeviceInfoSet = SetupDiCreateDeviceInfoList(&ClassGUID,0);
	if(DeviceInfoSet == INVALID_HANDLE_VALUE)
	{
		printError();
		goto final;
	}

	DeviceInfoData.cbSize = sizeof(SP_DEVINFO_DATA);
	if (!SetupDiCreateDeviceInfo(DeviceInfoSet,
		ClassName,
		&ClassGUID,
		NULL,
		0,
		DICD_GENERATE_ID,
		&DeviceInfoData))
	{
		printError();
		goto final;
	}

	if(!SetupDiSetDeviceRegistryProperty(DeviceInfoSet,
		&DeviceInfoData,
		SPDRP_HARDWAREID,
		(LPBYTE)hwIdList,
		(lstrlen(hwIdList)+1+1)*sizeof(TCHAR)))
	{
		printError();
		goto final;
	}

	if (!SetupDiCallClassInstaller(DIF_REGISTERDEVICE,
		DeviceInfoSet,
		&DeviceInfoData))
	{
		printError();
		printf("Installdev\n");
		goto final;
	}

	HMODULE newdevMod = NULL;
	UpdateDriverForPlugAndPlayDevicesProto UpdateFn;
	BOOL reboot = FALSE;

	newdevMod = LoadLibrary(TEXT("newdev.dll"));
	if(!newdevMod) {
		printError();
		goto final;
	}
	UpdateFn = (UpdateDriverForPlugAndPlayDevicesProto)GetProcAddress(newdevMod,UPDATEDRIVERFORPLUGANDPLAYDEVICES);
	if(!UpdateFn)
	{
		printError();
		goto final;
	}

	if(!UpdateFn(NULL,hwid,inf,INSTALLFLAG_FORCE,&reboot)) {
		printError();
		goto final;
	}

	failcode = reboot ? EXIT_REBOOT : EXIT_OK;
	final:

	if (DeviceInfoSet != INVALID_HANDLE_VALUE) {
		SetupDiDestroyDeviceInfoList(DeviceInfoSet);
	}

	return failcode;
}

DWORD setupLoopbackDevice(int team) {
	char commandBuffer[1024];
	ULONG outBufLen = 0;
	DWORD dwRetVal = 0;
	IP_ADAPTER_INFO* pAdapterInfos = (IP_ADAPTER_INFO*) malloc(sizeof(IP_ADAPTER_INFO));

	// retry up to 5 times, to get the adapter infos needed
	for( int i = 0; i < 5 && (dwRetVal == ERROR_BUFFER_OVERFLOW || dwRetVal == NO_ERROR); ++i )
	{
		dwRetVal = GetAdaptersInfo(pAdapterInfos, &outBufLen);
		if( dwRetVal == NO_ERROR )
		{
			break;
		}
		else if( dwRetVal == ERROR_BUFFER_OVERFLOW )
		{
			free(pAdapterInfos);
			pAdapterInfos = (IP_ADAPTER_INFO*) malloc(outBufLen);
		}
		else
		{
			pAdapterInfos = 0;
			break;
		}
	}
	if( dwRetVal == NO_ERROR )
	{
		dwRetVal = ERROR_NOT_FOUND;
		IP_ADAPTER_INFO* pAdapterInfo = pAdapterInfos;
		while( pAdapterInfo )
		{
			if (strcmp(pAdapterInfo->Description, "Microsoft Loopback Adapter") == 0){
				printf("Config adapter %d for team %d\n", pAdapterInfo->Index, team);
				sprintf_s(commandBuffer, "netsh interface ipv4 set address %d static 10.%d.%d.2 255.255.255.0 none", pAdapterInfo->Index, team/100, team%100);
				printf("%s\n", commandBuffer);
				system(commandBuffer);
				sprintf_s(commandBuffer, "netsh interface ipv4 add address %d 10.%d.%d.5 255.255.255.0", pAdapterInfo->Index, team/100, team%100);
				printf("%s\n", commandBuffer);
				system(commandBuffer);
				dwRetVal = NO_ERROR;
				break;
			}
			pAdapterInfo = pAdapterInfo->Next;
		}
	}
	free(pAdapterInfos);
	return dwRetVal;
}

int main(int argc, char **argv) {
	LPCTSTR hwid = "*msloop";
	LPTSTR inf = "C:\\Windows\\inf\\netloop.inf";//new TCHAR[512];
	/*GetWindowsDirectory(inf, 512);
	_tcscat_s(inf, 512, "\\inf\\netloop.inf");*/
	printf("Device to install: %s\n", inf);

	int team = 0;
	if (argc > 1) {
		team = atoi(argv[1]);
		char tempBuffer[10];
		sprintf_s(tempBuffer, "%u", team);
		if (strcmp(argv[1], tempBuffer) != 0) {
			printf("%s doesn't appear to equal %u; team number invalid\n", argv[0], team);
			getc(stdin);
			return ERROR_INVALID_PARAMETER;
		}
	} else {
		while (team<=0){
			printf("What team number do you want configured? ");
			scanf_s("%d", &team);
		}
	}
	if (setupLoopbackDevice(team)  == ERROR_NOT_FOUND) {
		printf("Loopback device not found; installing...\n");
		DWORD installError = cmdInstall(hwid, inf);
		if (installError) {
			printError(installError);
			printf("Press enter to continue...\n");
			getc(stdin);
			return installError;
		}
		for (int i = 0; i<3; i++) {
			printf("\rInstalled.  Waiting for registry to update... %d / 3", i);
			Sleep(1000);
		}
		printf("\n");
		DWORD setupError = setupLoopbackDevice(team);
		if (setupError) {
			printError(setupError);
			printf("Press enter to continue...\n");
			getc(stdin);
			return setupError;
		}
	}
	printf("Success!\n");
	printf("Press enter to continue...\n");
	getc(stdin);
	return 0;
}
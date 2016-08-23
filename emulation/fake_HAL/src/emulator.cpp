
#include "emulator.h"

DWORD WINAPI emulator(LPVOID nonExistData) // THIS VERSION IS ONLY FOR WINAPI ONLY 
{
	printf("executing emulator packet code");
	StateNetworkServer server;
	OutputStatePacket outPack;
	server.Open();
	while (true) {
		std::array<unsigned short, kPwmPins> pwmCopy;
		{//keep scope for lock guard to release mutex
			std::lock_guard<std::mutex> lock(lockerPWMValues);
			std::copy(pwmChannelValues.begin(), pwmChannelValues.end(), pwmCopy.begin());
		}
		for (int i = 0; i < kPwmPins; i++)
		{
			outPack.dio[0].pwmValues[i] = (float)pwmCopy[i] / 256;
		}
		std::cout << outPack.dio[0].pwmValues[0];
		std::cout << outPack.dio[0].pwmValues[1];
		server.SendStatePacket(outPack);
		Sleep(50);
	}
	server.Close();
	return 0;
}
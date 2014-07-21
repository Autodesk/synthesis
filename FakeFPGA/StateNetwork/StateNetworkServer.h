#ifndef __STATE_NETWORK_SERVER_H
#define __STATE_NETWORK_SERVER_H

#include "StateNetwork/StatePacket.h"
class StateNetworkServer
{
private:
	static const int PORT  = 2550;

	int udpSocket;
public:
	StateNetworkServer(void);
	~StateNetworkServer(void);
	void Open();
	void Close();
	void SendStatePacket(StatePacket pack);
};

#endif
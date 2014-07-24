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
	/// Opens the socket of this network server.
	void Open();
	/// Closes the socket of this network server.
	void Close();
	/// Sends the given state packet over this network server.
	void SendStatePacket(StatePacket pack);
};

#endif
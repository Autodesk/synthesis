#pragma once

#include "AardvarkGate.h"
#include <mutex>
#include <thread>
#include <cstdint>
#include <Windows.h>

class StateNetworkServer {
public:
	StateNetworkServer();
	virtual ~StateNetworkServer();

	static constexpr uint32_t kDefaultPort = 1540; // Flaming Chickens

	void Run();
private:
	uint32_t m_port;
	SOCKET m_socket;
	AardvarkGate m_gate;
};
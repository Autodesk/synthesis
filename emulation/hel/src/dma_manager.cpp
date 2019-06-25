#include "FRC_FPGA_ChipObject/tDMAManager.h"

namespace nFPGA{
	tDMAManager::tDMAManager(uint32_t /*dmaChannel*/, uint32_t /*hostBufferSize*/, tRioStatusCode* status): tSystem(status){} // TODO

	tDMAManager::~tDMAManager(){} // TODO

	void tDMAManager::start(tRioStatusCode* /*status*/){} // TODO

	void tDMAManager::stop(tRioStatusCode* /*status*/){} // TODO

	void tDMAManager::read(uint32_t* /*buf*/, size_t /*num*/, uint32_t /*timeout*/, size_t* /*remaining*/, tRioStatusCode* /*status*/){} // TODO

	void tDMAManager::write(uint32_t* /*buf*/, size_t /*num*/, uint32_t /*timeout*/, size_t* /*remaining*/, tRioStatusCode* /*status*/){} // TODO

	void tDMAManagerread(uint8_t* /*buf*/, size_t /*num*/, uint32_t /*timeout*/, size_t* /*remaining*/, tRioStatusCode* /*status*/){} // TODO

	void tDMAManager::write(uint8_t* /*buf*/, size_t /*num*/, uint32_t /*timeout*/, size_t* /*remaining*/, tRioStatusCode* /*status*/){} // TODO
}

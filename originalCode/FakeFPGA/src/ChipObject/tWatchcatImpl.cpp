#include "ChipObject/tWatchcatImpl.h"

#include "ChipObject/NiFpgaState.h"
#include "ChipObject/tGlobalImpl.h"

namespace nFPGA {
	tWatchcat_Impl::tWatchcat_Impl(NiFpgaState *state)
	{
		this->state = state;
		this->status.Alive = true;
		this->status.DisableCount = 0;
		this->status.SysDisableCount = 0;
		this->status.SystemActive = 1;
		tRioStatusCode status;
		this->lastFed = state->getGlobal()->readLocalTime(&status);
	}


	tWatchcat_Impl::~tWatchcat_Impl(void)
	{
	}
	tSystemInterface* tWatchcat_Impl::getSystemInterface()
	{
		return this->state;
	}

	tWatchcat_Impl::tStatus tWatchcat_Impl::readStatus(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		return this->status;
	}
	bool tWatchcat_Impl::readStatus_SystemActive(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		return this->status.SystemActive;
	}
	bool tWatchcat_Impl::readStatus_Alive(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		this->status.Alive = !immortal && (lastFed + expiration) > state->getGlobal()->readLocalTime(status);
		return this->status.Alive;
	}
	unsigned short tWatchcat_Impl::readStatus_SysDisableCount(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		return this->status.SysDisableCount;
	}
	unsigned short tWatchcat_Impl::readStatus_DisableCount(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		return this->status.DisableCount;
	}


	void tWatchcat_Impl::strobeKill(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		this->status.DisableCount++;
		this->status.SystemActive = 0;
		this->status.Alive = false;
	}

	void tWatchcat_Impl::strobeFeed(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		this->lastFed = state->getGlobal()->readLocalTime(status);
	}

	unsigned int tWatchcat_Impl::readTimer(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		return state->getGlobal()->readLocalTime(status);
	}

	void tWatchcat_Impl::writeExpiration(unsigned int value, tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		expiration = value;
	}
	unsigned int tWatchcat_Impl::readExpiration(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		return expiration;
	}

	void tWatchcat_Impl::writeImmortal(bool value, tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		immortal = value;
	}
	bool tWatchcat_Impl::readImmortal(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		return immortal;
	}

}
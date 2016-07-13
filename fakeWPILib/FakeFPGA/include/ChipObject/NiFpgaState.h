/*
* NiFpgaState.h
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#ifndef NIFPGASTATE_H_
#define NIFPGASTATE_H_

#include <ChipObject/NiRio.h>
#include <ChipObject/tSystemInterface.h>

class NiIRQ_Impl;

namespace nFPGA {

	class tDIO_Impl;
	class tAI_Impl;
	class tSolenoid_Impl;
	class tAccumulator_Impl;
	class tGlobal_Impl;
	class tAlarm_Impl;
	class tEncoder_Impl;
	class tInterrupt_Impl;
	class tCounter_Impl;
	class tAnalogTrigger_Impl;

	// Represents and maintains instances of all the systems in the FPGA
	class NiFpgaState: public tSystemInterface {
		friend class tDIO_Impl;
		friend class tAI_Impl;
		friend class tSolenoid_Impl;
		friend class tAccumulator_Impl;
		friend class tGlobal_Impl;
		friend class tEncoder_Impl;
		friend class tInterrupt_Impl;
		friend class tCounter_Impl;
		friend class tAnalogTrigger_Impl;
		friend class tAlarm_Impl;
	private:
#pragma region ADDRESSES
		static const int kFPGA_RESET_REGISTER = 0x8102;
		static const int kFPGA_COMMAND_REGISTER = 0x8104;
		static const int kFPGA_COMMAND_ENABLE_CLEAR = 4;
		static const int kFPGA_COMMAND_ENABLE_IN = 2;
		static const int kFPGA_INTERRUPT_BASE_ADDRESS = 0x8000;
	public:
		static const int kFPGA_SIGNATURE_REGISTER = 0x8108;
	private:
		static const int kMITE_IOPCR_REGISTER = 0x470;
		static const int kMITE_IOPCR_32BIT = 0xC00231;
#pragma endregion
	private:
		static const uint32_t FPGA_RAM_SIZE = 0x10000;
		static const uint32_t kExpectedFPGASignature[];
		static const uint32_t kExpectedFPGAVersion = 8210;
		static const uint32_t kExpectedFPGARevision = 0x00106004;

		uint8_t sigChunk;

		tDIO_Impl **dio;
		tAI_Impl **ai;
		tAccumulator_Impl **accum;
		tSolenoid_Impl *solenoid;
		tGlobal_Impl *global;
		tEncoder_Impl **encoder;
		tInterrupt_Impl **interrupt;
		tCounter_Impl **counter;
		tAnalogTrigger_Impl **analogTrigger;
		tAlarm_Impl *alarm;

		NiIRQ_Impl *irqManager;

	public:
		char *fpgaRAM;

		NiFpgaState();
		virtual ~NiFpgaState();

		// These all just get-or-create instances of the given modules.
		tDIO_Impl *getDIO(unsigned char module);
		tAI_Impl *getAnalog(unsigned char module);
		tAccumulator_Impl *getAccumulator(unsigned char sys_index);
		tSolenoid_Impl *getSolenoid();
		tGlobal_Impl *getGlobal();
		tEncoder_Impl *getEncoder(unsigned char sys_index);
		tInterrupt_Impl *getInterrupt(unsigned char sys_index);
		tCounter_Impl *getCounter(unsigned char sys_index);
		tAnalogTrigger_Impl *getAnalogTrigger(unsigned char sys_index);
		tAlarm_Impl *getAlarm();

		/// Gets the interrupt manager
		NiIRQ_Impl *getIRQManager();

		virtual const uint16_t getExpectedFPGAVersion();
		virtual const uint32_t getExpectedFPGARevision();
		virtual const uint32_t * const getExpectedFPGASignature();
		virtual void getHardwareFpgaSignature(uint32_t *guid_ptr,
			tRioStatusCode *status);
		virtual uint32_t getLVHandle(tRioStatusCode *status);
		virtual uint32_t getHandle();

		virtual uint32_t readSignatureChunk();
	};

} /* namespace nFPGA */

#endif /* NIFPGASTATE_H_ */

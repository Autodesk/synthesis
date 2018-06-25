#ifndef HANDLE_H
#define HANDLE_H

namespace minerva{
	union Handle{
		int32_t packed;
		struct{
			uint8_t channel;
			uint8_t module;
			uint8_t unused;
			uint8_t type: 7;
			uint8_t error: 1;
		} unpacked;
	};
}

#endif

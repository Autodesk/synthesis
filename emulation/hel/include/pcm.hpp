#ifndef _PCM_HPP_
#define _PCM_HPP_

#include "can_device.hpp"
#include "bounds_checked_array.hpp"

namespace hel{

    struct PCM{ //TODO add support for other PCM data
		static constexpr uint8_t NUM_SOLENOIDS = 8;

        enum MessageData{
            SOLENOIDS = 2,
            SIZE = 8
        };

		enum SendCommandByteMask: uint8_t{};

        enum ReceiveCommandIDMask: uint32_t{};

	private:
		BoundsCheckedArray<bool,NUM_SOLENOIDS> solenoids;

	public:
		BoundsCheckedArray<bool, NUM_SOLENOIDS> getSolenoid()const noexcept;
		void setSolenoid(uint8_t,bool);
		void setSolenoids(uint8_t);
		void setSolenoids(const BoundsCheckedArray<bool,NUM_SOLENOIDS>&);

		PCM()noexcept;
		PCM(const PCM&)noexcept;
    };
}

#endif

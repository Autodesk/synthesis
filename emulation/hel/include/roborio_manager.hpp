#ifndef _ROBORIO_MANAGER_HPP_
#define _ROBORIO_MANAGER_HPP_

#include <memory>
#include <mutex>
#include <thread>

#include "roborio.hpp"

namespace hel{

    /**
     *
     */

    class RoboRIOManager {
    public:

        // This is the only method exposed to the outside.
        // All other instance getters should be private, accessible through friend classes

        static std::pair<std::shared_ptr<RoboRIO>, std::unique_lock<std::recursive_mutex>> getInstance();

        static RoboRIO getCopy();

    private:
        RoboRIOManager() {}
        static std::shared_ptr<RoboRIO> instance;

        static std::recursive_mutex m;

    public:
        RoboRIOManager(RoboRIOManager const&) = delete;
        void operator=(RoboRIOManager const&) = delete;

        friend class SyncServer;
    };
}
#endif

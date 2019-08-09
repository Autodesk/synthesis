#ifndef _ROBORIO_MANAGER_HPP_
#define _ROBORIO_MANAGER_HPP_

#include <mutex>
#include <thread>

#include "roborio.hpp"

namespace hel{

    /**
     * \brief Manager of a global RoboRIO instance using a Singleton pattern
     */

    class RoboRIOManager {
    public:

        /**
         * \brief Get the RoboRIO instance for use
         * Locks the current thread and returns both the RoboRIO instance and the lock
         * \return A pair with the RoboRIO instance and thread lock
         */

        static std::pair<std::shared_ptr<RoboRIO>, std::unique_lock<std::recursive_mutex>> getInstance();

        /**
         * \brief Get a copy of the RoboRIO instance
         * \return The copied RoboRIO object
         */

        static RoboRIO getCopy();

    private:
        /**
         * Constructor for RoboRIOManager
         */

        RoboRIOManager() {}

        /**
         * \brief The RoboRIO instance
         */

        static std::shared_ptr<RoboRIO> instance;

        /**
         * \brief The mutex used to lock the thread accessing the RoboRIO instance
         */

        static std::recursive_mutex roborio_mutex;

    public:
        RoboRIOManager(RoboRIOManager const&) = delete;
        void operator=(RoboRIOManager const&) = delete;
    };
}
#endif

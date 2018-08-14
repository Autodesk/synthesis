#include "roborio_manager.hpp"
#include "roborio.hpp"

namespace hel{
    std::pair<std::shared_ptr<RoboRIO>, std::unique_lock<std::recursive_mutex>> RoboRIOManager::getInstance() {
        std::unique_lock<std::recursive_mutex> lock(roborio_mutex);
        if (instance == nullptr) {
            instance = std::make_shared<RoboRIO>();
        }
        return std::make_pair(instance, std::move(lock));
    }

    RoboRIO RoboRIOManager::getCopy() {
        auto instance = RoboRIOManager::getInstance();
        auto roborio_copy{*instance.first};
        instance.second.unlock();
        return roborio_copy;
    }
}

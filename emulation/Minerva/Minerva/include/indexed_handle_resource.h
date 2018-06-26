#include <stdint.h>

#include <array>
#include <memory>

#include <wpi/mutex.h>

#include "HAL/Errors.h"
#include "HAL/Types.h"
#include "HAL/handles/HandlesInternal.h"

namespace minerva {
    template<typename T, typename R, int16_t size, hal::HAL_HandleEnum enum_value>
    class IndexedHandleResource {
    public:
        IndexedHandleResource() = default;
        IndexedHandleResource(const IndexedHandleResource&) = delete;
        IndexedHandleResource& operator=(const IndexedHandleResource&) = delete;

        T allocate(int16_t index, int32_t* status);
        std::shared_ptr<R> get(T handle);

        void free(T handle);
        void resetHandles();
    private:
        std::array<std::shared_ptr<R>, size> items;
        std::array<wpi::mutex, size> handle_mutexes;
    };

    template<typename T, typename R, int16_t size, hal::HAL_HandleEnum enum_value>
    T IndexedHandleResource<T, R, size, enum_value>::allocate(int16_t index, int32_t* status) {
        if (index < 0 || index >= size) {
            return 0;
        }
        std::lock_guard<wpi::mutex> lock(handle_mutexes[index]);

        if (items[index] != nullptr) {
            return 0;
        }
        items[index] = std::make_shared<R>();
        return static_cast<T>(hal::createHandle(index, enum_value, 0));
    }
    template<typename T, typename R, int16_t size, hal::HAL_HandleEnum enum_value>
    std::shared_ptr<R> IndexedHandleResource<T, R, size, enum_value>::get(T handle) {
        int16_t index = (enum_value == static_cast<hal::HAL_HandleEnum>((handle >>24) & 0xFF))? static_cast<uint16_t>(handle & 0xFFFF): 0;
        if (index < 0 || index >= size) return;

        std::lock_guard<wpi::mutex> lock(handle_mutexes[index]);
        return items[index];
    }
    template<typename T, typename R, int16_t size, hal::HAL_HandleEnum enum_value>
    void IndexedHandleResource<T, R, size, enum_value>::free(T handle) {
        int16_t index = (enum_value == static_cast<hal::HAL_HandleEnum>((handle >>24) & 0xFF))? static_cast<uint16_t>(handle & 0xFFFF): 0;
        if (index < 0 || index >= size) return;

        std::lock_guard<wpi::mutex> lock(handle_mutexes[index]);
        items[index].reset();;
    }

    template<typename T, typename R, int16_t size, hal::HAL_HandleEnum enum_value>
    void IndexedHandleResource<T, R, size, enum_value>::resetHandles() {

        for (int i = 0; i < size; i++) {
            std::lock_guard<wpi::mutex> lock(handle_mutexes[i]);
            items[i].reset();
        }
    }

}

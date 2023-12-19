#pragma once

namespace SYN {
    class Core {
    public:
        static Core* Init() {
            if (Core::m_instance == nullptr) {
                Core::m_instance = new Core();
            }

            return Core::m_instance;
        }

        static bool Destroy() {
            if (Core::m_instance != nullptr) {
                delete Core::m_instance;
                Core::m_instance = nullptr;
            }

            return Core::m_instance == nullptr;
        }

    private:
        Core();
        ~Core();

        static Core *m_instance;
    };
}

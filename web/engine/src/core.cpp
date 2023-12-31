#include "core.h"

#include <emscripten/bind.h>

#include <string>

const float CONST_FLOATS[5] = { 0.0f, 1.0f, 2.0f, 6.0f, 3.0f };

namespace SYN {

    Core *Core::instance = nullptr;

    Core::Core(): physics_manager(PhysicsManager()) { }

    Core::~Core() { }

    std::weak_ptr<MiraAssembly> Core::LoadAssembly(const void *binary, size_t size) {
        mirabuf::Assembly *assembly = new mirabuf::Assembly();
        bool res = assembly->ParseFromArray(binary, size);

        if (res) {
            std::cout << "Successfully parsed assembly: '" << assembly->info().name() << "'\n";

            auto handler = std::make_shared<MiraAssembly>(assembly);
            this->assemblies.push_back(handler);
            return handler;
        } else {
            std::cout << "Failed to parse assembly\n";
            return std::weak_ptr<MiraAssembly>(); // TODO: Should probably having some better error handling
        }
    }
    
    void TestPrint(std::string str) {
        std::cout << "Test Print: '" << str << "'\n";
    }

    std::weak_ptr<MiraAssembly> LoadAssembly(const void *binary, size_t size) {
        return Core::instance->LoadAssembly(binary, size);
    }

    emscripten::val TestView(int num) {
        return emscripten::val(emscripten::typed_memory_view(num, CONST_FLOATS));
    }
}

// C Functions

extern "C" {

    void core_init() {
        if (SYN::Core::instance != nullptr) {
            delete SYN::Core::instance;
        }
        SYN::Core::instance = new SYN::Core();
    }

    void core_destroy() {
        if (SYN::Core::instance != nullptr) {
            delete SYN::Core::instance;
            SYN::Core::instance = nullptr;
        }
    }

}

// C++ Bindings

EMSCRIPTEN_BINDINGS(my_module) {
    emscripten::function("TestPrint", &SYN::TestPrint);
    emscripten::function("TestView", &SYN::TestView);

    emscripten::function("LoadMiraAssembly", &SYN::LoadAssembly, emscripten::allow_raw_pointers());
}
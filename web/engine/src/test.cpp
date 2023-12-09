#include <math.h>

#include "assembly.pb.h"
#include <google/protobuf/io/coded_stream.h>
#include <fstream>
#include <iostream>
#include <string>

class TestObj {
private:
    int m_a;
public:
    TestObj(int a) {
        this->m_a = a;
    }
    ~TestObj() { }

    int getA() {
        return this->m_a;
    }
};

extern "C" {

    void* make_obj() {
        return new TestObj(69);
    }

    int get_obj_value(void* obj) {
        return ((TestObj*)obj)->getA();
    }

    void free_obj(void* obj) {
        delete (TestObj*)obj;
    }

    int proto_test(void* mem, int size) {

        GOOGLE_PROTOBUF_VERIFY_VERSION;

        std::cout << "Correct Versions\n";

        mirabuf::Assembly assembly;
        assembly.mutable_info()->set_name("Hello");
        std::cout << assembly.DebugString() << std::endl;
        void *newMem = new uint8_t[assembly.ByteSizeLong()];
        assembly.SerializeToArray(newMem, assembly.ByteSizeLong());

        mirabuf::Assembly assembly2;
        int res = assembly2.ParseFromArray(newMem, assembly.ByteSizeLong()) ? 1 : 0;
        delete[] (uint8_t *)newMem;

        std::cout << assembly2.DebugString() << std::endl;

        mirabuf::Assembly assembly3;
        res += assembly3.ParseFromArray(mem, size) ? 1 : 0;

        std::cout << assembly3.DebugString() << std::endl;

        return res;
    }

    void *parse_assembly(void *mem, int size) {
        mirabuf::Assembly *assembly = new mirabuf::Assembly();
        bool res = assembly->ParseFromArray(mem, size);

        if (res) {
            std::cout << "Successfully parsed assembly\n";
        } else {
            std::cout << "Failed to parse assembly\n";
        }

        return assembly;
    }

    void destroy_assembly(void *assembly) {
        delete (mirabuf::Assembly *)assembly;
    }

    void debug_print_assembly(void *assembly) {
        auto definition = ((mirabuf::Assembly *)assembly)->data().parts().part_definitions();
        for (auto iter = definition.begin(); iter != definition.end(); ++iter) {
            if (iter->second.bodies_size() > 0) {
                std::cout << iter->second.DebugString() << std::endl;
            }
        }
    }

    void load_mesh_data(const void *assem, void **v, void **i, void **n) {
        const mirabuf::Assembly *assembly = (const mirabuf::Assembly *) assem;
        // assembly->
    }

}

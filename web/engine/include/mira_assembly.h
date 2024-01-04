#pragma once

#include <assembly.pb.h>
#include <string>
#include <vector>

namespace SYN {

    namespace MIRA {

        class Assembly;
        class Node;

        class Node {
        public:
            Node(std::weak_ptr<Assembly> assemblyPtr, std::vector<std::string> *parts = nullptr);
        };
    
        // TODO: Come up with better terminology
        class Assembly {
        public:
            Assembly(mirabuf::Assembly *assemblyPtr);
            Assembly(const Assembly&) = delete;
            ~Assembly();

            inline std::string GetName();
        private:
            mirabuf::Assembly *assemblyPtr;
        };
    
    }

}
#include "shared_main.h"

#if defined(OS_WIN)
// TODO
#else
int main(int argc, char* argv[]) {
    return synthesis::shared::main(argc, argv);
}
#endif

#include "shared_main.h"

#if defined(OS_WIN)
int APIENTRY wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow) {
    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);
    UNREFERENCED_PARAMETER(nCmdShow);
    return synthesis::shared::wWinMain(hInstance);
}
#else // ^^^ defined(OS_WIN) ^^^ / vvv !defined(OS_WIN) vvv
int main(int argc, char* argv[]) {
    return synthesis::shared::main(argc, argv);
}
#endif // !defined(OS_WIN)

#include "internal/shared_main.h"

#include "include/run_browser.h"

#include <iostream>

int main(int argc, char* argv[]) {
    std::cout << "Running..." << std::endl;

    StartBrowserClient();

    std::cout << "Done." << std::endl;

    return 0;
}

#include "resource_util.h"

#import <Foundation/Foundation.h>

#include <mach-o/dyld.h> // Mac specefic header
#include <cstdio>

namespace synthesis {
namespace shared {
namespace {

// Adapted from Chromium's base/mac/foundation_util.mm
bool UncachedAmIBundled() {
    return [[[NSBundle mainBundle] bundlePath] hasSuffix:@".app"];
}

bool AmIBundled() {
    static bool amIBundled = UncachedAmIBundled();
    return amIBundled;
}

} // namespace

// Addapted from Chromium's base/base_path_mac.mm
bool GetResourceDir(std::string& dir) {
    uint32_t pathSize = 0;
    _NSGetExecutablePath(nullptr, &pathSize);

    if (pathSize > 0) {
        dir.resize(pathSize);
        _NSGetExecutablePath(const_cast<char*>(dir.c_str()), &pathSize);
    }

    if (AmIBundled()) {
        std::string::size_type lastSep = dir.find_last_of("/");
        dir.resize(lastSep);
        dir.append("/../Resources"); // TODO: Check this dir
        return true;
    }

    dir.append("/Resources"); // TODO: Check this dir
    return true;
}

} // namespace shared
} // namespace synthesis

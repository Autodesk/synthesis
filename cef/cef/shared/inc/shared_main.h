#pragma once
#ifndef SYNTHESIS_CEF_SHARED_MAIN_H_
#define SYNTHESIS_CEF_SHARED_MAIN_H_

#include <include/base/cef_build.h>

#if defined(OS_WINDOWS)
#include <windows.h>
#endif // defined(OS_WINDOWS)

namespace synthesis {
namespace shared {

#if defined(OS_WINDOWS)
int APIENTRY wWinMain(HINSTANCE hInstance);
#else // ^^^ defined(OS_WINDOWS) ^^^ / vvv !defined(OS_WINDOWS) vvv
int main(int argc, char* argv[]);
#endif // !defined(OS_WINDOWS)

} // namespace shared
} // namespace synthesis

#endif // SYNTHESIS_CEF_SHARED_MAIN_H_

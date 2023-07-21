#ifndef SYNTHESIS_CEF_SHARED_MAIN_H_
#define SYNTHESIS_CEF_SHARED_MAIN_H_
#pragma once

#include <include/base/cef_build.h>

#if defined(OS_WIN)
#include <windows.h>
#endif // defined(OS_WIN)

namespace synthesis {
namespace shared {

#if defined(OS_WIN)
int APIENTRY wWinMain(HINSTANCE hInstance);
#else // ^^^ defined(OS_WIN) ^^^ / vvv !defined(OS_WIN) vvv
int main(int argc, char* argv[]);
#endif // !defined(OS_WIN)

} // namespace shared
} // namespace synthesis

#endif // SYNTHESIS_CEF_SHARED_MAIN_H_

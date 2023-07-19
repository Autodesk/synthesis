#ifndef SYNTHESIS_RUN_BROWSER_H_
#define SYNTHESIS_RUN_BROWSER_H_
#pragma once

#include "internal/core.h"

SYNTHESIS_BEGIN_EXTERN_C

// Implemented based on platform `run_browser_[platform][.cpp/.mm]`
SYNTHESIS_EXPORT void StartBrowserClient();

SYNTHESIS_END_EXTERN_C

#endif // SYNTHESIS_RUN_BROWSER_H_

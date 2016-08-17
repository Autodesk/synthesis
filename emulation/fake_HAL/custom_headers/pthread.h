#pragma once
//! @file pthread.h
/*the purpose of this file is to fill a dependency to pthread but since synthesis only 
runs on windows (which is dumb) I cant fill the dependency!*/
#define TASK task
#define STATUS int32_t
#define pthread_t void
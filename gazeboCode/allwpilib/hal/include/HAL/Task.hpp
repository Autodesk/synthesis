#pragma once

#ifdef __vxworks
#include <vxWorks.h>
#else
#include <stdint.h>
#include <pthread.h>
#endif

#ifndef _FUNCPTR_DEFINED
#define _FUNCPTR_DEFINED
#ifdef __cplusplus
typedef int (*FUNCPTR)(...);
/* ptr to function returning int */
#else
typedef int (*FUNCPTR) (); /* ptr to function returning int */
#endif /* __cplusplus */
#endif /* _FUNCPTR_DEFINED */

#ifndef _STATUS_DEFINED
#define _STATUS_DEFINED
typedef int STATUS;
#endif /* _STATUS_DEFINED */

#ifndef OK
#define OK		0
#endif /* OK */
#ifndef ERROR
#define ERROR		(-1)
#endif /* ERROR */

#ifdef __vxworks
#define NULL_TASK -1
typedef int32_t TASK;
#else
#define NULL_TASK NULL
typedef pthread_t* TASK;
#endif

extern "C" {
  // Note: These constants used to be declared extern and were defined in
  // Task.cpp. This caused issues with the JNI bindings for java, and so the
  // instantiations were moved here.
  const uint32_t VXWORKS_FP_TASK = 0x01000000;
  const int32_t HAL_objLib_OBJ_ID_ERROR = -1; // TODO: update to relevant TaskIDError
  const int32_t HAL_objLib_OBJ_DELETED = -1; // TODO: update to relevant TaskDeletedError
  const int32_t HAL_taskLib_ILLEGAL_OPTIONS = -1; // TODO: update to relevant TaskOptionsError
  const int32_t HAL_memLib_NOT_ENOUGH_MEMORY = -1; // TODO: update to relevant TaskMemoryError
  const int32_t HAL_taskLib_ILLEGAL_PRIORITY = -1; // TODO: update to relevant TaskPriorityError

	TASK spawnTask(char * name, int priority, int options, int stackSize, FUNCPTR entryPt,
			uint32_t arg0, uint32_t arg1, uint32_t arg2, uint32_t arg3, uint32_t arg4,
			uint32_t arg5, uint32_t arg6, uint32_t arg7, uint32_t arg8, uint32_t arg9);
	STATUS restartTask(TASK task);
	STATUS deleteTask(TASK task);
	STATUS isTaskReady(TASK task);
	STATUS isTaskSuspended(TASK task);
	STATUS suspendTask(TASK task);
	STATUS resumeTask(TASK task);
	STATUS verifyTaskID(TASK task);
	STATUS setTaskPriority(TASK task, int priority);
	STATUS getTaskPriority(TASK task, int* priority);

}

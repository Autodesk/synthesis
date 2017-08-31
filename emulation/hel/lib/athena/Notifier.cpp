/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "HAL/Notifier.h"

// For std::atexit()
#include <cstdlib>

#include <atomic>
#include <memory>
#include <mutex>

#include "HAL/ChipObject.h"
#include "HAL/Errors.h"
#include "HAL/HAL.h"
#include "HAL/cpp/make_unique.h"
#include "HAL/cpp/priority_mutex.h"
#include "HAL/handles/UnlimitedHandleResource.h"
#include "support/SafeThread.h"

using namespace hal;

static const int32_t kTimerInterruptNumber = 28;

static hal::priority_mutex notifierInterruptMutex;
static priority_recursive_mutex notifierMutex;
static uint64_t closestTrigger = UINT64_MAX;

namespace {
struct Notifier {
  std::shared_ptr<Notifier> prev, next;
  void* param;
  HAL_NotifierProcessFunction process;
  uint64_t triggerTime = UINT64_MAX;
  HAL_NotifierHandle handle;
  bool threaded;
};

// Safe thread to allow callbacks to run on their own thread
class NotifierThread : public wpi::SafeThread {
 public:
  void Main() {
    std::unique_lock<std::mutex> lock(m_mutex);
    while (m_active) {
      m_cond.wait(lock, [&] { return !m_active || m_notify; });
      if (!m_active) break;
      m_notify = false;
      uint64_t currentTime = m_currentTime;
      HAL_NotifierHandle handle = m_handle;
      HAL_NotifierProcessFunction process = m_process;
      lock.unlock();  // don't hold mutex during callback execution
      process(currentTime, handle);
      lock.lock();
    }
  }

  bool m_notify = false;
  HAL_NotifierHandle m_handle = HAL_kInvalidHandle;
  HAL_NotifierProcessFunction m_process;
  uint64_t m_currentTime;
};

class NotifierThreadOwner : public wpi::SafeThreadOwner<NotifierThread> {
 public:
  void SetFunc(HAL_NotifierProcessFunction process, void* param) {
    auto thr = GetThread();
    if (!thr) return;
    thr->m_process = process;
    m_param = param;
  }

  void Notify(uint64_t currentTime, HAL_NotifierHandle handle) {
    auto thr = GetThread();
    if (!thr) return;
    thr->m_currentTime = currentTime;
    thr->m_handle = handle;
    thr->m_notify = true;
    thr->m_cond.notify_one();
  }

  void* m_param;
};
}  // namespace

static std::shared_ptr<Notifier> notifiers;
static std::atomic_flag notifierAtexitRegistered = ATOMIC_FLAG_INIT;
static std::atomic_int notifierRefCount{0};

using namespace hal;

static UnlimitedHandleResource<HAL_NotifierHandle, Notifier,
                               HAL_HandleEnum::Notifier>
    notifierHandles;

// internal version of updateAlarm used during the alarmCallback when we know
// that the pointer is a valid pointer.
void updateNotifierAlarmInternal(std::shared_ptr<Notifier> notifierPointer,
                                 uint64_t triggerTime, int32_t* status) {}

static void alarmCallback(uint32_t, void*) {}

static void cleanupNotifierAtExit() {}

static void threadedNotifierHandler(uint64_t currentTimeInt,
                                    HAL_NotifierHandle handle) {}

extern "C" {

HAL_NotifierHandle HAL_InitializeNotifierNonThreadedUnsafe(
  HAL_NotifierProcessFunction process, void* param, int32_t* status) {

  std::shared_ptr<Notifier> notifier = std::make_shared<Notifier>();
  HAL_NotifierHandle handle = notifierHandles.Allocate(notifier);
  if (handle == HAL_kInvalidHandle) {
    *status = HAL_HANDLE_ERROR;
    return HAL_kInvalidHandle;
  }
  // create notifier structure and add to list
  notifier->next = notifiers;
  if (notifier->next) notifier->next->prev = notifier;
  notifier->param = param;
  notifier->process = process;
  notifier->handle = handle;
  notifier->threaded = false;
  notifiers = notifier;
  return handle;
}

HAL_NotifierHandle HAL_InitializeNotifier(HAL_NotifierProcessFunction process,
                                          void* param, int32_t* status) {
  std::shared_ptr<Notifier> notifier = std::make_shared<Notifier>();
  HAL_NotifierHandle handle = notifierHandles.Allocate(notifier);
  if (handle == HAL_kInvalidHandle) {
    *status = HAL_HANDLE_ERROR;
    return HAL_kInvalidHandle;
  }
  // create notifier structure and add to list
  notifier->next = notifiers;
  if (notifier->next) notifier->next->prev = notifier;
  notifier->param = param;
  notifier->process = process;
  notifier->handle = handle;
  notifier->threaded = false;
  notifiers = notifier;
  return handle;
}

HAL_NotifierHandle HAL_InitializeNotifierThreaded(
    HAL_NotifierProcessFunction process, void* param, int32_t* status) {
  NotifierThreadOwner* notify = new NotifierThreadOwner;
  notify->Start();
  notify->SetFunc(process, param);

  auto notifierHandle =
      HAL_InitializeNotifier(threadedNotifierHandler, notify, status);

  if (notifierHandle == HAL_kInvalidHandle || *status != 0) {
    delete notify;
    return HAL_kInvalidHandle;
  }

  auto notifier = notifierHandles.Get(notifierHandle);
  if (!notifier) {
    return HAL_kInvalidHandle;
  }
  notifier->threaded = true;

  return notifierHandle;
}

void HAL_CleanNotifier(HAL_NotifierHandle notifierHandle, int32_t* status) {
  {
    std::lock_guard<priority_recursive_mutex> sync(notifierMutex);
    auto notifier = notifierHandles.Get(notifierHandle);
    if (!notifier) return;

    // remove from list
    if (notifier->prev) notifier->prev->next = notifier->next;
    if (notifier->next) notifier->next->prev = notifier->prev;
    if (notifiers == notifier) notifiers = notifier->next;
    notifierHandles.Free(notifierHandle);

    if (notifier->threaded) {
      NotifierThreadOwner* owner =
          static_cast<NotifierThreadOwner*>(notifier->param);
      delete owner;
    }
  }
}

void* HAL_GetNotifierParam(HAL_NotifierHandle notifierHandle, int32_t* status) {
  auto notifier = notifierHandles.Get(notifierHandle);
  if (!notifier) return nullptr;
  if (notifier->threaded) {
    // If threaded, return thread param rather then notifier param
    NotifierThreadOwner* owner =
        static_cast<NotifierThreadOwner*>(notifier->param);
    return owner->m_param;
  }
  return notifier->param;
}

void HAL_UpdateNotifierAlarm(HAL_NotifierHandle notifierHandle,
                             uint64_t triggerTime, int32_t* status) {}

void HAL_StopNotifierAlarm(HAL_NotifierHandle notifierHandle, int32_t* status) {}

}  // extern "C"

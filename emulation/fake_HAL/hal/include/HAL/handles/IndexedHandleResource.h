/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include <stdint.h>

#include <array>
#include <memory>

#include "HAL/Errors.h"
#include "HAL/Types.h"
#include "HAL/cpp/make_unique.h"
#include "HAL/cpp/priority_mutex.h"
#include "HAL/handles/HandlesInternal.h"

namespace hal {

/**
 * The IndexedHandleResource class is a way to track handles. This version
 * allows a limited number of handles that are allocated by index.
 * Because they are allocated by index, each individual index holds its own
 * mutex, which reduces contention heavily.]
 *
 * @tparam THandle The Handle Type (Must be typedefed from HAL_Handle)
 * @tparam TStruct The struct type held by this resource
 * @tparam size The number of resources allowed to be allocated
 * @tparam enumValue The type value stored in the handle
 *
 */
template <typename THandle, typename TStruct, int16_t size,
          HAL_HandleEnum enumValue>
class IndexedHandleResource {
  friend class IndexedHandleResourceTest;

 public:
  IndexedHandleResource() = default;
  IndexedHandleResource(const IndexedHandleResource&) = delete;
  IndexedHandleResource& operator=(const IndexedHandleResource&) = delete;

  THandle Allocate(int16_t index, int32_t* status);
  std::shared_ptr<TStruct> Get(THandle handle);
  void Free(THandle handle);

 private:
  std::array<std::shared_ptr<TStruct>, size> m_structures;
  std::array<hal::priority_mutex, size> m_handleMutexes;
};

template <typename THandle, typename TStruct, int16_t size,
          HAL_HandleEnum enumValue>
THandle IndexedHandleResource<THandle, TStruct, size, enumValue>::Allocate(
    int16_t index, int32_t* status) {
  // don't aquire the lock if we can fail early.
  if (index < 0 || index >= size) {
    *status = RESOURCE_OUT_OF_RANGE;
    return HAL_kInvalidHandle;
  }
  std::lock_guard<hal::priority_mutex> sync(m_handleMutexes[index]);
  // check for allocation, otherwise allocate and return a valid handle
  if (m_structures[index] != nullptr) {
    *status = RESOURCE_IS_ALLOCATED;
    return HAL_kInvalidHandle;
  }
  m_structures[index] = std::make_shared<TStruct>();
  return static_cast<THandle>(hal::createHandle(index, enumValue));
}

template <typename THandle, typename TStruct, int16_t size,
          HAL_HandleEnum enumValue>
std::shared_ptr<TStruct>
IndexedHandleResource<THandle, TStruct, size, enumValue>::Get(THandle handle) {
  // get handle index, and fail early if index out of range or wrong handle
  int16_t index = getHandleTypedIndex(handle, enumValue);
  if (index < 0 || index >= size) {
    return nullptr;
  }
  std::lock_guard<hal::priority_mutex> sync(m_handleMutexes[index]);
  // return structure. Null will propogate correctly, so no need to manually
  // check.
  return m_structures[index];
}

template <typename THandle, typename TStruct, int16_t size,
          HAL_HandleEnum enumValue>
void IndexedHandleResource<THandle, TStruct, size, enumValue>::Free(
    THandle handle) {
  // get handle index, and fail early if index out of range or wrong handle
  int16_t index = getHandleTypedIndex(handle, enumValue);
  if (index < 0 || index >= size) return;
  // lock and deallocated handle
  std::lock_guard<hal::priority_mutex> sync(m_handleMutexes[index]);
  m_structures[index].reset();
}
}  // namespace hal

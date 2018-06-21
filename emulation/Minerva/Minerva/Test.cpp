#include "Test.h"

using namespace std;

void minerva::Relay::Set(Relay::Value value) {
  //if (StatusIsFatal()) return;

  int32_t status = 0;

  switch (value) {
    case kOff:
      if (m_direction == kBothDirections || m_direction == kForwardOnly) {
        HAL_SetRelay(m_forwardHandle, false, &status);
      }
      if (m_direction == kBothDirections || m_direction == kReverseOnly) {
        HAL_SetRelay(m_reverseHandle, false, &status);
      }
      break;
    case kOn:
      if (m_direction == kBothDirections || m_direction == kForwardOnly) {
        HAL_SetRelay(m_forwardHandle, true, &status);
      }
      if (m_direction == kBothDirections || m_direction == kReverseOnly) {
        HAL_SetRelay(m_reverseHandle, true, &status);
      }
      break;
    case kForward:
      if (m_direction == kReverseOnly) {
        break;
      }
      if (m_direction == kBothDirections || m_direction == kForwardOnly) {
        HAL_SetRelay(m_forwardHandle, true, &status);
      }
      if (m_direction == kBothDirections) {
        HAL_SetRelay(m_reverseHandle, false, &status);
      }
      break;
    case kReverse:
      if (m_direction == kForwardOnly) {
        break;
      }
      if (m_direction == kBothDirections) {
        HAL_SetRelay(m_forwardHandle, false, &status);
      }
      if (m_direction == kBothDirections || m_direction == kReverseOnly) {
        HAL_SetRelay(m_reverseHandle, true, &status);
      }
      break;
  }
}

#ifdef TEST_TEST

int main(){
	
}

#endif
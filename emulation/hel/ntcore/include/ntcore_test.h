/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016. All Rights Reserved.                             */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#ifndef NTCORE_TEST_H_
#define NTCORE_TEST_H_

#include "ntcore.h"

// Functions in this header are to be used only for testing

extern "C" {
struct NT_String* NT_GetStringForTesting(const char* string, int* struct_size);
// No need for free as one already exists in main library

struct NT_EntryInfo* NT_GetEntryInfoForTesting(const char* name,
                                               enum NT_Type type,
                                               unsigned int flags,
                                               unsigned long long last_change,
                                               int* struct_size);

void NT_FreeEntryInfoForTesting(struct NT_EntryInfo* info);

struct NT_ConnectionInfo* NT_GetConnectionInfoForTesting(
    const char* remote_id, const char* remote_ip, unsigned int remote_port,
    unsigned long long last_update, unsigned int protocol_version,
    int* struct_size);

void NT_FreeConnectionInfoForTesting(struct NT_ConnectionInfo* info);

struct NT_Value* NT_GetValueBooleanForTesting(unsigned long long last_change,
                                              int val, int* struct_size);

struct NT_Value* NT_GetValueDoubleForTesting(unsigned long long last_change,
                                             double val, int* struct_size);

struct NT_Value* NT_GetValueStringForTesting(unsigned long long last_change,
                                             const char* str, int* struct_size);

struct NT_Value* NT_GetValueRawForTesting(unsigned long long last_change,
                                          const char* raw, int raw_len,
                                          int* struct_size);

struct NT_Value* NT_GetValueBooleanArrayForTesting(
    unsigned long long last_change, const int* arr, size_t array_len,
    int* struct_size);

struct NT_Value* NT_GetValueDoubleArrayForTesting(
    unsigned long long last_change, const double* arr, size_t array_len,
    int* struct_size);

struct NT_Value* NT_GetValueStringArrayForTesting(
    unsigned long long last_change, const struct NT_String* arr,
    size_t array_len, int* struct_size);
// No need for free as one already exists in the main library

struct NT_RpcParamDef* NT_GetRpcParamDefForTesting(const char* name,
                                                   const struct NT_Value* val,
                                                   int* struct_size);

void NT_FreeRpcParamDefForTesting(struct NT_RpcParamDef* def);

struct NT_RpcResultDef* NT_GetRpcResultsDefForTesting(const char* name,
                                                      enum NT_Type type,
                                                      int* struct_size);

void NT_FreeRpcResultsDefForTesting(struct NT_RpcResultDef* def);

struct NT_RpcDefinition* NT_GetRpcDefinitionForTesting(
    unsigned int version, const char* name, size_t num_params,
    const struct NT_RpcParamDef* params, size_t num_results,
    const struct NT_RpcResultDef* results, int* struct_size);
// No need for free as one already exists in the main library

struct NT_RpcCallInfo* NT_GetRpcCallInfoForTesting(
    unsigned int rpc_id, unsigned int call_uid, const char* name,
    const char* params, size_t params_len, int* struct_size);
// No need for free as one already exists in the main library
}

#endif /* NTCORE_TEST_H_ */

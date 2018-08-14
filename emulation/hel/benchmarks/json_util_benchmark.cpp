#include <benchmark/benchmark.h>
#include "json_util.hpp"
#include <iostream>

const std::string PULL_VALUE_INPUT ="{\"roborio\":{\"digital_hdrs\":[0,1,0,0,1,0,0,1,0,0],\"joysticks\":[{\"is_xbox\":0,\"type\":0,\"name\":\"Test Gamepad\",\"buttons\":0,\"button_count\":10,\"axes\":[0,0,0,0,0,0,0,0,0,0,0,0],\"axis_count\":12,\"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0],\"povs\":[0,0,0,0,0,0,0,0,0,0,0,0],\"pov_count\":12,\"outputs\":0,\"left_rumble\":0,\"right_rumble\":0},{\"is_xbox\":0,\"type\":0,\"name\":\"\",\"buttons\":0,\"button_count\":10,\"axes\":[27,16,4,92,26,57,-25,-97,85,-92,45,-118],\"axis_count\":12,\"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0],\"povs\":[0,0,0,0,0,0,0,0,0,0,0,0],\"pov_count\":12,\"outputs\":0,\"left_rumble\":0,\"right_rumble\":0},{\"is_xbox\":0,\"type\":0,\"name\":\"\",\"buttons\":0,\"button_count\":10,\"axes\":[27,16,4,92,26,57,-25,-97,85,-92,45,-118],\"axis_count\":12,\"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0],\"povs\":[0,0,0,0,0,0,0,0,0,0,0,0],\"pov_count\":12,\"outputs\":0,\"left_rumble\":0,\"right_rumble\":0},{\"is_xbox\":0,\"type\":0,\"name\":\"\",\"buttons\":0,\"button_count\":10,\"axes\":[27,16,4,92,26,57,-25,-97,85,-92,45,-118],\"axis_count\":12,\"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0],\"povs\":[0,0,0,0,0,0,0,0,0,0,0,0],\"pov_count\":12,\"outputs\":0,\"left_rumble\":0,\"right_rumble\":0},{\"is_xbox\":0,\"type\":0,\"name\":\"\",\"buttons\":0,\"button_count\":10,\"axes\":[27,16,4,92,26,57,-25,-97,85,-92,45,-118],\"axis_count\":12,\"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0],\"povs\":[0,0,0,0,0,0,0,0,0,0,0,0],\"pov_count\":12,\"outputs\":0,\"left_rumble\":0,\"right_rumble\":0},{\"is_xbox\":0,\"type\":0,\"name\":\"\",\"buttons\":0,\"button_count\":10,\"axes\":[27,16,4,92,26,57,-25,-97,85,-92,45,-118],\"axis_count\":12,\"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0],\"povs\":[0,0,0,0,0,0,0,0,0,0,0,0],\"pov_count\":12,\"outputs\":0,\"left_rumble\":0,\"right_rumble\":0}],\"digital_mxp\":[{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0}],\"match_info\":{\"event_name\":\"\",\"game_specific_message\":\"LLL\",\"match_type\":\"NONE\",\"match_number\":0,\"replay_number\":0,\"alliance_station_id\":\"RED1\",\"match_time\":0.0},\"robot_mode\":{\"mode\":\"TELEOPERATED\",\"enabled\":1,\"emergency_stopped\":0,\"fms_attached\":1,\"ds_attached\":1}}}";

const std::string PULL_OBJECT_INPUT ="\"digital_mxp\":[{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0}],\"match_info\":{\"event_name\":\"\",\"game_specific_message\":\"LLL\",\"match_type\":\"NONE\",\"match_number\":0,\"replay_number\":0,\"alliance_station_id\":\"RED1\",\"match_time\":0.0},\"robot_mode\":{\"mode\":\"TELEOPERATED\",\"enabled\":1,\"emergency_stopped\":0,\"fms_attached\":1,\"ds_attached\":1}}}";

static void BM_PullFirstObject(benchmark::State& state) {
    std::cout<<"Input: "<<PULL_OBJECT_INPUT<<"\n";
    std::string in;
    std::string object;
    for(auto _ : state){
        in = PULL_OBJECT_INPUT;
        object = hel::pullObject(in);
    }
    std::cout<<"Pulled object: "<<object<<"\n\n";
}

static void BM_PullObjectByLabel(benchmark::State& state) {
    std::cout<<"Input: "<<PULL_VALUE_INPUT<<"\n";
    std::string in;
    std::string value;
    for(auto _ : state){
        in = PULL_VALUE_INPUT;
        value = hel::pullObject("\"joysticks\"",in);
    }
    std::cout<<"Pulled object: "<<value<<"\n\n";
}

BENCHMARK(BM_PullFirstObject);
BENCHMARK(BM_PullObjectByLabel);
BENCHMARK_MAIN();

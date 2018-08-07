#include <benchmark/benchmark.h>
#include "send_data.hpp"
#include <iostream>

static void BM_SendData(benchmark::State& state) {
    for(auto _ : state){
        auto instance = hel::SendDataManager::getInstance();
        instance.first->updateShallow();
        std::string data = instance.first->serializeShallow();
        std::cout<<"Sender value: "<<instance.first->toString()<<"\n";
        instance.second.unlock();
    }
}

BENCHMARK(BM_SendData);
BENCHMARK_MAIN();

#include "gtest/gtest.h"
#include "send_data.hpp"
#include <iostream>

TEST(SendDataTest, Serialize){
    hel::SendData a = {};
    std::cout<<"Serializaton: "<<a.serialize()<<"\n";

    EXPECT_EQ(0, 0); //TODO
}


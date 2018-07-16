//#include "gtest/gtest.h"
#include "receive_data.h"
#include <iostream>

TEST(ReceiveDataTest, Deserialize){
    std::string in = "\"digital_hdrs\":[0, 1, 1, 1, 0, 1, 0, 0, 1, 0]";
    std::cout<<"In:"<<in<<"\n";

    hel::ReceiveData receiver;
    receiver.deserialize(in);

    //EXPECT_EQ(0, 0); //TODO
}


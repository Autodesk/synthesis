#include "gtest/gtest.h"
#include "receive_data.hpp"
#include <iostream>

TEST(ReceiveDataTest, Deserialize){
    std::string in = "{\"RoboRIO\": {\"digital_hdrs\":[0, 1, 1, 1, 0, 1, 0, 0, 1, 0], \"joysticks\": [{\"is_xbox\":1, \"type\":1, \"name\":\"\", \"buttons\":3, \"button_count\":2, \"axes\":[0,0,0,0,0,0,0,0,0,0,0,0], \"axis_count\":0, \"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0], \"povs\":[0,0,0,0,0,0,0,0,0,0,0,0], \"pov_count\":0, \"outputs\":0, \"left_rumble\":0, \"right_rumble\":0}, {\"is_xbox\":0, \"type\":0, \"name\":\"\", \"buttons\":0, \"button_count\":0, \"axes\":[0,0,0,0,0,0,0,0,0,0,0,0], \"axis_count\":0, \"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0], \"povs\":[0,0,0,0,0,0,0,0,0,0,0,0], \"pov_count\":0, \"outputs\":0, \"left_rumble\":0, \"right_rumble\":0}, {\"is_xbox\":0, \"type\":0, \"name\":\"\", \"buttons\":0, \"button_count\":0, \"axes\":[0,0,0,0,0,0,0,0,0,0,0,0], \"axis_count\":0, \"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0], \"povs\":[0,0,0,0,0,0,0,0,0,0,0,0], \"pov_count\":0, \"outputs\":0, \"left_rumble\":0, \"right_rumble\":0}, {\"is_xbox\":0, \"type\":0, \"name\":\"\", \"buttons\":0, \"button_count\":0, \"axes\":[0,0,0,0,0,0,0,0,0,0,0,0], \"axis_count\":0, \"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0], \"povs\":[0,0,0,0,0,0,0,0,0,0,0,0], \"pov_count\":0, \"outputs\":0, \"left_rumble\":0, \"right_rumble\":0}, {\"is_xbox\":0, \"type\":0, \"name\":\"\", \"buttons\":0, \"button_count\":0, \"axes\":[0,0,0,0,0,0,0,0,0,0,0,0], \"axis_count\":0, \"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0], \"povs\":[0,0,0,0,0,0,0,0,0,0,0,0], \"pov_count\":0, \"outputs\":0, \"left_rumble\":0, \"right_rumble\":0}, {\"is_xbox\":0, \"type\":0, \"name\":\"\", \"buttons\":0, \"button_count\":0, \"axes\":[0,0,0,0,0,0,0,0,0,0,0,0], \"axis_count\":0, \"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0], \"povs\":[0,0,0,0,0,0,0,0,0,0,0,0], \"pov_count\":0, \"outputs\":0, \"left_rumble\":0, \"right_rumble\":0}], \"digital_mxp\":[{\"config\":\"DO\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.142500}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}, {\"config\":\"DI\", \"value\":0.000000}]}}";
    std::cout<<"Input: "<<in<<"\n\n";

    hel::ReceiveData receiver;
    receiver.deserialize(in);
    std::cout<<"Receiver value: "<<receiver.toString()<<"\n";

    EXPECT_EQ(0, 0); //TODO
}


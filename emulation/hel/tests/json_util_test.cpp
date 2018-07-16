#include "gtest/gtest.h"
#include "json_util.h"
#include <iostream>

struct Joystick{
    bool is_xbox;
    std::string name;
};

std::ostream& operator<<(std::ostream& o, Joystick a){
    return o<<"Joystick(is_xbox:"<<a.is_xbox<<" name:"<<a.name<<")";
}

TEST(JSONUtilTest, DeserializeTest){
	{
        //std::string in = "[{\"config\":\"DIO\",\"value\":0.010000},{\"config\":\"DIO\",\"value\":0.020000},{\"config\":\"DIO\",\"value\":0.030000}]";
        std::string in = "\"can\":\"no\",\"dio\":[0, 1,1,1,  0],\"pwm\":0.500,\"joystick\":{\"is_xbox\":1,\"name\":Maxwell}";
        std::cout<<"In:"<<in<<"\n";
        for(auto a: hel::splitObject(in)){
            std::cout<<a<<"\n";
        }
    }
    std::cout<<"\n=========================================================================\n\n";
    {
        std::string in = "\"can\":\"no\",\"dio\":[0, 1,1,1,  0],\"pwm\":0.500,\"joystick\":{\"is_xbox\":1,\"name\":Maxwell}";
        std::cout<<"In:"<<in<<"\n";
        Joystick joystick = hel::pullValue(
            "\"joystick\"", 
            in,
            std::function<Joystick(std::string)>([&](std::string i){
                Joystick j;
                j.is_xbox = std::stoi(hel::pullValue("\"is_xbox\"",i));
                j.name = hel::pullValue("\"name\"",i);
                return j;
            })
        );
        std::cout<<"Joystick received:"<<joystick<<"\n";

        float pwm = hel::pullValue(
            "\"pwm\"", 
            in,
            std::function<float(std::string)>([&](std::string i){
                return std::stof(i);
            })
        );
        std::cout<<"Pwm received:"<<pwm<<"\n";
        EXPECT_EQ(pwm, 0.5);

        for(bool a: hel::deserializeList(
            hel::pullValue("\"dio\"", in),
            std::function<bool(std::string)>([&](std::string i){
                return (bool)std::stoi(i);
            }),
            true
            )
        ){
            std::cout<<"DIO received:"<<a<<"\n";
        }
        std::string can = hel::pullValue("\"can\"", in);
        std::cout<<"CAN received:"<<can<<"\n";
        EXPECT_EQ(can, "\"no\"");

        std::cout<<"Remaining:"<<in<<"\n";
    }

}


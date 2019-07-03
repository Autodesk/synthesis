#include "testing.hpp"
#include "json_util.hpp"

struct Joystick{
    bool is_xbox;
    std::string name;
};

std::ostream& operator<<(std::ostream& o, Joystick a){
    return o<<"Joystick(is_xbox:"<<a.is_xbox<<" name:\""<<a.name<<"\")";
}

TEST(JSONUtilTest, DeserializeTest){
	{
        //std::string in = "[{\"config\":\"DIO\",\"value\":0.010000},{\"config\":\"DIO\",\"value\":0.020000},{\"config\":\"DIO\",\"value\":0.030000}]";
        std::string in = "\"can\":\"no\",\"dio\":[0, 1,1,1,  0],\"pwm\":0.500,\"joystick\":{\"is_xbox\":1,\"name\":\"Maxwell\"}";
        std::cout<<"In:"<<in<<"\n";
        for(auto a: hel::splitObject(in)){
            std::cout<<a<<"\n";
        }
    }
    std::cout<<"\n=========================================================================\n\n";
    {
        std::string in = "\"can\":\"no\",\"dio\":[0, 1,1,1,  0],\"pwm\":0.500,\"joystick\":{\"is_xbox\":1,\"name\":\"Maxwell\"}";
        std::cout<<"In:"<<in<<"\n";
        Joystick joystick = hel::pullObject(
            "\"joystick\"", 
            in,
            std::function<Joystick(std::string)>([&](std::string i){
                Joystick j;
                std::string xbox_temp = hel::pullObject("\"is_xbox\"",i);
                std::cout<<"\nis_xbox:\""<<xbox_temp<<"\"\n";
                j.is_xbox = std::stoi(xbox_temp);
                j.name = hel::unquote(hel::pullObject("\"name\"",i));
                return j;
            })
        );
        std::cout<<"Joystick received:"<<joystick<<"\n";

        for(bool a: hel::deserializeList(
            hel::pullObject("\"dio\"", in),
            std::function<bool(std::string)>([&](std::string i){
                return (bool)std::stoi(i);
            }),
            true
            )
        ){
            std::cout<<"DIO received:"<<a<<"\n";
        }
        std::string can = hel::pullObject("\"can\"", in);
        std::cout<<"CAN received:"<<can<<"\n";
        EXPECT_EQ(can, "\"no\"");

        float pwm = hel::pullObject(
            "\"pwm\"", 
            in,
            std::function<float(std::string)>([&](std::string i){
                                                  return std::stof(i);
                                              })
            );
        std::cout<<"Pwm received:"<<pwm<<"\n";
        EXPECT_EQ(pwm, 0.5);

        std::cout<<"Remaining:"<<in<<"\n";
    }

}


TEST(JSONUtilTest, FormatJSONTest){
    const std::string INPUT = "{\"mode\":\"TELEOPERATED\", \"enabled\":0, \"emergency_stopped\":0, \"fms_attached\":0, \"ds_attached\":1,\"arr\": [{\"a_channel\":0, \"a_type\":\"DI\", \"b_channel\":0, \"b_type\":\"DI\", \"ticks\":0}, {\"a_channel\":0, \"a_type\":\"DI\", \"b_channel\":0, \"b_type\":\"DI\", \"ticks\":0}, {\"a_channel\":0, \"a_type\":\"DI\", \"b_channel\":0, \"b_type\":\"DI\", \"ticks\":0}]}";
    std::cout<<"Formated JSON:\n"<<hel::formatJSON(INPUT)<<"\n";
    
}

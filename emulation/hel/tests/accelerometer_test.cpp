#include "testing.hpp"

TEST(Accelerometer, convertAccel) {
    hel::Accelerometer accelerometer;

    std::pair<uint8_t, uint8_t> raw_accel = {0b01100001,0b11010000}; //NI FPGA loses first 4 bits of second byte in conversion, so leave them zero
    std::cout<<"Raw acceleration: ["<<((int)raw_accel.first)<<","<<((int)raw_accel.second)<<"]\n";

    float acceleration = accelerometer.convertAccel(raw_accel);
    std::cout<<"Converted: "<<acceleration<<"\n";

    std::pair<uint8_t, uint8_t> converted_accel = accelerometer.convertAccel(acceleration);
    std::cout<<"Returned: ["<<((int)converted_accel.first)<<","<<((int)converted_accel.second)<<"]\n";

    EXPECT_EQ(raw_accel, converted_accel);
}

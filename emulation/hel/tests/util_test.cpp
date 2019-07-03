#include "testing.hpp"

#include <vector>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(UtilTest, findMostSignificantBit){
    constexpr uint32_t LOC = 9;
    uint32_t a = 1u << LOC;

    EXPECT_EQ(LOC, hel::findMostSignificantBit(a));
}

TEST(UtilTest, checkBitHigh){
    const std::vector<unsigned> locs = {0, 5, 9};

    uint32_t a = 0;
    for(const unsigned& loc: locs){
        a |= 1u << loc;
    }

    for(unsigned i = 0; i < sizeof(a) * 8; i++){
        if(std::find(locs.begin(), locs.end(), i) != locs.end()){
            EXPECT_TRUE(hel::checkBitHigh(a, i));
        } else {
            EXPECT_FALSE(hel::checkBitHigh(a, i));
        }
    }
}

TEST(UtilTest, setBit){
    constexpr unsigned I = 3;
    uint32_t a = 0;
    a = hel::setBit(a, true, I);

    EXPECT_TRUE(hel::checkBitHigh(a, I));

    a = hel::setBit(a, false, I);

    EXPECT_TRUE(hel::checkBitLow(a, I));
}

TEST(UtilTest, checkBits){ // TODO delete?
    uint32_t a = 0b11000;
    uint32_t b = 0b11000;
    uint32_t c = 0b01001;
    uint32_t comparison_mask_1 = 0b11111;
    uint32_t base_talon = 0x02040000;
    uint32_t send_talon = 33816705;
    uint32_t comparison_mask_2 = 0b11000001000000000000000000;

    EXPECT_EQ(true, hel::compareBits(a,b,comparison_mask_1));
    EXPECT_EQ(false, hel::compareBits(a,c,comparison_mask_1));
    EXPECT_EQ(true, hel::compareBits(send_talon,base_talon,comparison_mask_2));
}

TEST(UtilTest, Maybe){
    hel::Maybe<int> a;

    EXPECT_FALSE(a);
    EXPECT_DEATH(a.get(), "Assertion");

    constexpr int VAL = 5;
    a = VAL;

    EXPECT_TRUE(a);
    EXPECT_EQ(VAL, a.get());

    const std::function<int(int)> f = [](int x){ return x + 5; };
    const auto lifted = hel::Maybe<int>::lift<int>(f);

    EXPECT_EQ(f(VAL), a.fmap(lifted).get());
}

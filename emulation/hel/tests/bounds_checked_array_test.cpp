#include "testing.hpp"

#include <numeric>

TEST(BoundsCheckedArrayTest, DefaultConstructor){
    constexpr unsigned LEN = 10;

    constexpr unsigned VAL = 20;
    hel::BoundsCheckedArray<int, LEN> b(VAL);

    for(unsigned i = 0; i < LEN; i++){
        EXPECT_EQ(VAL, b[i]);
    }
}

TEST(BoundsCheckedArrayTest, IterableConstructor){
    constexpr unsigned LEN = 10;

    std::array<int, LEN> a;
    std::iota(a.begin(), a.end(), 0);

    hel::BoundsCheckedArray<int, LEN> b = a;

    for(unsigned i = 0; i < LEN; i++){
        EXPECT_EQ(a[i], b[i]);
    }
}

TEST(BoundsCheckedArrayTest, Equality){
    constexpr unsigned LEN = 10;

    hel::BoundsCheckedArray<int, LEN> a(0);
    EXPECT_EQ(a, a);

    hel::BoundsCheckedArray<int, LEN> b(1);
    EXPECT_NE(a, b);
}

TEST(BoundsCheckedArrayTest, Access){
    constexpr unsigned LEN = 10;

    constexpr unsigned VAL = 5;
    hel::BoundsCheckedArray<int, LEN> b(VAL);
    EXPECT_EQ(VAL, b[0]);
    EXPECT_EQ(VAL, b.at(0));

    EXPECT_THROW(b[LEN], std::out_of_range);
    EXPECT_THROW(b.at(LEN), std::out_of_range);
}

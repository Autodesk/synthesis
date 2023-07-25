#ifndef SYNTHESIS_CEF_DEBUG_H_
#define SYNTHESIS_CEF_DEBUG_H_
#pragma once

#include <iostream>
#include <algorithm>
#include <ostream>
#include <string>

#if SYNTHESIS_CEF_DEBUG
template <typename T_container, typename T = typename std::enable_if<!std::is_same<T_container, std::string>::value,
                                    typename T_container::value_type>::type>
std::ostream& operator<<(std::ostream& os, const T_container& container) {
    os << '{';
    std::string sep;

    for (const T& item : container) {
        os << sep << item, sep = ", ";
    }

    return os << '}';
}

template <typename A, typename B>
std::ostream& operator<<(std::ostream& os, const std::pair<A, B>& p) {
    return os << '(' << p.first << ", " << p.second << ')';
}

void dbg_out() {
    std::cout << std::endl;
}

template <typename Head, typename... Tail>
void dbg_out(Head A, Tail... B) {
    std::cout << ' ' << A;
    dbg_out(B...);
}

#define SYNTHESIS_DEBUG_LOG(...) std::cout << "--- SYNTHESIS DBG MSG --- [" << #__VA_ARGS__ << "]:", dbg_out(__VA_ARGS__)
#else // ^^^ SYNTHESIS_CEF_DEBUG ^^^ / vvv !SYNTHESIS_CEF_DEBUG vvv
#define SYNTHESIS_DEBUG_LOG(...)
#endif // SYNTHESIS_CEF_DEBUG

#endif // SYNTHESIS_CEF_DEBUG_H_

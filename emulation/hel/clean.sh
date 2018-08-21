#!/bin/bash

case "$1" in
    "emulator")
        killall qemu-system-arm &> /dev/null;
        rm -rf vm-package vm-package.tar.gz vm_lock;
        ;;
    "cmake")
        rm -rf CMakeFiles CMakeCache.txt cmake_install.cmake Makefile CMakeDoxyfile.in CMakeDoxygenDefaults.cmake;
        ;;
    "gtest")
        rm -rf GoogleTest-prefix bin/benchmarks/* bin/tests/* lib/lib lib/lib64 lib/google_test lib/GoogleBench lib/include;
        ;;
    "wpilib")
        rm -rf lib/wpilib;
        ;;
    "ctre")
        rm -rf lib/ctre;
        ;;
    "asio")
        rm -rf lib/ASIO;
        ;;
    "hel")
        rm -rf build/* lib/libhel.so;
        ;;
    "user-code")
        rm -rf user-code/*;
        ;;
    "docs")
        rm -rf docs/html/* docs/latex/*;
        ;;
    "all")
        killall qemu-system-arm;
        rm -rf vm-package vm-package.tar.gz vm_lock CMakeFiles CMakeCache.txt cmake_install.cmake Makefile CMakeDoxyfile.in CMakeDoxygenDefaults.cmake GoogleTest-prefix bin/benchmarks/* bin/tests/* lib/lib lib/lib64 lib/google_test lib/GoogleBench lib/include lib/wpilib lib/ctre lib/ASIO build/* lib/libhel.so docs/html/* docs/latex/*
        ;;
    *)
        printf "Skipping clean. No target specified.\n"
        ;;
esac

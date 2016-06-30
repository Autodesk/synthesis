#Aardvark wpilib project.
this is a custom implementation of the allwpilb repository.
this library is supposed to allow programmers to write a program and test even
before the original robot is built. this library is designed to implenent 
the three layers of the wpilibrary.
the repo is made of 4 seperate projects: Fakefpga, Aardvark-wpilib, setup loopback adapter, simple template. 


the most important pieces of the project are Fake fpga and Aardvark-wpilib.
the Fakefpga handles communication between the unity and the user code.
Aardvark-wpilib handles the communication between the Fakefpga and driverStation.
if are looking to understanding the structure of code I 
recommend looking at Aardvark-WPILib\FakeFPGA\src\Emulator.cpp and 
Aardvark-WPILib\Aardvark-WPILib\src

###warning
this library is not up to date with current wpilip.
it is not documenteted very well. tread lightly. 
this library has been discontinued from development.
the bxd synthesis team will not add any patches to this library.
if you use this library  you use at your own risk!
the java and c++ version are out of sync!


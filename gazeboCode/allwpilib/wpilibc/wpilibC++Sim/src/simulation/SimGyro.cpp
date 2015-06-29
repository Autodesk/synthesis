
#include "simulation/SimGyro.h"
#include "simulation/MainNode.h"

SimGyro::SimGyro(std::string topic) {
    commandPub = MainNode::Advertise<msgs::GzString>("~/simulator/"+topic+"/control");
  
    posSub = MainNode::Subscribe("~/simulator/"+topic+"/position",
                                 &SimGyro::positionCallback, this);
    velSub = MainNode::Subscribe("~/simulator/"+topic+"/velocity",
                                 &SimGyro::velocityCallback, this);

    if (commandPub->WaitForConnection(gazebo::common::Time(5.0))) { // Wait up to five seconds.
		std::cout << "Initialized ~/simulator/" + topic << std::endl;
	} else {
		std::cerr << "Failed to initialize ~/simulator/" + topic + ": does the gyro exist?" << std::endl;
	}
}

void SimGyro::Reset() {
    sendCommand("reset");
}

double SimGyro::GetAngle() {
    return position;
}

double SimGyro::GetVelocity() {
    return velocity;
}


void SimGyro::sendCommand(std::string cmd) {
  msgs::GzString msg;
  msg.set_data(cmd);
  commandPub->Publish(msg);
}


void SimGyro::positionCallback(const msgs::ConstFloat64Ptr &msg) {
    position = msg->data();
}

void SimGyro::velocityCallback(const msgs::ConstFloat64Ptr &msg) {
    velocity = msg->data();
}

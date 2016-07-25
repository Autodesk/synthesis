#include "potentiometer.h"

#include <gazebo/physics/physics.hh>
#include <gazebo/transport/transport.hh>
#include <boost/algorithm/string.hpp>

#include "msgs/msgs.h"

GZ_REGISTER_MODEL_PLUGIN(Potentiometer)

Potentiometer::Potentiometer() {}

Potentiometer::~Potentiometer() {}

void Potentiometer::Load(physics::ModelPtr model, sdf::ElementPtr sdf) {
  this->model = model;

  // Parse SDF properties
  joint = model->GetJoint(sdf->Get<std::string>("joint"));
  if (sdf->HasElement("topic")) {
    topic = sdf->Get<std::string>("topic");
  } else {
    topic = "~/"+sdf->GetAttribute("name")->GetAsString();
  }
  
  if (sdf->HasElement("units")) {
    radians = sdf->Get<std::string>("units") != "degrees";
  } else {
    radians = true;
  }

  gzmsg << "Initializing potentiometer: " << topic << " joint=" << joint->GetName()
        << " radians=" << radians << std::endl;

  // Connect to Gazebo transport for messaging
  std::string scoped_name = model->GetWorld()->GetName()+"::"+model->GetScopedName();
  boost::replace_all(scoped_name, "::", "/");
  node = transport::NodePtr(new transport::Node());
  node->Init(scoped_name);
  pub = node->Advertise<msgs::Float64>(topic);

  // Connect to the world update event.
  // This will trigger the Update function every Gazebo iteration
  updateConn = event::Events::ConnectWorldUpdateBegin(boost::bind(&Potentiometer::Update, this, _1));
}

void Potentiometer::Update(const common::UpdateInfo &info) {
  joint->GetAngle(0).Normalize();
  msgs::Float64 msg;
  if (radians) {
    msg.set_data(joint->GetAngle(0).Radian());
  } else {
    msg.set_data(joint->GetAngle(0).Degree());
  }
  pub->Publish(msg);
}

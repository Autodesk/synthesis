#include "Joint.h"
#include <Fusion/Components/JointGeometry.h>
#include <Fusion/Components/AsBuiltJoint.h>
#include <Core/Geometry/Point3D.h>
#include "RigidNode.h"
#include "JointSensor.h"
#include "ConfigData.h"

using namespace BXDJ;

Joint::Joint(const Joint & jointToCopy)
{
	parentOcc = jointToCopy.parentOcc;
	fusionJoint = jointToCopy.fusionJoint;
	fusionAsBuiltJoint = jointToCopy.fusionAsBuiltJoint;
	parent = jointToCopy.parent;
	child = jointToCopy.child;

	if (jointToCopy.driver != nullptr)
		setDriver(*jointToCopy.driver);
	else
		driver = nullptr;
}

Joint::Joint(RigidNode * parent, core::Ptr<fusion::Joint> fusionJoint, core::Ptr<fusion::Occurrence> parentOccurrence)
{
	parentOcc = (fusionJoint->occurrenceOne() == parentOccurrence) ? ONE : TWO;

	this->fusionJoint = fusionJoint;

	if (parent == NULL)
		throw "Parent node cannot be NULL!";

	this->parent = parent;
	this->child = std::make_shared<RigidNode>((parentOcc == ONE ? fusionJoint->occurrenceTwo() : fusionJoint->occurrenceOne()), this);
	driver = nullptr;
}

Joint::Joint(RigidNode * parent, core::Ptr<fusion::AsBuiltJoint> fusionJoint, core::Ptr<fusion::Occurrence> parentOccurrence)
{
	parentOcc = (fusionJoint->occurrenceOne() == parentOccurrence) ? ONE : TWO;

	this->fusionAsBuiltJoint = fusionJoint;

	if (parent == NULL)
		throw "Parent node cannot be NULL!";

	this->parent = parent;
	this->child = std::make_shared<RigidNode>((parentOcc == ONE ? fusionJoint->occurrenceTwo() : fusionJoint->occurrenceOne()), this);
	driver = nullptr;
}

RigidNode * Joint::getParent() const
{
	return parent;
}

std::shared_ptr<RigidNode> Joint::getChild() const
{
	return child;
}

Vector3<> Joint::getParentBasePoint() const
{
	core::Ptr<fusion::JointGeometry> geometry;

	if (fusionJoint != nullptr)
	{
		geometry = (parentOcc ? fusionJoint->geometryOrOriginOne() : fusionJoint->geometryOrOriginTwo());

		if (geometry == nullptr || geometry->origin() == nullptr)
			geometry = (!parentOcc ? fusionJoint->geometryOrOriginOne() : fusionJoint->geometryOrOriginTwo());
	}
	else
		geometry = fusionAsBuiltJoint->geometry();

	if (geometry == nullptr || geometry->origin() == nullptr)
		return Vector3<>(0, 0, 0);

	return Vector3<>(geometry->origin()->x(), geometry->origin()->y(), geometry->origin()->z());
}

Vector3<> Joint::getChildBasePoint() const
{
	core::Ptr<fusion::JointGeometry> geometry;

	if (fusionJoint != nullptr)
	{
		geometry = (parentOcc ? fusionJoint->geometryOrOriginTwo() : fusionJoint->geometryOrOriginOne());
		
		if (geometry == nullptr || geometry->origin() == nullptr)
			geometry = (!parentOcc ? fusionJoint->geometryOrOriginTwo() : fusionJoint->geometryOrOriginOne());
	}
	else
		geometry = fusionAsBuiltJoint->geometry();

	if (geometry == nullptr || geometry->origin() == nullptr)
		return Vector3<>(0, 0, 0);

	return Vector3<>(geometry->origin()->x(), geometry->origin()->y(), geometry->origin()->z());
}

void Joint::setDriver(Driver driver)
{
	this->driver = std::make_unique<Driver>(driver);
}

void Joint::setNoDriver()
{
	this->driver = nullptr;
}

std::unique_ptr<Driver> Joint::getDriver() const
{
	if (this->driver != nullptr)
		return std::make_unique<Driver>(*driver);
	else
		return nullptr;
}

void Joint::addSensor(JointSensor sensor)
{
	sensors.push_back(std::make_shared<JointSensor>(sensor));
}

void Joint::clearSensors()
{
	sensors.clear();
}

std::unique_ptr<Driver> Joint::searchDriver(const ConfigData & config)
{
	if (fusionJoint != nullptr)
		return config.getDriver(fusionJoint);
	else
		return config.getDriver(fusionAsBuiltJoint);
}

std::vector<std::shared_ptr<JointSensor>> Joint::searchSensors(const ConfigData & config)
{
	if (fusionJoint != nullptr)
		return config.getSensors(fusionJoint);
	else
		return config.getSensors(fusionAsBuiltJoint);
}

void Joint::write(XmlWriter & output) const
{
	if (driver != nullptr)
		output.write(*driver);

	for (std::shared_ptr<JointSensor> sensor : sensors)
		output.write(*sensor);
}

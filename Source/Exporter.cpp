#include "Exporter.h"
#include "Data/Filesystem.h"
#include "Data/BXDA/Mesh.h"
#include "Data/BXDA/SubMesh.h"
#include "Data/BXDA/Vertex.h"
#include "Data/BXDA/Surface.h"
#include "Data/BXDA/Triangle.h"
#include "Data/BXDJ/RigidNode.h"

using namespace Synthesis;

Exporter::Exporter(Ptr<Application> app) : fusionApplication(app)
{}

Exporter::~Exporter()
{}

std::vector<Ptr<Joint>> Exporter::collectJoints()
{
	Ptr<FusionDocument> document = fusionApplication->activeDocument();

	std::string stringifiedJoints;

	std::vector<Ptr<Joint>> allJoints = document->design()->rootComponent()->allJoints();
	std::vector<Ptr<Joint>> joints;

	for (int j = 0; j < allJoints.size(); j++)
	{
		Ptr<Joint> joint = allJoints[j];

		JointTypes type = joint->jointMotion()->jointType();

		// Check if joint is supported to have drivers
		if (type == JointTypes::RevoluteJointType ||
			type == JointTypes::SliderJointType ||
			type == JointTypes::CylindricalJointType)
			joints.push_back(joint);
	}

	return joints;
}

std::string Exporter::stringifyJoints(std::vector<Ptr<Joint>> joints)
{
	std::string stringifiedJoints = "";

	for (Ptr<Joint> joint : joints)
	{
		stringifiedJoints += std::to_string(joint->name().length()) + " " + joint->name() + " ";

		JointTypes type = joint->jointMotion()->jointType();

		// Specify if joint supports linear and/or angular motion
		// Angular motion only
		if (type == JointTypes::RevoluteJointType)         
			stringifiedJoints += 5;

		// Linear motion only
		else if (type == JointTypes::SliderJointType ||
				 type == JointTypes::CylindricalJointType)      
			stringifiedJoints += 6;

		// Both angular and linear motion
		else if (false)
			stringifiedJoints += 7;

		// Neither angular nor linear motion
		else                                                                            
			stringifiedJoints += 4;

		stringifiedJoints += " ";
	}

	return stringifiedJoints;
}

void Exporter::exportExample()
{
	BXDA::Mesh mesh = BXDA::Mesh(Guid());
	BXDA::SubMesh subMesh = BXDA::SubMesh();

	// Face
	std::vector<BXDA::Vertex> vertices;
	vertices.push_back(BXDA::Vertex(Vector3<>(1, 2, 3), Vector3<>(1, 0, 0)));
	vertices.push_back(BXDA::Vertex(Vector3<>(4, 5, 6), Vector3<>(1, 0, 0)));
	vertices.push_back(BXDA::Vertex(Vector3<>(7, 8, 9), Vector3<>(1, 0, 0)));
	
	subMesh.addVertices(vertices);

	// Surface
	BXDA::Surface surface;
	std::vector<BXDA::Triangle> triangles;
	triangles.push_back(BXDA::Triangle(0, 1, 2));
	surface.addTriangles(triangles);
	surface.setColor(255, 16, 0);

	subMesh.addSurface(surface);
	mesh.addSubMesh(subMesh);

	//Generates timestamp and attaches to file name
	std::string filename = Filesystem::getCurrentRobotDirectory() + "exampleFusion.bxda";
	BXDA::BinaryWriter binary(filename);
	binary.write(mesh);
}

void Exporter::exportExampleXml()
{
	std::string filename = Filesystem::getCurrentRobotDirectory() + "exampleFusionXml.bxdj";
	BXDJ::XmlWriter xml(filename, false);
	xml.startElement("BXDJ");
	xml.writeAttribute("Version", "3.0.0");
	xml.startElement("Node");
	xml.writeAttribute("GUID", "0ba8e1ce-1004-4523-b844-9bfa69efada9");
	xml.writeElement("ParentID", "-1");
	xml.writeElement("ModelFileName", "node_0.bxda");
	xml.writeElement("ModelID", "Part2:1");
}

void Exporter::exportMeshes(BXDJ::ConfigData config)
{
	Ptr<UserInterface> userInterface = fusionApplication->userInterface();
	Ptr<FusionDocument> document = fusionApplication->activeDocument();
	
	// Generate tree
	Guid::resetAutomaticSeed();
	BXDJ::RigidNode rootNode(document->design()->rootComponent(), config);

	// Write robot to file
	Filesystem::createDirectory(Filesystem::getCurrentRobotDirectory());
	std::string filename = Filesystem::getCurrentRobotDirectory() + "skeleton.bxdj";
	BXDJ::XmlWriter xml(filename, false);

	xml.startElement("BXDJ");
	xml.writeAttribute("Version", "3.0.0");
	xml.write(rootNode);
	xml.endElement();
}

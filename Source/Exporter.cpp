#include "Exporter.h"

using namespace Synthesis;

Exporter::Exporter(Ptr<Application> app) : fusionApplication(app)
{}

Exporter::~Exporter()
{}

void Exporter::exportExample()
{
	BXDA::Mesh mesh = BXDA::Mesh();
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

	subMesh.addSurface(surface);
	mesh.addSubMesh(subMesh);

	//Generates timestamp and attaches to file name
	std::string filename = "C:\\Users\\t_walkn\\Desktop\\exampleFusion.bxda";
	BXDA::BinaryWriter binary(filename);
	binary.write(mesh);
}

void Exporter::exportExampleXml()
{
	std::string filename = "C:\\Users\\t_walkn\\Desktop\\exampleFusionXml.bxdj";
	BXDJ::XmlWriter xml(filename, false);
	xml.writeElement("test", "value");
	xml.startElement("document");
	xml.writeElement("inner", "value");
	xml.writeAttribute("attr", "attrVal");
	xml.endElement();
}

void Exporter::exportMeshes()
{
	Ptr<UserInterface> userInterface = fusionApplication->userInterface();
	Ptr<FusionDocument> document = fusionApplication->activeDocument();
	
	BXDJ::RigidNode rootNode(document->design()->rootComponent());

	std::string filename = "C:\\Users\\t_walkn\\Desktop\\node_0.bxda";
	BXDA::BinaryWriter binary(filename);
	BXDA::Mesh mesh;
	rootNode.getMesh(mesh);
	binary.write(mesh);
}

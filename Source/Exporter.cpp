#include "Exporter.h"

using namespace Synthesis;

Exporter::Exporter(Ptr<Application> app) : fusionApplication(app)
{}

Exporter::~Exporter()
{}

void Exporter::exportMeshes()
{
	Ptr<FusionDocument> document = fusionApplication->activeDocument();
	Ptr<UserInterface> userInterface = fusionApplication->userInterface();

	BXDA::Mesh mesh = BXDA::Mesh();

	for (Ptr<Component> comp : document->design()->allComponents())
	{
		BXDA::SubMesh subMesh = BXDA::SubMesh();

		for (Ptr<BRepBody> m_bod : comp->bRepBodies())
		{
			Ptr<TriangleMeshCalculator> meshCalculator = m_bod->meshManager()->createMeshCalculator();
			meshCalculator->setQuality(LowQualityTriangleMesh);

			Ptr<TriangleMesh> fusionMesh = meshCalculator->calculate();

			// Add vertices to sub-mesh
			std::vector<BXDA::Vertex> vertices(fusionMesh->nodeCount());
			std::vector<double> coords = fusionMesh->nodeCoordinatesAsDouble();
			std::vector<double> norms = fusionMesh->normalVectorsAsDouble();

			for (int v = 0; v < coords.size(); v += 3)
				vertices[v/3] = BXDA::Vertex(Vector3<>(coords[v], coords[v + 1], coords[v + 2]), Vector3<>(norms[v], norms[v + 1], norms[v + 2]));

			subMesh.addVertices(vertices);

			// Add faces to sub-mesh
			std::vector<int> indices = fusionMesh->nodeIndices();
			subMesh.addSurface(BXDA::Surface(indices));
		}

		mesh.addSubMesh(subMesh);
	}

	//Generates timestamp and attaches to file name
	std::string filename = "node_0.bxda";
	BXDA::BinaryWriter binary(filename);
	binary.write(mesh);

	userInterface->messageBox(mesh.toString());
}

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

void Exporter::buildNodeTree()
{
	std::string output;

	Ptr<UserInterface> userInterface = fusionApplication->userInterface();
	Ptr<FusionDocument> document = fusionApplication->activeDocument();
	Ptr<OccurrenceList> rootOccurences = document->design()->rootComponent()->occurrences()->asList();
	
	BXDJ::RigidNode rootNode;

	output += "Analyzed Components: ";
	for (Ptr<Occurrence> occurence : rootOccurences)
		if (occurence->isGrounded())
			rootNode.addOccurence(occurence);

	/*std::vector<Ptr<Occurrence>> allTreeRootOccurences;

	output += "\nGrounded Occurences: ";
	for (Ptr<Occurrence> occurence : groundedOccurences)
	{
		output += occurence->name() + "\n";

		allTreeRootOccurences.push_back(occurence);

		output += "Rigid Groups: ";
		for (Ptr<RigidGroup> rigidGroup : occurence->rigidGroups())
		{
			output += rigidGroup->name() + "\n";

			for (Ptr<Occurrence> rigidGroupOccurence : rigidGroup->occurrences())
				allTreeRootOccurences.push_back(rigidGroupOccurence);
		}

		output += "Rigid Joints: ";
		for (Ptr<Joint> joint : occurence->joints())
		{
			if (joint->jointMotion()->jointType() == JointTypes::RigidJointType)
			{
				allTreeRootOccurences.push_back(joint->occurrenceOne());
				allTreeRootOccurences.push_back(joint->occurrenceTwo());
			}
		}

		output += "\n";
	}

	output += "\nAll Root Occurences: ";
	for (Ptr<Occurrence> occurence : allTreeRootOccurences)
	{
		output += occurence->name() + "\n";
	}*/

	std::string filename = "C:\\Users\\t_walkn\\Desktop\\exampleFusion.bxda";
	BXDA::BinaryWriter binary(filename);
	BXDA::Mesh mesh;
	rootNode.getMesh(mesh);
	binary.write(mesh);

	userInterface->messageBox(output);
}

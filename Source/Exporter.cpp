#include "Exporter.h"

using namespace Synthesis;

Exporter::Exporter(Ptr<Application> app) : _app(app)
{
	_ui = _app->userInterface();
	//outFile = spdlog::stdout_color_mt("console");
}

Exporter::~Exporter()
{}

void Exporter::loadMeshes()
{
	Ptr<FusionDocument> doc = _app->activeDocument();

	std::string a = "";

	BXDA::Mesh * mesh = new BXDA::Mesh();

	//LVector3 * _verts = new LVector3();
	//LVector3 * _norms = new LVector3();


	//Generates timestamp and attaches to file name
	time_t now = time(NULL);
	tm * ptm = localtime(&now);
	char buffer[32];
	// Format: 20:20:00
	strftime(buffer, 32, "%H.%M.%S", ptm);

	std::string filename = doc->name() + "_" + buffer + ".bxda";

	BinaryWriter * binary = new BinaryWriter(filename);

	//Vector3 * _temp = new Vector3();
	//Vector3 * _temp2 = new Vector3();

	for (Ptr<Component> comp : doc->design()->allComponents())
	{
		BXDA::SubMesh * subMesh = new BXDA::SubMesh();

		for (Ptr<BRepBody> m_bod : comp->bRepBodies())
		{
			Ptr<TriangleMeshCalculator> meshCalculator = m_bod->meshManager()->createMeshCalculator();
			meshCalculator->setQuality(LowQualityTriangleMesh);

			Ptr<TriangleMesh> fusionMesh = meshCalculator->calculate();

			// Add vertices to sub-mesh
			std::vector<BXDA::Vertex> vertices(fusionMesh->nodeCount() * 3);
			std::vector<double> coords = fusionMesh->nodeCoordinatesAsDouble();
			std::vector<double> norms = fusionMesh->normalVectorsAsDouble();

			for (int v = 0; v < coords.size(); v += 3)
				vertices.push_back(BXDA::Vertex(BXDA::Vector3(coords[v], coords[v + 1], coords[v + 2]), BXDA::Vector3(norms[v], norms[v + 1], norms[v + 2])));

			subMesh->addVertices(vertices);

			// Add faces to sub-mesh
			std::vector<int> indices = fusionMesh->nodeIndices();
			subMesh->addSurface(BXDA::Surface(indices));
		}

		for (Ptr<Joint> joint : comp->allJoints())
		{
			a += "Parent of joint: " + joint->parentComponent()->name();

			a += "\n";
		}

		mesh->addSubMesh(*subMesh);

		delete subMesh;
	}

	binary->Write(mesh);

	delete mesh;

	_ui->messageBox(a);
}

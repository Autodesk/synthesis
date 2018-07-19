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
		BXDA::SubMesh * tempSubmesh = new BXDA::SubMesh();

		for (Ptr<BRepBody> m_bod : comp->bRepBodies())
		{
			Ptr<TriangleMeshCalculator> meshCalculator = m_bod->meshManager()->createMeshCalculator();
			meshCalculator->setQuality(LowQualityTriangleMesh);

			Ptr<TriangleMesh> mesh = meshCalculator->calculate();

			/*
			for (Ptr<Vector3D> ve : mesh->normalVectors())
			{
				tempSubmesh->verts.push_back(ve->x());
				tempSubmesh->verts.push_back(ve->y());
				tempSubmesh->verts.push_back(ve->z());
			}

			for (Ptr<Point3D> no : mesh->nodeCoordinates())
			{
				tempSubmesh->norms.push_back(no->x());
				tempSubmesh->norms.push_back(no->y());
				tempSubmesh->norms.push_back(no->z());
			}

			mesh->subMeshes.push_back(new SubMesh(_tempS));
			mesh->colliders.push_back(new SubMesh(_tempS));
			*/
		}

		for (Ptr<Joint> joint : comp->allJoints())
		{
			a += "Parent of joint: " + joint->parentComponent()->name();

			a += "\n";
		}

		delete tempSubmesh;
	}

	binary->Write(mesh);

	delete mesh;

	_ui->messageBox(a);
}

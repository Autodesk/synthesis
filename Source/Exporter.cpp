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

	string a = "";

	BXDA * bxda = new BXDA();

	//LVector3 * _verts = new LVector3();
	//LVector3 * _norms = new LVector3();


	//Generates timestamp and attaches to file name
	time_t now = time(NULL);
	tm * ptm = localtime(&now);
	char buffer[32];
	// Format: 20:20:00
	strftime(buffer, 32, "%H.%M.%S", ptm);

	string filename = doc->name() + "_" + buffer + ".bxda";

	BinaryWriter * binary = new BinaryWriter(filename);

	//Vector3 * _temp = new Vector3();
	//Vector3 * _temp2 = new Vector3();

	Submesh * _tempS = new Submesh();

	Ptr<TriangleMeshCalculator> calc;

	for (Ptr<Component> comp : doc->design()->allComponents())
	{
		// 
		_tempS->verts.clear();
		_tempS->verts.shrink_to_fit();
		_tempS->norms.clear();
		_tempS->norms.shrink_to_fit();

		for (Ptr<BRepBody> m_bod : comp->bRepBodies())
		{

			calc = m_bod->meshManager()->createMeshCalculator();
			calc->setQuality(LowQualityTriangleMesh);
			Ptr<TriangleMesh> mesh = calc->calculate();

			for (Ptr<Vector3D> ve : mesh->normalVectors())
			{
				_tempS->verts.push_back(ve->x());
				_tempS->verts.push_back(ve->y());
				_tempS->verts.push_back(ve->z());
			}

			for (Ptr<Point3D> no : mesh->nodeCoordinates())
			{
				_tempS->norms.push_back(no->x());
				_tempS->norms.push_back(no->y());
				_tempS->norms.push_back(no->z());
			}

			bxda->meshes.push_back(new Submesh(_tempS));
			bxda->colliders.push_back(new Submesh(_tempS));
		}

		for (Ptr<Joint> joint : comp->allJoints())
		{
			a += "Parent of joint: " + joint->parentComponent()->name();

			a += "\n";
		}
	}

	binary->Write(bxda);

	_ui->messageBox(a);
}

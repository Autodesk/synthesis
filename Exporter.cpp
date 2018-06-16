#include "Exporter.h"

using namespace Synthesis;

Exporter::Exporter(Ptr<Application> app) : _app(app) {
	_ui = _app->userInterface();
	//outFile = spdlog::stdout_color_mt("console");
}

Exporter::~Exporter() {
	
}

int Exporter::exportCommon() {
	Ptr<FusionDocument> doc = _app->activeDocument();

	string a = "";

	BXDA * bxda = new BXDA();

	LVector3 * _verts = new LVector3();
	LVector3 * _norms = new LVector3();

	BinaryWriter * binary = new BinaryWriter("out.txt");

	Vector3 * _temp = new Vector3();
	Vector3 * _temp2 = new Vector3();

	Ptr<TriangleMeshCalculator> calc;

	for (Ptr<Component> comp : doc->design()->allComponents()) {
		a += "name : " + comp->name() + "\n";
		for (Ptr<BRepBody> m_bod : comp->bRepBodies()) {

			calc = m_bod->meshManager()->createMeshCalculator();
			calc->setQuality(LowQualityTriangleMesh);
			Ptr<TriangleMesh> mesh = calc->calculate();

			for (Ptr<Vector3D> ve : mesh->normalVectors()) {
				_temp->x = ve->x();
				_temp->y = ve->y();
				_temp->z = ve->z();
				_verts->add(_temp);
			}

			for (Ptr<Point3D> no : mesh->nodeCoordinates()) {
				_temp2->x = no->x();
				_temp2->y = no->y();
				_temp2->z = no->z();
				_norms->add(_temp);
			}

			bxda->meshes.push_back(new Submesh(_verts, _norms));
			bxda->colliders.push_back(new Submesh(_verts, _norms));
		}

		a += "\n";
	}

	binary->Write(bxda);
	delete binary;					//Close the stream


	//delete _temp;
	
	_ui->messageBox(a);

	return 0;
}


int Exporter::exportWheel() {
	if (!_ui)
		return 1;


	Ptr<FusionDocument> doc = _app->activeDocument();

	string a = "";


	for (Ptr<Component> comp : doc->design()->allComponents()) {
		a += "Component : " + comp->name() + "\n";
		for (Ptr<Joint> j : comp->joints()) {
			a += j->name() + " ";
		}
		a += "\n";
	}

	_ui->messageBox(a);

	return 0;
}

void Exporter::writeToFile(string a, logLevels lvl) {

	switch (lvl)
	{
	case Synthesis::info:
		//console->info(a);
		break;
	case Synthesis::warn:
		//console->warn(a);
		break;
	case Synthesis::critikal:
		//console->critical(a);
		break;
	default:
		//console->info(a);
		break;
	}
}

/*
Ptr<CommandDefinition> Exporter::expCommand() {
	//Ptr<>
}
 */

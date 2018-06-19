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

	//LVector3 * _verts = new LVector3();
	//LVector3 * _norms = new LVector3();

	BinaryWriter * binary = new BinaryWriter("out.txt");

	//Vector3 * _temp = new Vector3();
	//Vector3 * _temp2 = new Vector3();

	Submesh * _tempS = new Submesh();

	Ptr<TriangleMeshCalculator> calc;

	for (Ptr<Component> comp : doc->design()->allComponents()) {
		a += "name : " + comp->name() + "\n";
		for (Ptr<BRepBody> m_bod : comp->bRepBodies()) {

			_tempS->verts.clear();
			_tempS->verts.shrink_to_fit();
			_tempS->norms.clear();
			_tempS->norms.shrink_to_fit();

			calc = m_bod->meshManager()->createMeshCalculator();
			calc->setQuality(LowQualityTriangleMesh);
			Ptr<TriangleMesh> mesh = calc->calculate();

			for (Ptr<Vector3D> ve : mesh->normalVectors()) {
				_tempS->verts.push_back(ve->x());
				_tempS->verts.push_back(ve->y());
				_tempS->verts.push_back(ve->z());
			}

			for (Ptr<Point3D> no : mesh->nodeCoordinates()) {
				_tempS->norms.push_back(no->x());
				_tempS->norms.push_back(no->y());
				_tempS->norms.push_back(no->z());
			}

			bxda->meshes.push_back(new Submesh(_tempS));
			bxda->colliders.push_back(new Submesh(_tempS));
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

#include "Exporter.h"

using namespace Synthesis;

Exporter::Exporter(Ptr<Application> app) : _app(app) {
	_ui = _app->userInterface();
}

Exporter::Exporter() {

}

Exporter::~Exporter() {

}

int Exporter::exportCommon() {
	Ptr<FusionDocument> doc = _app->activeDocument();

	string a = "";

	for (Ptr<Component> comp : doc->design()->allComponents()) {
		a += "name : " + comp->name() + "\n";
		a += "Total bodies in component : " + (int)comp->bRepBodies()->count();
		for (Ptr<BRepBody> m_bod : comp->bRepBodies()) {
			a += "\t";
			Ptr<TriangleMesh> mesh = m_bod->meshManager()->createMeshCalculator()->calculate();
			a += "Mesh index : " + mesh->nodeCount();
			/*a += "Volume : " + m_bod->physicalProperties()->volume;
			a += "Material : " + m_bod->physicalProperties.material;
			a += "Mass : " + m_bod->physicalProperties()->mass;*/
		}

		a += "\n";
	}

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

/*
Ptr<CommandDefinition> Exporter::expCommand() {
	//Ptr<>
}
 */

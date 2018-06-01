#include "Exporter.h"

using namespace Synthesis;

Exporter::Exporter(Ptr<Application> app) : _app(app) {
	_ui = _app->userInterface();
    
    Synthesis::EUI* _eui = new Synthesis::EUI(_ui, _app);
}

Exporter::Exporter() {

}

Exporter::~Exporter() {

}


void Exporter::Test() {
	if (!_ui)
		//return false;

	_ui->messageBox("Started Exporting");

	Ptr<FusionDocument> doc = _app->activeDocument();

	string a = "";

	for (Ptr<Joint> j : doc->design()->rootComponent()->allJoints()) {
		a += j->name() + " ";
		doc->design()->activeComponent() = j->parentComponent();
	}

	_ui->messageBox(a);
}

/*
Ptr<CommandDefinition> Exporter::expCommand() {
	//Ptr<>
}
 */

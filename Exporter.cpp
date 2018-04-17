#include "Exporter.h"

Exporter::Exporter(Ptr<Application> app) : _app(app) {
	_ui = _app->userInterface();
}

Exporter::Exporter() {

}

Exporter::~Exporter() {

}


void Exporter::Test() {
	if (!_ui)
		//return false;

	_ui->messageBox("Started Exporting");
    
    EUI* _eui = new EUI(_ui, _app);

	Ptr<FusionDocument> doc = _app->activeDocument();

	string a = "";

	for (Ptr<Joint> j : doc->design()->rootComponent()->allJoints()) {
		a += j->name() + " ";
		doc->design()->activeComponent() = j->parentComponent();
	}

	//_ui->messageBox(a);
}

/*
Ptr<CommandDefinition> Exporter::expCommand() {
	//Ptr<>
}
 */

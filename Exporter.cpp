#include "Exporter.h"

Exporter::Exporter(Ptr<Application> _app) : app(_app) {
	ui = app->userInterface();
}

Exporter::Exporter() {

}

Exporter::~Exporter() {

}


void Exporter::test() {

	
	if (!ui)
		//return false;

	ui->messageBox("Started Exporting");

	Ptr<FusionDocument> doc = app->activeDocument();

	string a = "";

	for (Ptr<Joint> j : doc->design()->rootComponent()->allJoints()) {
		a += j->name() + " ";
		doc->design()->activeComponent() = j->parentComponent();
	}

	ui->messageBox(a);
}
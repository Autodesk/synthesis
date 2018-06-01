#pragma once

#include "Exporter.h"

using namespace adsk::core;
using namespace adsk::fusion;
using namespace adsk::cam;
using namespace std;

class EUI_Handlers {
public:
	EUI_Handlers(Ptr<Application>);
	~EUI_Handlers();

	MyCommandCreatedEventHandler _commandCreated;
private:
};

class MyCommandCreatedEventHandler : public adsk::core::CommandCreatedEventHandler {
public:
	void notify(const Ptr<CommandCreatedEventArgs>& eventArgs) override;
	Ptr<Application> _app;
private:

};
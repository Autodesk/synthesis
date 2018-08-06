#include "EUI.h"

using namespace Synthesis;

// Receive Form Data Handler
template<>
bool EUI::addEventToPalette<ReceiveFormDataHandler>(Ptr<Palette> palette)
{
	if (formDataHandler == nullptr)
		formDataHandler = new ReceiveFormDataHandler(app, this);

	Ptr<HTMLEvent> htmlEvent = palette->incomingFromHTML();
	if (htmlEvent)
		htmlEvent->add(formDataHandler);

	return true;
}

template<>
bool EUI::clearEventFromPalette<ReceiveFormDataHandler>(Ptr<Palette> palette)
{
	if (formDataHandler != nullptr)
		return false;

	Ptr<HTMLEvent> htmlEvent = palette->incomingFromHTML();
	if (htmlEvent)
		htmlEvent->remove(formDataHandler);

	return true;
}

// Close Exporter Form Handler
template<>
bool EUI::addEventToPalette<CloseExporterFormEventHandler>(Ptr<Palette> palette)
{
	if (closeExporterHandler == nullptr)
		closeExporterHandler = new CloseExporterFormEventHandler(app);

	Ptr<UserInterfaceGeneralEvent> closeEvent = palette->closed();
	if (closeEvent)
		closeEvent->add(closeExporterHandler);

	return true;
}

template<>
bool EUI::clearEventFromPalette<CloseExporterFormEventHandler>(Ptr<Palette> palette)
{
	if (formDataHandler != nullptr)
		return false;

	Ptr<UserInterfaceGeneralEvent> closeEvent = palette->closed();
	if (closeEvent)
		closeEvent->remove(closeExporterHandler);

	return true;
}

// Close Sensor Form Handler
template<>
bool EUI::addEventToPalette<CloseSensorFormEventHandler>(Ptr<Palette> palette)
{
	if (closeSensorsHandler == nullptr)
		closeSensorsHandler = new CloseSensorFormEventHandler(app);

	Ptr<UserInterfaceGeneralEvent> closeEvent = palette->closed();
	if (closeEvent)
		closeEvent->add(closeExporterHandler);

	return true;
}

template<>
bool EUI::clearEventFromPalette<CloseSensorFormEventHandler>(Ptr<Palette> palette)
{
	if (formDataHandler != nullptr)
		return false;

	Ptr<UserInterfaceGeneralEvent> closeEvent = palette->closed();
	if (closeEvent)
		closeEvent->remove(closeExporterHandler);

	return true;
}

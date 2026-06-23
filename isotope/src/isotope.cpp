#include <Core/Application/Application.h>
#include <Core/Memory.h>
#include <Core/UserInterface/CommandControl.h>
#include <Core/UserInterface/CommandCreatedEvent.h>
#include <Core/UserInterface/CommandDefinition.h>
#include <Core/UserInterface/CommandDefinitions.h>
#include <Core/UserInterface/ToolbarControl.h>
#include <Core/UserInterface/ToolbarControls.h>
#include <Core/UserInterface/ToolbarPanel.h>
#include <Core/UserInterface/ToolbarPanelList.h>
#include <Core/UserInterface/ToolbarPanels.h>
#include <Core/UserInterface/ToolbarTab.h>
#include <Core/UserInterface/ToolbarTabs.h>
#include <Core/UserInterface/UserInterface.h>
#include <Core/UserInterface/Workspace.h>
#include <Core/UserInterface/Workspaces.h>

#include <string>

#include "config_command.h"
#include "context.h"

GlobalContext gctx;

extern "C" XI_EXPORT bool run(const char* context) {
    if (!gctx.configure()) {
        return false;
    }

    adsk::core::Ptr<adsk::core::Workspace> workspace = gctx.ui->workspaces()->itemById("FusionSolidEnvironment");
    if (!workspace || !workspace->isValid()) {
        gctx.ui->messageBox("Failed to find FusionSolidEnvironment workspace.");
        return false;
    }

    adsk::core::Ptr<adsk::core::ToolbarTab> tab = workspace->toolbarTabs()->itemById("ToolsTab");
    if (!tab || !tab->isValid()) {
        gctx.ui->messageBox("Failed to find ToolsTab in workspace.");
        return false;
    }

    tab->activate();
    tab->toolbarPanels()->add("isotope_tool_tab", "Isotope");

    auto button = gctx.ui->commandDefinitions()->addButtonDefinition(
        "isotope_command", "Isotope", "This command does something interesting.", "./resources/isotope_exporter/");

    if (!button || !button->isValid()) {
        gctx.ui->messageBox("Failed to create command definition for Isotope.");
        return false;
    }

    button->commandCreated()->add(new ConfigureCommandCreatedHandler(gctx));

    auto panel = gctx.ui->allToolbarPanels()->itemById("isotope_tool_tab");

    if (!panel || !panel->isValid()) {
        gctx.ui->messageBox("Failed to find toolbar panel for Isotope.");
        return false;
    }

    auto button_control = panel->controls()->addCommand(button);

    if (!button_control || !button_control->isValid()) {
        gctx.ui->messageBox("Failed to add command control for Isotope.");
        return false;
    }

    button_control->isPromoted(true);
    button_control->isPromotedByDefault(true);

    return true;
}

extern "C" XI_EXPORT bool stop() {
    if (!gctx.isValid()) {
        return true;
    }

    auto panel = gctx.ui->allToolbarPanels()->itemById("isotope_tool_tab");
    if (panel && panel->isValid()) {
        panel->deleteMe();
    }

    auto button_def = gctx.ui->commandDefinitions()->itemById("isotope_command");
    if (button_def && button_def->isValid()) {
        button_def->deleteMe();
    }

    return true;
}

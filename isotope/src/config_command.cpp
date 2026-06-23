#include "config_command.h"

#include <Core/UserInterface/Command.h>
#include <Core/UserInterface/CommandCreatedEventArgs.h>
#include <Core/UserInterface/CommandEvent.h>

#include "parser.h"

void ConfigureCommandCreatedHandler::notify(const adsk::core::Ptr<adsk::core::CommandCreatedEventArgs>& args) {
    if (!this->gctx.isValid()) {
        return;
    }
    adsk::core::Ptr<adsk::core::Command> command = args->command();
    if (!command || !command->isValid()) {
        this->gctx.ui->messageBox("Invalid command in ConfigureCommandCreatedHandler.");
        return;
    }

    command->execute()->add(new ConfigureCommandExecutedHandler(this->gctx));
}

void ConfigureCommandExecutedHandler::notify(const adsk::core::Ptr<adsk::core::CommandEventArgs>& eventArgs) {
    if (!this->gctx.isValid()) {
        return;
    }
    export_design(this->gctx);
}

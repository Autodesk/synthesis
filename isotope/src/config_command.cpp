#include "config_command.h"

#include <Core/UserInterface/CommandCreatedEventArgs.h>
#include <Core/UserInterface/Command.h>
#include <Core/UserInterface/CommandEvent.h>

void ConfigureCommandCreatedHandler::notify(const adsk::core::Ptr<adsk::core::CommandCreatedEventArgs>& args) {
    adsk::core::Ptr<adsk::core::Command> command = args->command();
    if (!command || !command->isValid()) {
        gctx->ui->messageBox("Invalid command in ConfigureCommandCreatedHandler.");
        return;
    }

    command->execute()->add(new ConfigureCommandExecutedHandler(gctx));
}

void ConfigureCommandExecutedHandler::notify(const adsk::core::Ptr<adsk::core::CommandEventArgs>& eventArgs) {
    gctx->ui->messageBox("Configure command executed successfully.");
}

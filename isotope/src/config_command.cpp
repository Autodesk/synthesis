#include "config_command.h"

#include "parser.h"

#include <Core/UserInterface/Command.h>
#include <Core/UserInterface/CommandCreatedEventArgs.h>
#include <Core/UserInterface/CommandEvent.h>

void ConfigureCommandCreatedHandler::notify(const adsk::core::Ptr<adsk::core::CommandCreatedEventArgs>& args) {
    assert(this->gctx.isValid());
    adsk::core::Ptr<adsk::core::Command> command = args->command();
    if (!command || !command->isValid()) {
        this->gctx.ui->messageBox("Invalid command in ConfigureCommandCreatedHandler.");
        return;
    }

    command->execute()->add(new ConfigureCommandExecutedHandler(this->gctx));
}

void ConfigureCommandExecutedHandler::notify(const adsk::core::Ptr<adsk::core::CommandEventArgs>& eventArgs) {
    assert(this->gctx.isValid());
    export_design(this->gctx);
}

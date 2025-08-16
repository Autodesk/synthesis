#pragma once
#ifndef ISOTOPE_CONFIG_COMMAND_H_
#define ISOTOPE_CONFIG_COMMAND_H_

#include <Core/UserInterface/CommandCreatedEventHandler.h>
#include <Core/UserInterface/CommandEventHandler.h>

#include "context.h"

class ConfigureCommandCreatedHandler : public adsk::core::CommandCreatedEventHandler {
private:
    const GlobalContext& gctx;

public:
    ConfigureCommandCreatedHandler(const GlobalContext& context) : gctx(context) {}

    void notify(const adsk::core::Ptr<adsk::core::CommandCreatedEventArgs>& args) override;
};

class ConfigureCommandExecutedHandler : public adsk::core::CommandEventHandler {
private:
    const GlobalContext& gctx;

public:
    ConfigureCommandExecutedHandler(const GlobalContext& context) : gctx(context) {}

    void notify(const adsk::core::Ptr<adsk::core::CommandEventArgs>& eventArgs) override;
};

#endif // ISOTOPE_CONFIG_COMMAND_H_

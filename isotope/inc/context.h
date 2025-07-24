#pragma once
#ifndef ISOTOPE_CONTEXT_H_
#define ISOTOPE_CONTEXT_H_

#include <Core/Application/Application.h>
#include <Core/Memory.h>
#include <Core/UserInterface/UserInterface.h>

struct GlobalContext {
    adsk::core::Ptr<adsk::core::Application> app;
    adsk::core::Ptr<adsk::core::UserInterface> ui;

    bool configure();
    bool isValid() const;
};

#endif // ISOTOPE_CONTEXT_H_

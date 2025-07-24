#pragma once
#ifndef ISOTOPE_COMPONENTS_H_
#define ISOTOPE_COMPONENTS_H_

#include <Core/CoreAll.h>
#include <Fusion/FusionAll.h>

#include "assembly.pb.h"

mirabuf::Parts map_all_parts(
    const adsk::core::Ptr<adsk::fusion::Components>& components,
    const mirabuf::material::Materials& materials);

#endif // ISOTOPE_COMPONENTS_H_

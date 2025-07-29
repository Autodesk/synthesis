#pragma once
#ifndef ISOTOPE_COMPONENTS_H_
#define ISOTOPE_COMPONENTS_H_

#include "assembly.pb.h"
#include "types.pb.h"

#include <Core/CoreAll.h>
#include <Fusion/FusionAll.h>

mirabuf::Parts map_all_parts(const adsk::core::Ptr<adsk::fusion::Components>& components,
    const mirabuf::material::Materials& materials); // TODO: Replace parameter with appearance map

mirabuf::Node parse_component_root(
    const adsk::core::Ptr<adsk::fusion::Component>& component, mirabuf::Parts* parts);

#endif // ISOTOPE_COMPONENTS_H_

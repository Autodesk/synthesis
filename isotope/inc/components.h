#pragma once
#ifndef ISOTOPE_COMPONENTS_H_
#define ISOTOPE_COMPONENTS_H_

#include <Core/CoreAll.h>
#include <Fusion/FusionAll.h>

#include <utility>

#include "assembly.pb.h"
#include "types.pb.h"

std::pair<mirabuf::Parts, mirabuf::Node> map_parts(const adsk::core::Ptr<adsk::fusion::Components>& components,
    const adsk::core::Ptr<adsk::fusion::Component>& root, const mirabuf::material::Materials& materials);

void map_rigid_groups(const adsk::core::Ptr<adsk::fusion::Component>& root, mirabuf::joint::Joints* joints);

#endif // ISOTOPE_COMPONENTS_H_

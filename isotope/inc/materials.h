#pragma once
#ifndef ISOTOPE_MATERIALS_H_
#define ISOTOPE_MATERIALS_H_

#include <Core/CoreAll.h>

#include "material.pb.h"

mirabuf::material::Materials map_all_materials(const adsk::core::Ptr<adsk::core::Appearances>& design_appearances,
    const adsk::core::Ptr<adsk::core::Materials>& design_materials);

#endif // ISOTOPE_MATERIALS_H_

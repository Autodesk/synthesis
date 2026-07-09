#pragma once
#ifndef ISOTOPE_JOINT_HIERARCHY_H_
#define ISOTOPE_JOINT_HIERARCHY_H_

#include <Core/Memory.h>
#include <Fusion/Fusion/Design.h>

#include "joint.pb.h"

void build_joint_part_hierarchy(mirabuf::joint::Joints* joints, const adsk::core::Ptr<adsk::fusion::Design>& design);

#endif // ISOTOPE_JOINT_HIERARCHY_H_

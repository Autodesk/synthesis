#pragma once
#ifndef ISOTOPE_JOINTS_H_
#define ISOTOPE_JOINTS_H_

#include <Core/CoreAll.h>
#include <Fusion/FusionAll.h>

#include "joint.pb.h"
#include "signal.pb.h"
#include "types.pb.h"

std::pair<mirabuf::joint::Joints, mirabuf::signal::Signals> populate_joints(
    const adsk::core::Ptr<adsk::fusion::Design>& design);

mirabuf::GraphContainer create_joint_graph(const mirabuf::joint::Joints& joints);

#endif // ISOTOPE_JOINTS_H_

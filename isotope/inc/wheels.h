#pragma once

#include <Core/Memory.h>
#include <Core/UserInterface/UserInterface.h>
#include <Fusion/Fusion/Design.h>

#include "joint.pb.h"

// Revolute joints whose axes are parallel and whose origins lie along that axis
// form candidate axle pairs. Pairs sharing an axle direction are grouped, their
// midpoints are projected into the side-view plane perpendicular to the axle, and
// the largest collinear set of midpoints is selected as the drivetrain.
void detect_and_tag_wheels(mirabuf::joint::Joints* joints, const adsk::core::Ptr<adsk::fusion::Design>& design,
    const adsk::core::Ptr<adsk::core::UserInterface>& ui = nullptr);

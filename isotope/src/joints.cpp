#include "joints.h"

#include <Fusion/FusionAll.h>
#include <Core/CoreAll.h>

#include <Fusion/Components/RevoluteJointMotion.h>
#include <Fusion/Components/SliderJointMotion.h>
#include <Core/Geometry/Point3D.h>
#include <Fusion/BRep/BRepEdge.h>
#include <Fusion/BRep/BRepFace.h>
#include <Fusion/Components/Joint.h>
#include <Fusion/Components/JointGeometry.h>
#include <Fusion/Components/JointOrigin.h>
#include <Fusion/Components/Occurrence.h>

#include "assembly.pb.h"
#include "joint.pb.h"
#include "signal.pb.h"
#include "types.pb.h"

#include "util.h"

#include <variant>
#include <string>

namespace {

mirabuf::joint::RigidGroup map_rigid_group(const adsk::fusion::Joint* /* adsk::fusion::joint | adsk::fusion::AsBuiltJoint */ joint
) {
    assert(joint);
    assert(joint->jointMotion()->jointType() == adsk::fusion::JointTypes::RigidJointType);

    if (!joint->occurrenceOne()->isLightBulbOn() ||
        !joint->occurrenceTwo()->isLightBulbOn()) {
        return {};
    }

    mirabuf::joint::RigidGroup group;
    std::string group_name = "group_" + joint->occurrenceOne()->name() + "_" +
                             joint->occurrenceTwo()->name();
    group.set_name(group_name);
    group.add_occurrences(joint->occurrenceOne()->name());
    group.add_occurrences(joint->occurrenceTwo()->name());

    return group;
}

void fill_revolute_joint_motion(const adsk::core::Ptr<adsk::fusion::RevoluteJointMotion>& motion, mirabuf::joint::Joint* proto_joint) {
    assert(motion);
    assert(proto_joint);

    proto_joint->set_joint_motion_type(mirabuf::joint::JointMotion::REVOLUTE);
    auto dof = proto_joint->mutable_rotational()->mutable_rotational_freedom();
    dof->set_name("Rotational Joint");
    dof->set_value(motion->rotationValue());
    if (motion->rotationLimits()) {
        dof->mutable_limits()->set_lower(motion->rotationLimits()->minimumValue());
        dof->mutable_limits()->set_upper(motion->rotationLimits()->maximumValue());
    }

    auto rotation_axis_vector = motion->rotationAxisVector();
    if (rotation_axis_vector) {
        dof->mutable_axis()->set_x(rotation_axis_vector->x());
        dof->mutable_axis()->set_y(rotation_axis_vector->y());
        dof->mutable_axis()->set_z(rotation_axis_vector->z());
    } else {
        auto rotation_axis = motion->rotationAxis();
        assert(rotation_axis);
        dof->mutable_axis()->set_x((int) rotation_axis == 0);
        dof->mutable_axis()->set_y((int) rotation_axis == 2);
        dof->mutable_axis()->set_z((int) rotation_axis == 1);
    }
}

void fill_slider_joint_motion(const adsk::core::Ptr<adsk::fusion::SliderJointMotion>& motion, mirabuf::joint::Joint* proto_joint) {
    assert(motion);
    assert(proto_joint);

    proto_joint->set_joint_motion_type(mirabuf::joint::JointMotion::SLIDER);
    auto dof = proto_joint->mutable_prismatic()->mutable_prismatic_freedom();
    dof->mutable_axis()->set_x(-motion->slideDirectionVector()->x());
    dof->mutable_axis()->set_y(-motion->slideDirectionVector()->y());
    dof->mutable_axis()->set_z(-motion->slideDirectionVector()->z());

    switch (motion->slideDirection()) {
        case adsk::fusion::JointDirections::XAxisJointDirection:
            dof->set_pivotdirection(mirabuf::Axis::X);
            break;
        case adsk::fusion::JointDirections::YAxisJointDirection:
            dof->set_pivotdirection(mirabuf::Axis::Y);
            break;
        case adsk::fusion::JointDirections::ZAxisJointDirection:
            dof->set_pivotdirection(mirabuf::Axis::Z);
            break;
        case adsk::fusion::JointDirections::CustomJointDirection:
        default:
            break;
    }

    if (motion->slideLimits()) {
        dof->mutable_limits()->set_lower(motion->slideLimits()->minimumValue());
        dof->mutable_limits()->set_upper(motion->slideLimits()->maximumValue());
    }

    dof->set_value(motion->slideValue());
}

void fill_motion_from_joint(const adsk::core::Ptr<adsk::fusion::JointMotion>& motion, mirabuf::joint::Joint* proto_joint) {
    assert(motion);
    assert(proto_joint);

    switch (motion->jointType()) {
        case adsk::fusion::JointTypes::RevoluteJointType:
            fill_revolute_joint_motion(adsk::core::Ptr<adsk::fusion::RevoluteJointMotion>(motion), proto_joint);
            break;
        case adsk::fusion::JointTypes::SliderJointType:
            fill_slider_joint_motion(adsk::core::Ptr<adsk::fusion::SliderJointMotion>(motion), proto_joint);
            break;
        case adsk::fusion::JointTypes::RigidJointType:
            proto_joint->set_joint_motion_type(mirabuf::joint::JointMotion::RIGID);
            break;
        case adsk::fusion::JointTypes::CylindricalJointType:
        case adsk::fusion::JointTypes::BallJointType:
        case adsk::fusion::JointTypes::PinSlotJointType:
        case adsk::fusion::JointTypes::PlanarJointType:
        case adsk::fusion::JointTypes::InferredJointType:
        default:
            break;
    }
}

adsk::core::Ptr<adsk::core::Point3D> origin_from_joint_geometry(const adsk::fusion::JointGeometry* geometry, const adsk::core::Ptr<adsk::fusion::Occurrence> occurrence) {
    if (!geometry) {
        return adsk::core::Point3D::create();
    }


    auto entity_one = geometry->entityOne();
    if (!entity_one) {
        return adsk::core::Point3D::create();
    }

    auto edge_or_face = fusion_base_to_variant<adsk::fusion::BRepEdge, adsk::fusion::BRepFace>(entity_one.get());
    adsk::core::Ptr<adsk::core::Point3D> result = std::visit(overloaded{
        [&geometry](std::monostate) -> auto {
            return geometry->origin();
        },
        [&geometry, &occurrence](const adsk::fusion::BRepEdge* edge) -> auto {
            if (!edge->assemblyContext()) {
                auto new_entity = edge->createForAssemblyContext(occurrence);
                auto min = new_entity->boundingBox()->minPoint();
                auto max = new_entity->boundingBox()->maxPoint();
                auto org = adsk::core::Point3D::create((max->x() + min->x()) / 2.0f, (max->y() + min->y()) / 2.0f, (max->z() + min->z()) / 2.0f);
                return org;
            }

            return geometry->origin();
        },
        [&geometry, &occurrence](const adsk::fusion::BRepFace* face) -> auto {
            if (!face->assemblyContext()) {
                auto new_entity = face->createForAssemblyContext(occurrence);
                return new_entity->centroid();
            }

            return geometry->origin();
        }
    }, edge_or_face);

    return result;
}

adsk::core::Ptr<adsk::core::Point3D> origin_from_joint_origin(const adsk::fusion::JointOrigin* joint_origin) {
    if (!joint_origin) {
        return adsk::core::Point3D::create();
    }

    auto origin = joint_origin->geometry()->origin();
    double offset_x = joint_origin->offsetX() ? joint_origin->offsetX()->value() : 0;
    double offset_y = joint_origin->offsetY() ? joint_origin->offsetY()->value() : 0;
    double offset_z = joint_origin->offsetZ() ? joint_origin->offsetZ()->value() : 0;
    return adsk::core::Point3D::create(origin->x() + offset_x, origin->y() + offset_y, origin->z() + offset_z);
}

adsk::core::Ptr<adsk::core::Point3D> get_joint_origin(const adsk::fusion::Joint* fusion_joint) {
    assert(fusion_joint);
    auto raw_geo_test = fusion_joint->geometryOrOriginOne();
    auto geometry_or_origin = fusion_base_to_variant<adsk::fusion::JointGeometry, adsk::fusion::JointOrigin>(raw_geo_test.get());
    if (std::holds_alternative<std::monostate>(geometry_or_origin)) {
        return adsk::core::Point3D::create();
    }

    adsk::core::Ptr<adsk::core::Point3D> result = std::visit(overloaded{
        [](std::monostate) -> auto {
            return adsk::core::Point3D::create();
        },
        [&fusion_joint](const adsk::fusion::JointGeometry* geometry) -> auto {
            return origin_from_joint_geometry(geometry, fusion_joint->occurrenceOne());
        },
        [](const adsk::fusion::JointOrigin* origin) -> auto {
            return origin_from_joint_origin(origin);
        }
    }, geometry_or_origin);

    return result;
}

} // namespace

std::pair<mirabuf::joint::Joints, mirabuf::signal::Signals> populate_joints(
    const adsk::core::Ptr<adsk::fusion::Design>& design) {
    assert(design);
    mirabuf::joint::Joints joints;
    joints.mutable_info()->set_name("");
    joints.mutable_info()->set_guid("joints-guid");
    joints.mutable_info()->set_version(1);

    mirabuf::signal::Signals signals;

    auto& joint_definition_ground = (*joints.mutable_joint_definitions())["grounded"];
    joint_definition_ground.mutable_info()->set_name("grounded");

    auto& joint_instance_ground = (*joints.mutable_joint_instances())["grounded"];
    joint_instance_ground.mutable_info()->set_name("grounded");

    joint_instance_ground.set_joint_reference(joint_definition_ground.info().guid());

    auto process_joint = [&joints, &signals](const adsk::fusion::Joint* /* adsk::fusion::joint | adsk::fusion::AsBuiltJoint */ joint) {
        assert(joint);
        if (joint->isSuppressed()) {
            return;
        }

        auto motion = joint->jointMotion();
        if (motion->jointType() == adsk::fusion::JointTypes::RigidJointType) {
            auto rigidGroup = map_rigid_group(joint);
            if (!rigidGroup.occurrences().empty()) {
                joints.mutable_rigid_groups()->Add()->CopyFrom(rigidGroup);
            }
        }

        auto& signal = (*signals.mutable_signal_map())[joint->name()];
        signal.mutable_info()->set_name(joint->name());
        signal.mutable_info()->set_guid(joint->name());
        signal.mutable_info()->set_version(1);
        signal.set_io(mirabuf::signal::IOType::OUTPUT);
        signal.set_device_type(mirabuf::signal::DeviceType::PWM);

        auto& joint_instance = (*joints.mutable_joint_instances())[joint->name()];
        joint_instance.set_signal_reference(signal.info().guid());
        joint_instance.set_parent_part(joint->occurrenceOne()->name());
        joint_instance.set_child_part(joint->occurrenceTwo()->name());

        // TODO: Wheel logic should go here

        auto& joint_definition = (*joints.mutable_joint_definitions())[joint->name()];
        joint_definition.set_motor_reference(signal.info().guid());
        joint_definition.mutable_info()->set_name(joint->name());
        joint_definition.mutable_info()->set_guid(joint->name());
        joint_definition.mutable_info()->set_version(1);

        auto joint_origin = get_joint_origin(joint);

        if (joint_origin) {
            joint_definition.mutable_origin()->set_x(joint_origin->x());
            joint_definition.mutable_origin()->set_y(joint_origin->y());
            joint_definition.mutable_origin()->set_z(joint_origin->z());
        } else {
            joint_definition.mutable_origin()->set_x(0.0f);
            joint_definition.mutable_origin()->set_y(0.0f);
            joint_definition.mutable_origin()->set_z(0.0f);
        }

        joint_definition.set_break_magnitude(0.0f);

        auto& motor = (*joints.mutable_motor_definitions())[joint->name()];
        auto simple_motor = motor.mutable_simple_motor();

        // These are values I just chose on a whim, they need to be checked and changed to make sure
        // everything works correctly.
        simple_motor->set_stall_torque(0.5f);
        simple_motor->set_max_velocity(1.0f);
        simple_motor->set_braking_constant(0.8f);

        fill_motion_from_joint(motion, &joint_definition);
    };

    for (const auto& joint : design->rootComponent()->allJoints()) {
        process_joint(joint.get());
    }

    for (const auto& asBuiltJoint : design->rootComponent()->allAsBuiltJoints()) {
        // TODO: Replace adsk::fusion::Joint* with auto to make this function call valid
        // the compiler will make two instances of the lambda, one for each type
        // process_joint(asBuiltJoint.get());
    }

    return { joints, signals };
}

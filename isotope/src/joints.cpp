#include "joints.h"

#include <Core/CoreAll.h>
#include <Core/Geometry/Point3D.h>
#include <Core/Memory.h>
#include <Fusion/BRep/BRepEdge.h>
#include <Fusion/BRep/BRepFace.h>
#include <Fusion/Components/AsBuiltJoint.h>
#include <Fusion/Components/Component.h>
#include <Fusion/Components/Joint.h>
#include <Fusion/Components/JointGeometry.h>
#include <Fusion/Components/JointOrigin.h>
#include <Fusion/Components/Occurrence.h>
#include <Fusion/Components/RevoluteJointMotion.h>
#include <Fusion/Components/SliderJointMotion.h>
#include <Fusion/Fusion/Design.h>
#include <Fusion/FusionAll.h>
#include <Fusion/FusionTypeDefs.h>

#include <string>

#include "joint.pb.h"
#include "signal.pb.h"
#include "types.pb.h"

#include "util.h"

namespace {

template <typename JointT>
mirabuf::joint::RigidGroup map_rigid_group(const JointT* joint) {
    assert(joint);
    assert(joint->jointMotion()->jointType() == adsk::fusion::JointTypes::RigidJointType);

    if (!joint->occurrenceOne() || !joint->occurrenceTwo()) {
        return {};
    }

    if (!joint->occurrenceOne()->isLightBulbOn() || !joint->occurrenceTwo()->isLightBulbOn()) {
        return {};
    }

    mirabuf::joint::RigidGroup group;
    std::string group_name = "group_" + joint->occurrenceOne()->name() + "_" + joint->occurrenceTwo()->name();
    group.set_name(group_name);
    group.add_occurrences(guid_occurrence(joint->occurrenceOne()));
    group.add_occurrences(guid_occurrence(joint->occurrenceTwo()));

    return group;
}

void fill_revolute_joint_motion(
    const adsk::core::Ptr<adsk::fusion::RevoluteJointMotion>& motion, mirabuf::joint::Joint* proto_joint) {
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
        switch (rotation_axis) {
            case adsk::fusion::JointDirections::XAxisJointDirection:
                dof->mutable_axis()->set_x(true);
                break;
            case adsk::fusion::JointDirections::YAxisJointDirection:
                dof->mutable_axis()->set_y(true);
                break;
            case adsk::fusion::JointDirections::ZAxisJointDirection:
                dof->mutable_axis()->set_z(true);
                break;
            default:
                break;
        }
    }
}

void fill_slider_joint_motion(
    const adsk::core::Ptr<adsk::fusion::SliderJointMotion>& motion, mirabuf::joint::Joint* proto_joint) {
    assert(motion);
    assert(proto_joint);

    proto_joint->set_joint_motion_type(mirabuf::joint::JointMotion::SLIDER);
    auto dof = proto_joint->mutable_prismatic()->mutable_prismatic_freedom();
    if (auto slide_direction_vector = motion->slideDirectionVector()) {
        dof->mutable_axis()->set_x(-slide_direction_vector->x());
        dof->mutable_axis()->set_y(-slide_direction_vector->y());
        dof->mutable_axis()->set_z(-slide_direction_vector->z());
    }

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

void fill_motion_from_joint(
    const adsk::core::Ptr<adsk::fusion::JointMotion>& motion, mirabuf::joint::Joint* proto_joint) {
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

adsk::core::Ptr<adsk::core::Point3D> bounding_box_center(const adsk::core::Ptr<adsk::fusion::BRepEdge>& entity) {
    if (!entity) {
        return adsk::core::Point3D::create();
    }

    auto bounding_box = entity->boundingBox();
    if (!bounding_box) {
        return adsk::core::Point3D::create();
    }

    auto min = bounding_box->minPoint();
    auto max = bounding_box->maxPoint();
    return adsk::core::Point3D::create(
        (max->x() + min->x()) / 2.0, (max->y() + min->y()) / 2.0, (max->z() + min->z()) / 2.0);
}

adsk::core::Ptr<adsk::core::Point3D> origin_from_joint_geometry(
    const adsk::fusion::JointGeometry* geometry, const adsk::core::Ptr<adsk::fusion::Occurrence>& occurrence) {
    if (!geometry) {
        return adsk::core::Point3D::create();
    }

    auto entity_one = geometry->entityOne();
    if (!entity_one) {
        return adsk::core::Point3D::create();
    }

    if (auto edge = fusion_try_cast<adsk::fusion::BRepEdge>(entity_one.get())) {
        if (edge->assemblyContext()) {
            return geometry->origin();
        }

        return bounding_box_center(edge->createForAssemblyContext(occurrence));
    } else if (auto face = fusion_try_cast<adsk::fusion::BRepFace>(entity_one.get())) {
        if (face->assemblyContext()) {
            return geometry->origin();
        }

        if (auto face_for_context = face->createForAssemblyContext(occurrence)) {
            return face_for_context->centroid();
        }

        return adsk::core::Point3D::create();
    }

    return geometry->origin();
}

adsk::core::Ptr<adsk::core::Point3D> origin_from_joint_origin(const adsk::fusion::JointOrigin* joint_origin) {
    if (!joint_origin) {
        return adsk::core::Point3D::create();
    }

    auto geometry = joint_origin->geometry();
    if (!geometry) {
        return adsk::core::Point3D::create();
    }

    auto origin     = geometry->origin();
    double offset_x = joint_origin->offsetX() ? joint_origin->offsetX()->value() : 0;
    double offset_y = joint_origin->offsetY() ? joint_origin->offsetY()->value() : 0;
    double offset_z = joint_origin->offsetZ() ? joint_origin->offsetZ()->value() : 0;
    return adsk::core::Point3D::create(origin->x() + offset_x, origin->y() + offset_y, origin->z() + offset_z);
}

adsk::core::Ptr<adsk::core::Point3D> get_joint_origin(const adsk::fusion::Joint* fusion_joint) {
    assert(fusion_joint);
    auto geo_or_origin = fusion_joint->geometryOrOriginOne();
    if (auto geometry = fusion_try_cast<adsk::fusion::JointGeometry>(geo_or_origin.get())) {
        return origin_from_joint_geometry(geometry, fusion_joint->occurrenceOne());
    } else if (auto origin = fusion_try_cast<adsk::fusion::JointOrigin>(geo_or_origin.get())) {
        return origin_from_joint_origin(origin);
    }

    return adsk::core::Point3D::create();
}

// AsBuiltJoint always provides a JointGeometry directly, no JointOrigin variant.
adsk::core::Ptr<adsk::core::Point3D> get_joint_origin(const adsk::fusion::AsBuiltJoint* joint) {
    assert(joint);
    auto geometry = joint->geometry();
    if (!geometry) {
        return adsk::core::Point3D::create();
    }

    return origin_from_joint_geometry(geometry.get(), joint->occurrenceOne());
}

} // namespace

std::pair<mirabuf::joint::Joints, mirabuf::signal::Signals> populate_joints(
    const adsk::core::Ptr<adsk::fusion::Design>& design) {
    assert(design);
    mirabuf::joint::Joints joints;
    joints.mutable_info()->set_name("");
    joints.mutable_info()->set_guid(uuid4());
    joints.mutable_info()->set_version(1);

    mirabuf::signal::Signals signals;

    auto& joint_definition_ground = (*joints.mutable_joint_definitions())["grounded"];
    joint_definition_ground.mutable_info()->set_name("grounded");
    joint_definition_ground.mutable_info()->set_guid(uuid4());
    joint_definition_ground.mutable_info()->set_version(1);

    auto& joint_instance_ground = (*joints.mutable_joint_instances())["grounded"];
    joint_instance_ground.mutable_info()->set_name("grounded");
    joint_instance_ground.mutable_info()->set_guid(uuid4());
    joint_instance_ground.mutable_info()->set_version(1);

    joint_instance_ground.set_joint_reference(joint_definition_ground.info().guid());

    auto process_joint = [&joints, &signals](const auto* joint) {
        assert(joint);
        if (joint->isSuppressed()) {
            return;
        }

        auto motion = joint->jointMotion();
        if (!motion) {
            return;
        }

        if (motion->jointType() == adsk::fusion::JointTypes::RigidJointType) {
            auto rigidGroup = map_rigid_group(joint);
            if (!rigidGroup.occurrences().empty()) {
                joints.mutable_rigid_groups()->Add()->CopyFrom(rigidGroup);
            }

            return;
        }

        const std::string signal_guid = uuid4();
        auto& signal                  = (*signals.mutable_signal_map())[signal_guid];
        signal.mutable_info()->CopyFrom(create_info_from_fus_obj(joint, signal_guid));
        signal.set_io(mirabuf::signal::OUTPUT);
        signal.set_device_type(mirabuf::signal::PWM);

        auto& joint_definition = (*joints.mutable_joint_definitions())[joint->entityToken()];
        joint_definition.mutable_info()->CopyFrom(create_info_from_fus_obj(joint));
        joint_definition.set_motor_reference(joint->entityToken());

        auto& joint_instance = (*joints.mutable_joint_instances())[joint->entityToken()];
        joint_instance.mutable_info()->CopyFrom(create_info_from_fus_obj(joint));
        joint_instance.set_signal_reference(signal.info().guid());
        joint_instance.set_joint_reference(joint_definition.info().guid());
        joint_instance.set_parent_part(guid_occurrence(joint->occurrenceOne()));
        joint_instance.set_child_part(guid_occurrence(joint->occurrenceTwo()));

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

        auto& motor = (*joints.mutable_motor_definitions())[joint->entityToken()];
        motor.mutable_info()->CopyFrom(create_info_from_fus_obj(joint));
        auto simple_motor = motor.mutable_simple_motor();

        simple_motor->set_stall_torque(0.5f);
        simple_motor->set_max_velocity(1.0f);
        simple_motor->set_braking_constant(0.8f);

        fill_motion_from_joint(motion, &joint_definition);
    };

    for (const auto& joint : design->rootComponent()->allJoints()) {
        process_joint(joint.get());
    }

    for (const auto& asBuiltJoint : design->rootComponent()->allAsBuiltJoints()) {
        process_joint(asBuiltJoint.get());
    }

    return {joints, signals};
}

mirabuf::GraphContainer create_joint_graph(const mirabuf::joint::Joints& joints) {
    // "ground" is a synthetic root node.  The "grounded" joint definition is a
    // metadata entry for the fixed/world joint and is intentionally excluded from
    // the hierarchy (matches Python exporter behaviour).
    mirabuf::GraphContainer joint_tree;
    auto* ground_entry = joint_tree.mutable_nodes()->Add();
    ground_entry->set_value("ground");

    for (const auto& [key, joint] : joints.joint_definitions()) {
        if (key == "grounded" || joint.info().guid().empty()) {
            continue;
        }

        mirabuf::Node def_node;
        def_node.set_value(joint.info().guid());
        ground_entry->mutable_children()->Add()->CopyFrom(def_node);
        joint_tree.mutable_nodes()->Add()->CopyFrom(def_node);
    }

    return joint_tree;
}

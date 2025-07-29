#include "joints.h"

#include "assembly.pb.h"
#include "joint.pb.h"
#include "signal.pb.h"
#include <locale>

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

} // namespace

std::pair<mirabuf::joint::Joints, mirabuf::signal::Signals> populate_joints(
    const adsk::core::Ptr<adsk::fusion::Design>& design) {
    assert(design);
    mirabuf::joint::Joints joints;
    joints.mutable_info()->set_name("");
    joints.mutable_info()->set_guid("joints-guid");
    joints.mutable_info()->set_version(1);

    mirabuf::signal::Signals signals;
    // auto& part = (*parts.mutable_part_definitions())[component->id()];

    auto& joint_definition_ground = (*joints.mutable_joint_definitions())["grounded"];
    joint_definition_ground.mutable_info()->set_name("grounded");
    // todo: other info stuff

    auto& joint_instance_ground = (*joints.mutable_joint_instances())["grounded"];
    joint_instance_ground.mutable_info()->set_name("grounded");
    // todo: other info stuff

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

        auto& motor = (*joints.mutable_motor_definitions())[joint->name()];
        auto simple_motor = motor.mutable_simple_motor();
        // These are values I just chose on a whim, they need to be checked and changed to make sure
        // everything works correctly.
        simple_motor->set_stall_torque(0.5f);
        simple_motor->set_max_velocity(1.0f);
        simple_motor->set_braking_constant(0.8f);

        // TODO: Motion info depending on joint type
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

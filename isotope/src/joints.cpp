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

#include <memory>
#include <optional>
#include <stack>
#include <string>
#include <unordered_map>
#include <unordered_set>
#include <variant>
#include <vector>

#include "assembly.pb.h"
#include "joint.pb.h"
#include "signal.pb.h"
#include "types.pb.h"

#include "util.h"

namespace {

using AnyJointPtr = std::variant<adsk::core::Ptr<adsk::fusion::Joint>, adsk::core::Ptr<adsk::fusion::AsBuiltJoint>>;

std::string joint_entity_token(const AnyJointPtr& joint) {
    return std::visit([](const auto& j) -> std::string { return j->entityToken(); }, joint);
}

template <typename JointT>
mirabuf::joint::RigidGroup map_rigid_group(const JointT* joint) {
    assert(joint);
    assert(joint->jointMotion()->jointType() == adsk::fusion::JointTypes::RigidJointType);

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
        assert(rotation_axis);
        dof->mutable_axis()->set_x((int) rotation_axis == 0);
        dof->mutable_axis()->set_y((int) rotation_axis == 2);
        dof->mutable_axis()->set_z((int) rotation_axis == 1);
    }
}

void fill_slider_joint_motion(
    const adsk::core::Ptr<adsk::fusion::SliderJointMotion>& motion, mirabuf::joint::Joint* proto_joint) {
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
    auto min = entity->boundingBox()->minPoint();
    auto max = entity->boundingBox()->maxPoint();
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

    // clang-format off
    auto visitor = overloaded{
        [&](std::monostate)                     { return geometry->origin(); },
        [&](const adsk::fusion::BRepEdge* edge) {
            if (edge->assemblyContext()) return geometry->origin();
            return bounding_box_center(edge->createForAssemblyContext(occurrence));
        },
        [&](const adsk::fusion::BRepFace* face) {
            if (face->assemblyContext()) return geometry->origin();
            return face->createForAssemblyContext(occurrence)->centroid();
        }};
    // clang-format on
    return std::visit(
        visitor, fusion_base_to_variant<adsk::fusion::BRepEdge, adsk::fusion::BRepFace>(entity_one.get()));
}

adsk::core::Ptr<adsk::core::Point3D> origin_from_joint_origin(const adsk::fusion::JointOrigin* joint_origin) {
    if (!joint_origin) {
        return adsk::core::Point3D::create();
    }

    auto origin     = joint_origin->geometry()->origin();
    double offset_x = joint_origin->offsetX() ? joint_origin->offsetX()->value() : 0;
    double offset_y = joint_origin->offsetY() ? joint_origin->offsetY()->value() : 0;
    double offset_z = joint_origin->offsetZ() ? joint_origin->offsetZ()->value() : 0;
    return adsk::core::Point3D::create(origin->x() + offset_x, origin->y() + offset_y, origin->z() + offset_z);
}

adsk::core::Ptr<adsk::core::Point3D> get_joint_origin(const adsk::fusion::Joint* fusion_joint) {
    assert(fusion_joint);
    // clang-format off
    auto visitor = overloaded{
        [](std::monostate)                              { return adsk::core::Point3D::create(); },
        [&](const adsk::fusion::JointGeometry* geometry){ return origin_from_joint_geometry(geometry, fusion_joint->occurrenceOne()); },
        [](const adsk::fusion::JointOrigin* origin)     { return origin_from_joint_origin(origin); }};
    // clang-format on
    return std::visit(visitor, fusion_base_to_variant<adsk::fusion::JointGeometry, adsk::fusion::JointOrigin>(
                                   fusion_joint->geometryOrOriginOne().get()));
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

adsk::core::Ptr<adsk::fusion::Occurrence> search_for_grounded(const adsk::core::Ptr<adsk::fusion::Component>& root) {
    for (const auto& occ : root->allOccurrences()) {
        if (occ->isGrounded()) {
            return occ;
        }
    }
    return nullptr;
}

enum class OccurrenceRelationship {
    TRANSFORM, // Hierarchy parenting
    CONNECTION, // A rigid joint or other designator
    NEXT, // The next joint in a list
    NONE,
};

using enum OccurrenceRelationship;

struct GraphEdge;

struct GraphNode {
    adsk::core::Ptr<adsk::fusion::Occurrence> data = nullptr;
    std::vector<std::shared_ptr<GraphEdge>> edges{};
    std::optional<AnyJointPtr> joint = std::nullopt;
};

struct GraphEdge {
    OccurrenceRelationship relationship = NONE;
    std::shared_ptr<GraphNode> node     = nullptr;
};

adsk::core::Ptr<adsk::fusion::Occurrence> joint_connection(
    const adsk::core::Ptr<adsk::fusion::Joint>& joint, const adsk::core::Ptr<adsk::fusion::Occurrence>& occurrence) {
    bool is_rigid = joint->jointMotion()->jointType() == adsk::fusion::RigidJointType;
    if (is_rigid) {
        if (joint->occurrenceOne() == occurrence) {
            return joint->occurrenceTwo();
        }

        if (joint->occurrenceTwo() == occurrence) {
            return joint->occurrenceOne();
        }
        return nullptr;
    }

    return joint->occurrenceOne() != occurrence ? joint->occurrenceOne() : nullptr;
}

std::optional<std::shared_ptr<GraphNode>> populate_node(const adsk::core::Ptr<adsk::fusion::Occurrence>& occurrence,
    std::shared_ptr<GraphNode> prev, OccurrenceRelationship relationship, bool is_ground,
    std::unordered_set<std::string>& visited_occurrence_entity_tokens,
    const std::unordered_map<std::string, AnyJointPtr>& dynamic_joints) {
    if (occurrence->isGrounded() && !is_ground) {
        return std::nullopt;
    }

    if (relationship == NEXT && prev) {
        prev->edges.push_back(
            std::make_shared<GraphEdge>(GraphEdge{relationship, std::make_shared<GraphNode>(GraphNode{occurrence})}));
        return std::nullopt;
    }

    if (prev && dynamic_joints.contains(occurrence->entityToken())) {
        return std::nullopt;
    }

    if (visited_occurrence_entity_tokens.contains(occurrence->entityToken())) {
        return std::nullopt;
    }

    visited_occurrence_entity_tokens.insert(occurrence->entityToken());
    auto node = std::make_shared<GraphNode>(GraphNode{occurrence});

    for (const auto& occ : occurrence->childOccurrences()) {
        populate_node(occ, node, TRANSFORM, is_ground, visited_occurrence_entity_tokens, dynamic_joints);
    }

    for (const auto& joint : occurrence->joints()) {
        if (!joint || !joint->occurrenceOne() || !joint->occurrenceTwo()) {
            continue;
        }

        bool is_rigid   = joint->jointMotion()->jointType() == adsk::fusion::RigidJointType;
        auto connection = joint_connection(joint, occurrence);
        if (!connection) {
            continue;
        }

        if (!prev || connection->entityToken() != prev->data->entityToken()) {
            populate_node(connection, node, is_rigid ? CONNECTION : NEXT, is_ground, visited_occurrence_entity_tokens,
                dynamic_joints);
        }
    }

    if (prev) {
        prev->edges.push_back(std::make_shared<GraphEdge>(GraphEdge{relationship, node}));
    }

    return node;
}

std::optional<mirabuf::Node> create_tree_parts(
    std::shared_ptr<GraphNode> occurrence_node, OccurrenceRelationship relationship) {
    // NONE edges link occurrence nodes to joint-axis simulation nodes; they are
    // joint-level connections and must not be traversed as part of the occurrence
    // tree (that is populate_joint's job).
    if (relationship == NEXT || relationship == NONE || !occurrence_node->data->isLightBulbOn()) {
        return std::nullopt;
    }

    mirabuf::Node node;
    node.set_value(guid_occurrence(occurrence_node->data));
    for (auto edge : occurrence_node->edges) {
        auto child_node = create_tree_parts(edge->node, edge->relationship);
        if (child_node) {
            node.mutable_children()->Add()->CopyFrom(child_node.value());
        }
    }

    return node;
}

void populate_joint(std::shared_ptr<GraphNode> sim_node, mirabuf::joint::Joints* joints) {
    mirabuf::joint::JointInstance* joint = nullptr;
    if (!sim_node->joint.has_value()) {
        joint = &(*joints->mutable_joint_instances())["grounded"];
    } else {
        joint = &(*joints->mutable_joint_instances())[joint_entity_token(*sim_node->joint)];
    }

    assert(joint);
    auto root = create_tree_parts(sim_node, CONNECTION);
    if (root) {
        joint->mutable_parts()->mutable_nodes()->Add()->CopyFrom(root.value());
    }

    // Only follow NONE edges, those are the joint-level links inserted by
    // recurse_link_node_axis.  TRANSFORM/CONNECTION/NEXT edges are occurrence-
    // level relationships that belong to the occurrence tree, not the joint tree.
    for (auto edge : sim_node->edges) {
        if (edge->relationship == NONE) {
            populate_joint(edge->node, joints);
        }
    }
}

void get_all_joints(adsk::core::Ptr<adsk::fusion::Component> root_component,
    adsk::core::Ptr<adsk::fusion::Occurrence> grounded,
    std::vector<adsk::core::Ptr<adsk::fusion::Occurrence>>& grounded_connections,
    std::unordered_map<std::string, AnyJointPtr>& dynamic_joints) {
    auto process_joint = [&](const auto& joint) -> void {
        assert(joint);
        if (!joint->occurrenceOne() || !joint->occurrenceTwo()) {
            return;
        }

        if (joint->jointMotion()->jointType() != adsk::fusion::RigidJointType) {
            if (!dynamic_joints.contains(joint->occurrenceOne()->entityToken())) {
                dynamic_joints[joint->occurrenceOne()->entityToken()] = joint;
            }
        } else {
            if (joint->occurrenceOne()->entityToken() == grounded->entityToken()) {
                grounded_connections.push_back(joint->occurrenceTwo());
            } else if (joint->occurrenceTwo()->entityToken() == grounded->entityToken()) {
                grounded_connections.push_back(joint->occurrenceOne());
            }
        }
    };

    for (const auto& j : root_component->allJoints()) {
        process_joint(j);
    }

    for (const auto& j : root_component->allAsBuiltJoints()) {
        process_joint(j);
    }
}

void look_for_grounded_joints(const std::vector<adsk::core::Ptr<adsk::fusion::Occurrence>>& grounded_connections,
    const std::unordered_map<std::string, AnyJointPtr>& dynamic_joints, std::shared_ptr<GraphNode> root_node) {
    for (auto& grounded_connection : grounded_connections) {
        std::unordered_set<std::string> visited;
        populate_node(grounded_connection, root_node, CONNECTION, false, visited, dynamic_joints);
    }
}

void populate_axis(const adsk::core::Ptr<adsk::fusion::Design>& design,
    std::unordered_map<std::string, std::shared_ptr<GraphNode>>& simulation_nodes,
    const std::unordered_map<std::string, AnyJointPtr>& dynamic_joints, const std::string& occurrence_token,
    const AnyJointPtr& joint) {
    auto result = design->findEntityByToken(occurrence_token);
    if (result.empty() || !result.at(0)) {
        return;
    }

    auto occurrence = static_cast<adsk::core::Ptr<adsk::fusion::Occurrence>>(result[0]);
    if (!occurrence) {
        return;
    }

    std::unordered_set<std::string> visited;
    auto node = populate_node(occurrence, nullptr, NONE, false, visited, dynamic_joints);
    if (node) {
        node.value()->joint                = joint;
        simulation_nodes[occurrence_token] = node.value();
    }
}

std::vector<std::string> get_connected_axis_tokens(std::shared_ptr<GraphNode> start) {
    std::vector<std::string> tokens;
    std::unordered_set<const GraphNode*> visited_nodes;
    std::unordered_set<std::string> visited_tokens;

    std::stack<const GraphNode*> stack;
    stack.push(start.get());

    while (!stack.empty()) {
        const GraphNode* node = stack.top();
        stack.pop();
        if (!visited_nodes.insert(node).second) {
            continue;
        }

        for (const auto& edge : node->edges) {
            if (edge->relationship == NEXT) {
                std::string token = edge->node->data->entityToken();
                if (visited_tokens.insert(token).second) {
                    tokens.emplace_back(std::move(token));
                }
            } else {
                stack.push(edge->node.get());
            }
        }
    }

    return tokens;
}

void recurse_link_node_axis(std::shared_ptr<GraphNode> root_node,
    const std::unordered_map<std::string, std::shared_ptr<GraphNode>>& simulation_nodes) {
    const std::vector<std::string> tokens = get_connected_axis_tokens(root_node);
    for (const auto& key : tokens) {
        auto it = simulation_nodes.find(key);
        if (it == simulation_nodes.end()) {
            continue;
        }

        root_node->edges.push_back(std::make_shared<GraphEdge>(GraphEdge{NONE, it->second}));
        recurse_link_node_axis(it->second, simulation_nodes);
    }
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
    // TODO: Add comment
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

void build_joint_part_hierarchy(mirabuf::joint::Joints* joints, const adsk::core::Ptr<adsk::fusion::Design>& design) {
    std::unordered_set<std::string> visited_occurrence_entity_tokens;
    std::unordered_map<std::string, AnyJointPtr> dynamic_joints;
    std::unordered_map<std::string, std::shared_ptr<GraphNode>> simulation_nodes;
    std::vector<adsk::core::Ptr<adsk::fusion::Occurrence>> grounded_connections;

    auto grounded = search_for_grounded(design->rootComponent());

    // If there was anything that represented that the C++ exporter is currently
    // experimental it would be this. Not having a grounded node is a very common
    // user facing problem and simply asserting this will cause fusion to crash.
    // In the future if we want to actually support this section of the project
    // we will need to update this into an actual error system.
    //
    // Note for future development:
    // All instances of `assert(..)` need to be removed as Fusion simply cannot catch
    // these errors and will crash.
    assert(grounded);

    get_all_joints(design->rootComponent(), grounded, grounded_connections, dynamic_joints);

    auto root_node =
        populate_node(grounded, nullptr, NONE, true, visited_occurrence_entity_tokens, dynamic_joints).value();
    simulation_nodes["ground"] = root_node;

    look_for_grounded_joints(grounded_connections, dynamic_joints, root_node);

    for (const auto& [key, value] : dynamic_joints) {
        populate_axis(design, simulation_nodes, dynamic_joints, key, value);
    }

    recurse_link_node_axis(root_node, simulation_nodes);

    populate_joint(root_node, joints);
}

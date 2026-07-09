#include "joint_hierarchy.h"

#include <Core/Memory.h>
#include <Fusion/Components/AsBuiltJoint.h>
#include <Fusion/Components/Component.h>
#include <Fusion/Components/Joint.h>
#include <Fusion/Components/Occurrence.h>
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

#include "joint.pb.h"
#include "types.pb.h"

#include "util.h"

namespace {

using AnyJointPtr = std::variant<adsk::core::Ptr<adsk::fusion::Joint>, adsk::core::Ptr<adsk::fusion::AsBuiltJoint>>;

std::string joint_entity_token(const AnyJointPtr& joint) {
    return std::visit([](const auto& j) -> std::string { return j->entityToken(); }, joint);
}

adsk::core::Ptr<adsk::fusion::Occurrence> search_for_grounded(const adsk::core::Ptr<adsk::fusion::Component>& root) {
    auto all_occurrences = root->allOccurrences();
    if (!all_occurrences) {
        return nullptr;
    }

    for (const auto& occ : all_occurrences) {
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
    auto motion   = joint->jointMotion();
    bool is_rigid = motion && motion->jointType() == adsk::fusion::JointTypes::RigidJointType;
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

std::shared_ptr<GraphNode> populate_node(const adsk::core::Ptr<adsk::fusion::Occurrence>& occurrence,
    std::shared_ptr<GraphNode> prev, OccurrenceRelationship relationship, bool is_ground,
    std::unordered_set<std::string>& visited_occurrence_entity_tokens,
    const std::unordered_map<std::string, AnyJointPtr>& dynamic_joints) {
    if (occurrence->isGrounded() && !is_ground) {
        return nullptr;
    }

    if (relationship == NEXT && prev) {
        prev->edges.push_back(
            std::make_shared<GraphEdge>(GraphEdge{relationship, std::make_shared<GraphNode>(GraphNode{occurrence})}));
        return nullptr;
    }

    if (prev && dynamic_joints.contains(occurrence->entityToken())) {
        return nullptr;
    }

    if (visited_occurrence_entity_tokens.contains(occurrence->entityToken())) {
        return nullptr;
    }

    visited_occurrence_entity_tokens.insert(occurrence->entityToken());
    auto node = std::make_shared<GraphNode>(GraphNode{occurrence});

    auto child_occurrences = occurrence->childOccurrences();
    if (child_occurrences) {
        for (const auto& occ : child_occurrences) {
            populate_node(occ, node, TRANSFORM, is_ground, visited_occurrence_entity_tokens, dynamic_joints);
        }
    }

    auto joint_list = occurrence->joints();
    if (joint_list) {
        for (const auto& joint : joint_list) {
            if (!joint || !joint->occurrenceOne() || !joint->occurrenceTwo() || !joint->jointMotion()) {
                continue;
            }

            bool is_rigid   = joint->jointMotion()->jointType() == adsk::fusion::JointTypes::RigidJointType;
            auto connection = joint_connection(joint, occurrence);
            if (!connection) {
                continue;
            }

            if (!prev || connection->entityToken() != prev->data->entityToken()) {
                populate_node(connection, node, is_rigid ? CONNECTION : NEXT, is_ground,
                    visited_occurrence_entity_tokens, dynamic_joints);
            }
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
    // recurse_link_node_axis. TRANSFORM/CONNECTION/NEXT edges are occurrence-
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
        if (!joint->occurrenceOne() || !joint->occurrenceTwo() || !joint->jointMotion()) {
            return;
        }

        if (joint->jointMotion()->jointType() != adsk::fusion::JointTypes::RigidJointType) {
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
    if (std::string_view(result[0]->objectType()) != FusionTypeName<adsk::fusion::Occurrence>::value) {
        return;
    }

    auto occurrence = static_cast<adsk::core::Ptr<adsk::fusion::Occurrence>>(result[0]);

    std::unordered_set<std::string> visited;
    auto node = populate_node(occurrence, nullptr, NONE, false, visited, dynamic_joints);
    if (node) {
        node->joint                        = joint;
        simulation_nodes[occurrence_token] = node;
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

void build_joint_part_hierarchy(mirabuf::joint::Joints* joints, const adsk::core::Ptr<adsk::fusion::Design>& design) {
    std::unordered_set<std::string> visited_occurrence_entity_tokens;
    std::unordered_map<std::string, AnyJointPtr> dynamic_joints;
    std::unordered_map<std::string, std::shared_ptr<GraphNode>> simulation_nodes;
    std::vector<adsk::core::Ptr<adsk::fusion::Occurrence>> grounded_connections;

    auto grounded = search_for_grounded(design->rootComponent());

    if (!grounded) {
        return;
    }

    get_all_joints(design->rootComponent(), grounded, grounded_connections, dynamic_joints);

    auto root_node = populate_node(grounded, nullptr, NONE, true, visited_occurrence_entity_tokens, dynamic_joints);
    if (!root_node) {
        return;
    }
    simulation_nodes["ground"] = root_node;

    look_for_grounded_joints(grounded_connections, dynamic_joints, root_node);

    for (const auto& [key, value] : dynamic_joints) {
        populate_axis(design, simulation_nodes, dynamic_joints, key, value);
    }

    recurse_link_node_axis(root_node, simulation_nodes);

    populate_joint(root_node, joints);
}

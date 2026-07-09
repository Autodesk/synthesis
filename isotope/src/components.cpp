#include "components.h"

#include <Core/Geometry/Matrix3D.h>

#include <vector>

#include "assembly.pb.h"
#include "joint.pb.h"
#include "types.pb.h"

#include "util.h"

namespace {

mirabuf::PhysicalProperties map_physical_properties(
    const adsk::core::Ptr<adsk::fusion::PhysicalProperties>& properties) {
    mirabuf::PhysicalProperties new_properties;
    new_properties.set_mass(properties->mass());
    new_properties.set_volume(properties->volume());
    new_properties.set_density(properties->density());
    new_properties.set_area(properties->area());
    if (auto com = properties->centerOfMass()) {
        if (auto vec = com->asVector()) {
            new_properties.mutable_com()->set_x(vec->x());
            new_properties.mutable_com()->set_y(vec->y());
            new_properties.mutable_com()->set_z(vec->z());
        }
    }

    return new_properties;
}

mirabuf::TriangleMesh map_b_rep_body(const adsk::core::Ptr<adsk::fusion::BRepBody>& body) {
    auto mesh_mgr = body->meshManager();
    if (!mesh_mgr) {
        return {};
    }

    auto calc = mesh_mgr->createMeshCalculator();
    if (!calc) {
        return {};
    }

    calc->setQuality(adsk::fusion::TriangleMeshQualityOptions::LowQualityTriangleMesh);
    auto fus_mesh = calc->calculate();
    if (!fus_mesh) {
        return {};
    }

    mirabuf::TriangleMesh mesh;
    mesh.mutable_info()->CopyFrom(create_info_from_fus_obj(body));

    mesh.set_has_volume(true);

    std::vector<float> coords = fus_mesh->nodeCoordinatesAsFloat();
    mesh.mutable_mesh()->mutable_verts()->Add(coords.begin(), coords.end());

    std::vector<float> normals = fus_mesh->normalVectorsAsFloat();
    mesh.mutable_mesh()->mutable_normals()->Add(normals.begin(), normals.end());

    std::vector<int> node_indicies = fus_mesh->nodeIndices();
    mesh.mutable_mesh()->mutable_indices()->Add(node_indicies.begin(), node_indicies.end());

    std::vector<float> texture_coords = fus_mesh->textureCoordinatesAsFloat();
    mesh.mutable_mesh()->mutable_uv()->Add(texture_coords.begin(), texture_coords.end());

    return mesh;
}

mirabuf::TriangleMesh map_mesh_body(const adsk::core::Ptr<adsk::fusion::MeshBody>& body) {
    auto fus_mesh = body->displayMesh();
    if (!fus_mesh) {
        return {};
    }

    mirabuf::TriangleMesh mesh;
    mesh.mutable_info()->CopyFrom(create_info_from_fus_obj(body));
    mesh.set_has_volume(true);

    std::vector<float> coords = fus_mesh->nodeCoordinatesAsFloat();
    mesh.mutable_mesh()->mutable_verts()->Add(coords.begin(), coords.end());

    std::vector<float> normals = fus_mesh->normalVectorsAsFloat();
    mesh.mutable_mesh()->mutable_normals()->Add(normals.begin(), normals.end());

    std::vector<int> node_indicies = fus_mesh->nodeIndices();
    mesh.mutable_mesh()->mutable_indices()->Add(node_indicies.begin(), node_indicies.end());

    std::vector<float> texture_coords = fus_mesh->textureCoordinatesAsFloat();
    mesh.mutable_mesh()->mutable_uv()->Add(texture_coords.begin(), texture_coords.end());

    return mesh;
}

adsk::core::Ptr<adsk::core::Matrix3D> get_matrix_world(const adsk::core::Ptr<adsk::fusion::Occurrence>& occurrence) {
    if (!occurrence) {
        return nullptr;
    }

    auto transform = occurrence->transform2();
    if (!transform) {
        return nullptr;
    }

    auto matrix          = transform->copy();
    auto next_occurrence = occurrence;
    while (next_occurrence->assemblyContext()) {
        next_occurrence     = next_occurrence->assemblyContext();
        auto next_transform = next_occurrence->transform2();
        if (!next_transform) {
            break;
        }

        matrix->transformBy(next_transform);
    }

    return matrix;
}

mirabuf::Node parse_child_occurrence(
    const adsk::core::Ptr<adsk::fusion::Occurrence>& occurrence, mirabuf::Parts* parts) {
    if (!occurrence->isLightBulbOn()) {
        return {};
    }

    mirabuf::Node node;

    // TODO: Really explicit typing for this sort of thing would be great.
    const std::string map_constant = guid_occurrence(occurrence);
    node.set_value(map_constant);

    if (parts->part_instances().find(map_constant) != parts->part_instances().end()) {
        return node;
    }

    auto& part = (*parts->mutable_part_instances())[map_constant];
    part.mutable_info()->CopyFrom(create_info_from_fus_obj(occurrence, map_constant));
    if (occurrence->appearance()) {
        part.set_appearance(occurrence->appearance()->id());
    } else {
        part.set_appearance("default");
    }

    auto component = occurrence->component();
    if (component) {
        if (auto material = component->material()) {
            part.set_physical_material(material->id());
        }

        auto& part_defs                 = parts->part_definitions();
        const std::string component_ref = guid_component(component);
        if (part_defs.find(component_ref) != part_defs.end()) {
            part.set_part_definition_reference(component_ref);
        }
    }

    auto transform_array = occurrence->transform()->asArray();
    part.mutable_transform()->mutable_spatial_matrix()->Add(transform_array.begin(), transform_array.end());

    if (auto world_matrix = get_matrix_world(occurrence)) {
        auto world_transform = world_matrix->asArray();
        part.mutable_global_transform()->mutable_spatial_matrix()->Add(world_transform.begin(), world_transform.end());
    }

    // final recursive step to parse child occurrences
    std::vector<adsk::core::Ptr<adsk::fusion::Occurrence>> child_occurrences;
    if (auto occurrence_list = occurrence->childOccurrences()) {
        occurrence_list->copyTo(std::back_inserter(child_occurrences));
    }
    for (const auto& child_occurrence : child_occurrences) {
        if (!child_occurrence->isLightBulbOn()) {
            continue;
        }

        auto child_node = parse_child_occurrence(child_occurrence, parts);
        node.mutable_children()->Add()->CopyFrom(child_node);
    }

    return node;
}

mirabuf::Parts build_part_definitions(const adsk::core::Ptr<adsk::fusion::Components>& components,
    const google::protobuf::Map<std::string, mirabuf::material::Appearance>& appearances) {
    mirabuf::Parts parts;

    if (!components) {
        return parts;
    }

    std::vector<adsk::core::Ptr<adsk::fusion::Component>> fusion_components;
    components->copyTo(std::back_inserter(fusion_components));
    for (const auto& component : fusion_components) {
        const std::string component_ref = guid_component(component);
        if (parts.part_definitions().find(component_ref) != parts.part_definitions().end()) {
            continue;
        }

        auto& part = (*parts.mutable_part_definitions())[component_ref];
        part.mutable_info()->CopyFrom(create_info_from_fus_obj(component, component_ref));
        part.set_dynamic(true);

        if (auto props = component->physicalProperties()) {
            *part.mutable_physical_data() = map_physical_properties(props);
        }

        std::vector<adsk::core::Ptr<adsk::fusion::BRepBody>> b_rep_bodies;
        if (auto body_list = component->bRepBodies()) {
            body_list->copyTo(std::back_inserter(b_rep_bodies));
        }
        for (const auto& body : b_rep_bodies) {
            if (!body->isLightBulbOn()) {
                continue;
            }

            auto& part_body = *part.mutable_bodies()->Add();
            part_body.mutable_info()->CopyFrom(create_info_from_fus_obj(body));
            part_body.mutable_triangle_mesh()->CopyFrom(map_b_rep_body(body));

            auto appearance = body->appearance();
            if (appearance && appearances.find(appearance->id()) != appearances.end()) {
                part_body.set_appearance_override(appearance->id());
            } else {
                part_body.set_appearance_override("default");
            }
        }

        std::vector<adsk::core::Ptr<adsk::fusion::MeshBody>> mesh_bodies;
        if (auto mesh_body_list = component->meshBodies()) {
            mesh_body_list->copyTo(std::back_inserter(mesh_bodies));
        }
        for (const auto& body : mesh_bodies) {
            if (!body->isLightBulbOn()) {
                continue;
            }

            auto& part_body = *part.mutable_bodies()->Add();
            part_body.mutable_info()->CopyFrom(create_info_from_fus_obj(body));
            part_body.mutable_triangle_mesh()->CopyFrom(map_mesh_body(body));

            auto appearance = body->appearance();
            if (appearance && appearances.find(appearance->id()) != appearances.end()) {
                part_body.set_appearance_override(appearance->id());
            } else {
                part_body.set_appearance_override("default");
            }
        }
    }

    return parts;
}

mirabuf::Node parse_component_root(const adsk::core::Ptr<adsk::fusion::Component>& component, mirabuf::Parts* parts) {
    mirabuf::Node root_node;
    const std::string map_constant = guid_component(component);
    root_node.set_value(map_constant);

    if (parts->part_instances().find(map_constant) != parts->part_instances().end()) {
        return root_node;
    }

    auto& part = (*parts->mutable_part_instances())[map_constant];
    part.mutable_info()->CopyFrom(create_info_from_fus_obj(component, map_constant));
    auto& part_defs = parts->part_definitions();
    if (part_defs.find(map_constant) != part_defs.end()) {
        part.set_part_definition_reference(map_constant);
    }

    std::vector<adsk::core::Ptr<adsk::fusion::Occurrence>> child_occurrences;
    if (auto occurrence_list = component->occurrences()) {
        occurrence_list->copyTo(std::back_inserter(child_occurrences));
    }
    for (const auto& child_occurrence : child_occurrences) {
        if (!child_occurrence->isLightBulbOn()) {
            continue;
        }

        auto child_node = parse_child_occurrence(child_occurrence, parts);
        root_node.mutable_children()->Add()->CopyFrom(child_node);
    }

    return root_node;
}

} // namespace

std::pair<mirabuf::Parts, mirabuf::Node> map_parts(const adsk::core::Ptr<adsk::fusion::Components>& components,
    const adsk::core::Ptr<adsk::fusion::Component>& root, const mirabuf::material::Materials& materials) {
    auto parts     = build_part_definitions(components, materials.appearances());
    auto root_node = parse_component_root(root, &parts);
    return {std::move(parts), std::move(root_node)};
}

void map_rigid_groups(const adsk::core::Ptr<adsk::fusion::Component>& root, mirabuf::joint::Joints* joints) {
    for (const auto& fus_group : root->allRigidGroups()) {
        if (!fus_group) {
            continue;
        }

        auto mira_group = mirabuf::joint::RigidGroup();
        mira_group.set_name(fus_group->entityToken());

        auto occurrences = fus_group->occurrences();
        if (!occurrences) {
            continue;
        }

        for (const auto& occurrence : occurrences) {
            if (!occurrence || !occurrence->isLightBulbOn()) {
                continue;
            }

            mira_group.mutable_occurrences()->Add(guid_occurrence(occurrence));
        }

        if (mira_group.occurrences().size() > 1) {
            joints->mutable_rigid_groups()->Add()->CopyFrom(mira_group);
        }
    }
}

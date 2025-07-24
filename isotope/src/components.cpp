#include "components.h"

#include <vector>

#include "assembly.pb.h"
#include "types.pb.h"

namespace {

mirabuf::PhysicalProperties map_physical_properties(const adsk::core::Ptr<adsk::fusion::PhysicalProperties>& properties) {
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
    // auto calc = body->meshManager()->createMeshCalculator();
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
    mesh.mutable_info()->set_name(body->name());
    // TODO: Info guid
    mesh.mutable_info()->set_version(1);
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

    mirabuf::TriangleMesh mesh;
    // TODO: Info crap (this is getting annoying)
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

} // namespace

mirabuf::Parts map_all_parts(
    const adsk::core::Ptr<adsk::fusion::Components>& components,
    const mirabuf::material::Materials& materials) {
    mirabuf::Parts parts;

    std::vector<adsk::core::Ptr<adsk::fusion::Component>> fusion_components;
    components->copyTo(std::back_inserter(fusion_components));
    for (const auto& component : fusion_components) {
        auto& part = (*parts.mutable_part_definitions())[component->id()];
        part.mutable_info()->set_name(component->name());
        part.mutable_info()->set_guid(component->id());
        part.mutable_info()->set_version(1);
        part.set_dynamic(true);

        if (auto props = component->physicalProperties()) {
            *part.mutable_physical_data() = map_physical_properties(props);
        }

        std::vector<adsk::core::Ptr<adsk::fusion::BRepBody>> b_rep_bodies;
        component->bRepBodies()->copyTo(std::back_inserter(b_rep_bodies));
        for (const auto& body : b_rep_bodies) {
            if (!body->isLightBulbOn()) {
                continue;
            }

            auto& part_body = *part.mutable_bodies()->Add();
            part_body.mutable_info()->set_name(body->name());
            // TODO: guid
            part_body.mutable_info()->set_version(1);
            part_body.mutable_triangle_mesh()->CopyFrom(map_b_rep_body(body));

            if (auto appearances = materials.appearances(); // TODO: Replace the parameter
                appearances.find(body->appearance()->id()) != appearances.end()) {
                part_body.set_appearance_override(body->appearance()->id());
            } else {
                part_body.set_appearance_override("default");
            }
        }

        std::vector<adsk::core::Ptr<adsk::fusion::MeshBody>> mesh_bodies;
        component->meshBodies()->copyTo(std::back_inserter(mesh_bodies));
        for (const auto& body : mesh_bodies) {
            if (!body->isLightBulbOn()) {
                continue;
            }

            auto& part_body = *part.mutable_bodies()->Add();
            part_body.mutable_info()->set_name(body->name());
            part_body.mutable_info()->set_version(1);
            part_body.mutable_triangle_mesh()->CopyFrom(map_mesh_body(body));

            if (auto appearances = materials.appearances(); // TODO: Replace the parameter
                appearances.find(body->appearance()->id()) != appearances.end()) {
                part_body.set_appearance_override(body->appearance()->id());
            } else {
                part_body.set_appearance_override("default");
            }
        }
    }

    return parts;
}

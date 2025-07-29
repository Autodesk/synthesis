#include "materials.h"

#include "material.pb.h"

#include <Core/CoreAll.h>
#include <Fusion/FusionAll.h>

#include <vector>

namespace {

    mirabuf::material::Appearance default_appearance() {
        mirabuf::material::Appearance appearance;
        appearance.mutable_info()->set_name("Default Appearance");
        appearance.mutable_info()->set_guid("default-appearance-guid");
        appearance.mutable_info()->set_version(1);
        appearance.set_roughness(0.5f);
        appearance.set_metallic(0.5f);
        appearance.set_specular(0.5f);

        appearance.mutable_albedo()->set_r(127);
        appearance.mutable_albedo()->set_g(127);
        appearance.mutable_albedo()->set_b(127);
        appearance.mutable_albedo()->set_a(255);

        return appearance;
    }

    mirabuf::material::Appearance map_appearance(const adsk::core::Ptr<adsk::core::Appearance>& appearance) {
        mirabuf::material::Appearance new_appearance = default_appearance();
        new_appearance.mutable_info()->set_name(appearance->name());
        new_appearance.mutable_info()->set_guid(appearance->id());

        // TODO - Map all other appearance properties
        return new_appearance;
    }

    mirabuf::material::PhysicalMaterial default_physical_material() {
        mirabuf::material::PhysicalMaterial physical_material;
        physical_material.mutable_info()->set_name("Default Physical Material");
        physical_material.mutable_info()->set_guid("default-physical-material-guid");
        physical_material.mutable_info()->set_version(1);
        physical_material.set_dynamic_friction(0.5f);
        physical_material.set_static_friction(0.5f);
        physical_material.set_restitution(0.5f);
        physical_material.set_deformable(false);
        physical_material.set_mattype(mirabuf::material::PhysicalMaterial_MaterialType_METAL);

        return physical_material;
    }

    mirabuf::material::PhysicalMaterial map_physical_material(const adsk::core::Ptr<adsk::core::Material>& material) {
        mirabuf::material::PhysicalMaterial new_physical_material = default_physical_material();
        new_physical_material.mutable_info()->set_name(material->name());
        new_physical_material.mutable_info()->set_guid(material->id());

        // TODO - Map all other physical material properties
        return new_physical_material;
    }

} // namespace

mirabuf::material::Materials map_all_materials(const adsk::core::Ptr<adsk::core::Appearances>& design_appearances,
    const adsk::core::Ptr<adsk::core::Materials>& design_materials) {
    mirabuf::material::Materials materials;
    (*materials.mutable_appearances())["default"] = default_appearance();

    std::vector<adsk::core::Ptr<adsk::core::Appearance>> appearances;
    design_appearances->copyTo(std::back_inserter(appearances));
    for (const auto& appearance : appearances) {
        auto& new_appearance = (*materials.mutable_appearances())[appearance->id()];
        new_appearance       = map_appearance(appearance);
    }

    std::vector<adsk::core::Ptr<adsk::core::Material>> physical_materials;
    design_materials->copyTo(std::back_inserter(physical_materials));
    for (const auto& material : physical_materials) {
        auto& new_physical_material = (*materials.mutable_physicalmaterials())[material->id()];
        new_physical_material       = map_physical_material(material);
    }

    return materials;
}

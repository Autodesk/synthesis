#include "materials.h"

#include <Core/Application/Color.h>
#include <Core/Application/ColorProperty.h>
#include <Core/Application/FloatProperty.h>
#include <Core/Application/IntegerProperty.h>
#include <Core/Application/Property.h>
#include <Core/CoreAll.h>
#include <Core/Materials/AppearanceTextureProperty.h>
#include <Core/Memory.h>
#include <Fusion/FusionAll.h>

#include <algorithm>
#include <unordered_map>
#include <vector>

#include "material.pb.h"

#include "util.h"

namespace {

mirabuf::material::Appearance default_appearance() {
    mirabuf::material::Appearance appearance;
    appearance.mutable_info()->set_name("Default Appearance");
    appearance.mutable_info()->set_guid("default");
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
    mirabuf::material::Appearance result = default_appearance();
    result.mutable_info()->CopyFrom(create_info_from_fus_obj(appearance));
    result.set_roughness(0.9f);
    result.set_metallic(0.3f);
    result.set_specular(0.5f);
    result.mutable_albedo()->set_r(10);
    result.mutable_albedo()->set_g(10);
    result.mutable_albedo()->set_b(10);
    result.mutable_albedo()->set_a(127);

    auto properties = appearance->appearanceProperties();
    if (!properties) {
        return result;
    }

    if (auto p = properties->itemById("surface_roughness")) {
        if (auto fp = dynamic_cast<adsk::core::FloatProperty*>(p.get())) {
            result.set_roughness(fp->value());
        }
    }

    adsk::core::Ptr<adsk::core::IntegerProperty> model_item = properties->itemById("interior_model");
    if (!model_item) {
        return result;
    }

    const int model_type = model_item->value();

    if (model_type == 0) {
        if (auto p = properties->itemById("opaque_f0")) {
            if (auto fp = dynamic_cast<adsk::core::FloatProperty*>(p.get())) {
                result.set_metallic(fp->value());
            }
        }
    } else if (model_type == 1) {
        result.set_metallic(0.8f);
    }

    int16_t opacity = 255;
    if (model_type == 3) {
        adsk::core::Ptr<adsk::core::FloatProperty> dist = properties->itemById("transparent_distance");
        if (dist) {
            constexpr float OPACITY_RAMPING_CONSTANT = 14.0f;
            const float dist_val                     = static_cast<float>(dist->value());

            opacity = static_cast<int16_t>(
                std::clamp((255.0f * dist_val) / (dist_val + OPACITY_RAMPING_CONSTANT), 0.0f, 255.0f));
        }
    }

    const char* color_key = (model_type <= 1) ? "opaque_albedo" : "layered_diffuse";

    adsk::core::Ptr<adsk::core::Color> base_color         = nullptr;
    adsk::core::Ptr<adsk::core::ColorProperty> color_prop = properties->itemById(color_key);
    if (color_prop && color_prop->value()) {
        base_color = color_prop->value();
        base_color->opacity(opacity);
    }

    if (!base_color) {
        for (const auto& prop : properties) {
            if (prop->name() != "Color") {
                continue;
            }

            auto cp = dynamic_cast<adsk::core::ColorProperty*>(prop.get());
            if (!cp || !cp->value() || cp->id() == "surface_albedo") {
                continue;
            }

            base_color = cp->value();
            break;
        }
    }

    if (base_color) {
        result.mutable_albedo()->set_r(base_color->red());
        result.mutable_albedo()->set_g(base_color->green());
        result.mutable_albedo()->set_b(base_color->blue());
        result.mutable_albedo()->set_a(base_color->opacity());
    }

    return result;
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

void set_from_prop(const auto& props, const std::string& id, auto callback) {
    if (!props) {
        return;
    }

    if (auto p = props->itemById(id)) {
        if (auto fp = dynamic_cast<adsk::core::FloatProperty*>(p.get())) {
            callback(fp->value());
        }
    }
}

static const std::unordered_map<std::string, float> FRICTION_COEFFS = {
    {"Aluminum", 1.1f},
    {"Steel, Cast", 0.75f},
    {"Steel, Mild", 0.75f},
    {"Rubber, Nitrile", 1.0f},
    {"ABS Plastic", 0.7f},
};

mirabuf::material::PhysicalMaterial map_physical_material(const adsk::core::Ptr<adsk::core::Material>& material) {
    mirabuf::material::PhysicalMaterial new_physical_material = default_physical_material();
    new_physical_material.mutable_info()->CopyFrom(create_info_from_fus_obj(material));

    new_physical_material.set_deformable(false);
    new_physical_material.set_mattype(mirabuf::material::PhysicalMaterial_MaterialType_METAL);

    auto friction_it = FRICTION_COEFFS.find(material->name());
    float friction   = friction_it != FRICTION_COEFFS.end() ? friction_it->second : 0.5f;
    new_physical_material.set_dynamic_friction(friction);
    new_physical_material.set_static_friction(friction);
    new_physical_material.set_restitution(0.5f);

    auto mat_props             = material->materialProperties();
    auto mechanical_properties = new_physical_material.mutable_mechanical();
    auto strength_properties   = new_physical_material.mutable_strength();

    // clang-format off
    set_from_prop(mat_props, "structural_Young_modulus",            [&](float v) { mechanical_properties->set_young_mod(v); });
    set_from_prop(mat_props, "structural_Poisson_ratio",            [&](float v) { mechanical_properties->set_poisson_ratio(v); });
    set_from_prop(mat_props, "structural_Shear_modulus",            [&](float v) { mechanical_properties->set_shear_mod(v); });
    set_from_prop(mat_props, "structural_Density",                  [&](float v) { mechanical_properties->set_density(v); });
    set_from_prop(mat_props, "structural_Damping_coefficient",      [&](float v) { mechanical_properties->set_damping_coefficient(v); });
    set_from_prop(mat_props, "structural_Minimum_yield_stress",     [&](float v) { strength_properties->set_yield_strength(v); });
    set_from_prop(mat_props, "structural_Minimum_tensile_strength", [&](float v) { strength_properties->set_tensile_strength(v); });
    // clang-format on

    return new_physical_material;
}

} // namespace

mirabuf::material::Materials map_all_materials(const adsk::core::Ptr<adsk::core::Appearances>& design_appearances,
    const adsk::core::Ptr<adsk::core::Materials>& design_materials) {
    mirabuf::material::Materials materials;
    (*materials.mutable_appearances())["default"] = default_appearance();

    std::vector<adsk::core::Ptr<adsk::core::Appearance>> appearances;
    if (design_appearances) {
        design_appearances->copyTo(std::back_inserter(appearances));
    }
    for (const auto& appearance : appearances) {
        auto& new_appearance = (*materials.mutable_appearances())[appearance->id()];
        new_appearance       = map_appearance(appearance);
        new_appearance.mutable_info()->CopyFrom(create_info_from_fus_obj(appearance));
    }

    std::vector<adsk::core::Ptr<adsk::core::Material>> physical_materials;
    if (design_materials) {
        design_materials->copyTo(std::back_inserter(physical_materials));
    }
    for (const auto& material : physical_materials) {
        auto& new_physical_material = (*materials.mutable_physicalmaterials())[material->id()];
        new_physical_material       = map_physical_material(material);
        new_physical_material.mutable_info()->CopyFrom(create_info_from_fus_obj(material));
    }

    return materials;
}

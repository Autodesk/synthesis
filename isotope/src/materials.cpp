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
    mirabuf::material::Appearance new_appearance = default_appearance();
    new_appearance.mutable_info()->CopyFrom(create_info_from_fus_obj(appearance));

    new_appearance.set_roughness(0.9f);
    new_appearance.set_metallic(0.3f);
    new_appearance.set_specular(0.5f);

    new_appearance.mutable_albedo()->set_r(10);
    new_appearance.mutable_albedo()->set_g(10);
    new_appearance.mutable_albedo()->set_b(10);
    new_appearance.mutable_albedo()->set_a(127);

    auto properties = appearance->appearanceProperties();
    if (auto roughness_property = properties->itemById("surface_roughness")) {
        new_appearance.set_roughness(dynamic_cast<adsk::core::FloatProperty*>(roughness_property.get())->value());
    }

    adsk::core::Ptr<adsk::core::IntegerProperty> model_item = properties->itemById("interior_model");
    if (!model_item) {
        return new_appearance;
    }

    adsk::core::Ptr<adsk::core::Color> base_color = nullptr;

    int mat_model_type = model_item->value();
    switch (mat_model_type) {
        case 0: {
            if (auto reflectance_property = properties->itemById("opaque_f0")) {
                new_appearance.set_metallic(
                    dynamic_cast<adsk::core::FloatProperty*>(reflectance_property.get())->value());
            }

            adsk::core::Ptr<adsk::core::ColorProperty> color = properties->itemById("opaque_albedo");
            if (color && color->value()) {
                base_color = color->value();
                base_color->opacity(255);
            }

            break;
        }
        case 1: {
            new_appearance.set_metallic(0.8);

            adsk::core::Ptr<adsk::core::ColorProperty> color = properties->itemById("opaque_albedo");
            if (color && color->value()) {
                base_color = color->value();
                base_color->opacity(255);
            }

            break;
        }
        case 2: {
            adsk::core::Ptr<adsk::core::ColorProperty> color = properties->itemById("layered_diffuse");
            if (color && color->value()) {
                base_color = color->value();
                base_color->opacity(255);
            }

            break;
        }
        case 3: {
            adsk::core::Ptr<adsk::core::ColorProperty> color = properties->itemById("layered_diffuse");
            adsk::core::Ptr<adsk::core::FloatProperty> transparent_distance =
                properties->itemById("transparent_distance");

            constexpr float OPACITY_RAMPING_CONSTANT = 14.0f;
            float opacity =
                (255.0f * transparent_distance->value()) / (transparent_distance->value() + OPACITY_RAMPING_CONSTANT);
            if (opacity > 255) {
                opacity = 255;
            } else if (opacity < 0) {
                opacity = 0;
            }

            if (color && color->value()) {
                base_color = color->value();
                base_color->opacity(static_cast<int16_t>(opacity));
            }
            break;
        }
    }

    if (base_color) {
        new_appearance.mutable_albedo()->set_r(base_color->red());
        new_appearance.mutable_albedo()->set_g(base_color->green());
        new_appearance.mutable_albedo()->set_b(base_color->blue());
        new_appearance.mutable_albedo()->set_a(base_color->opacity());
    } else {
        for (auto prop : appearance->appearanceProperties()) {
            if (prop->name() == "Color") {
                auto color_property = dynamic_cast<adsk::core::ColorProperty*>(prop.get());
                if (color_property->value() && color_property->id() != "surface_albedo") {
                    new_appearance.mutable_albedo()->set_r(color_property->value()->red());
                    new_appearance.mutable_albedo()->set_g(color_property->value()->green());
                    new_appearance.mutable_albedo()->set_b(color_property->value()->blue());
                    new_appearance.mutable_albedo()->set_a(color_property->value()->opacity());
                    break;
                }
            }
        }
    }

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

#define SET_FROM_PROP(props, id, obj, method)                                  \
    do {                                                                       \
        if (auto p = (props)->itemById(id)) {                                  \
            if (auto fp = dynamic_cast<adsk::core::FloatProperty*>(p.get())) { \
                (obj)->method(fp->value());                                    \
            }                                                                  \
        }                                                                      \
    } while (0)

// Friction coefficients by Fusion material name, matching the Python exporter's lookup table.
static const std::unordered_map<std::string, float> FRICTION_COEFFS = {
    {"Aluminum",        1.1f},
    {"Steel, Cast",     0.75f},
    {"Steel, Mild",     0.75f},
    {"Rubber, Nitrile", 1.0f},
    {"ABS Plastic",     0.7f},
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

    SET_FROM_PROP(mat_props, "structural_Young_modulus", mechanical_properties, set_young_mod);
    SET_FROM_PROP(mat_props, "structural_Poisson_ratio", mechanical_properties, set_poisson_ratio);
    SET_FROM_PROP(mat_props, "structural_Shear_modulus", mechanical_properties, set_shear_mod);
    SET_FROM_PROP(mat_props, "structural_Density", mechanical_properties, set_density);
    SET_FROM_PROP(mat_props, "structural_Damping_coefficient", mechanical_properties, set_damping_coefficient);
    SET_FROM_PROP(mat_props, "structural_Minimum_yield_stress", strength_properties, set_yield_strength);
    SET_FROM_PROP(mat_props, "structural_Minimum_tensile_strength", strength_properties, set_tensile_strength);

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
        new_appearance.mutable_info()->CopyFrom(create_info_from_fus_obj(appearance));
    }

    std::vector<adsk::core::Ptr<adsk::core::Material>> physical_materials;
    design_materials->copyTo(std::back_inserter(physical_materials));
    for (const auto& material : physical_materials) {
        auto& new_physical_material = (*materials.mutable_physicalmaterials())[material->id()];
        new_physical_material       = map_physical_material(material);
        new_physical_material.mutable_info()->CopyFrom(create_info_from_fus_obj(material));
    }

    return materials;
}

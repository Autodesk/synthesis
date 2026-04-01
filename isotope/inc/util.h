#pragma once
#ifndef ISOTOPE_UTILITY_H_
#define ISOTOPE_UTILITY_H_

#include <Core/Base.h>
#include <Core/Memory.h>
#include <Fusion/Components/Component.h>
#include <Fusion/Components/Joint.h>
#include <Fusion/Components/JointGeometry.h>
#include <Fusion/Components/JointOrigin.h>
#include <Fusion/Components/Occurrence.h>

#include <string_view>
#include <variant>

#include "types.pb.h"

template <typename T>
struct FusionTypeName;

#define DEFINE_FUSION_TYPE_NAME(type)                    \
    template <>                                          \
    struct FusionTypeName<type> {                        \
        static constexpr std::string_view value = #type; \
    }

DEFINE_FUSION_TYPE_NAME(adsk::fusion::JointGeometry);
DEFINE_FUSION_TYPE_NAME(adsk::fusion::JointOrigin);
DEFINE_FUSION_TYPE_NAME(adsk::fusion::BRepEdge);
DEFINE_FUSION_TYPE_NAME(adsk::fusion::BRepFace);

template <typename T>
const T* fusion_try_cast(const adsk::core::Base* base) {
    if (!base) {
        return nullptr;
    } else if (std::string_view(base->objectType()) == FusionTypeName<T>::value) {
        return dynamic_cast<const T*>(base);
    } else {
        return nullptr;
    }
}

template <typename VariantT, typename T, typename... Ts>
VariantT fusion_base_to_variant_impl(const adsk::core::Base* raw) {
    if (auto* p = fusion_try_cast<T>(raw)) {
        return VariantT{std::in_place_type<const T*>, p};
    } else if constexpr (sizeof...(Ts) > 0) {
        return fusion_base_to_variant_impl<VariantT, Ts...>(raw);
    } else {
        return VariantT{std::in_place_type<std::monostate>};
    }
}

template <typename... Ts>
std::variant<std::monostate, const Ts*...> fusion_base_to_variant(const adsk::core::Base* base) {
    using VariantT = std::variant<std::monostate, const Ts*...>;
    if (!base) {
        return std::variant<std::monostate, const Ts*...>{};
    }

    return fusion_base_to_variant_impl<VariantT, Ts...>(base);
}

template <class... Ts>
struct overloaded : Ts... {
    using Ts::operator()...;
};

template <class... Ts>
overloaded(Ts...) -> overloaded<Ts...>;

template <class, class = void>
struct has_name : std::false_type {};

template <class T>
struct has_name<T, std::void_t<decltype(std::declval<T>()->name())>> : std::true_type {};

template <class, class = void>
struct has_entity_token : std::false_type {};

template <class T>
struct has_entity_token<T, std::void_t<decltype(std::declval<T>()->entityToken())>> : std::true_type {};

template <class, class = void>
struct has_id : std::false_type {};

template <class T>
struct has_id<T, std::void_t<decltype(std::declval<T>()->id())>> : std::true_type {};

template <typename FusObjPtr>
mirabuf::Info create_info_from_fus_obj(const FusObjPtr& obj, const std::string& override_guid = "") {
    mirabuf::Info info;

    // The python exporter sets all version numbers to 5.
    // This version number can be used to differentiate between robot exports from
    // the C++ and python exporters respectively.
    info.set_version(1);

    if constexpr (has_name<FusObjPtr>::value) {
        info.set_name(obj->name());
    }

    if (!override_guid.length()) {
        if constexpr (has_entity_token<FusObjPtr>::value) {
            info.set_guid(obj->entityToken());
        } else if constexpr (has_id<FusObjPtr>::value) {
            info.set_guid(obj->id());
        }
    } else {
        info.set_guid(override_guid);
    }

    return info;
}

std::string guid_component(const adsk::core::Ptr<adsk::fusion::Component>& component);
std::string guid_occurrence(const adsk::core::Ptr<adsk::fusion::Occurrence>& occurrence);

std::string uuid4();

#endif // ISOTOPE_UTILITY_H_

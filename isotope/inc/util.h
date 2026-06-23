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

#include <string>
#include <string_view>

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
DEFINE_FUSION_TYPE_NAME(adsk::fusion::Occurrence);

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

template <typename T>
concept HasName = requires(T t) { t->name(); };

template <typename T>
concept HasEntityToken = requires(T t) { t->entityToken(); };

template <typename T>
concept HasId = requires(T t) { t->id(); };

template <typename FusObjPtr>
mirabuf::Info create_info_from_fus_obj(const FusObjPtr& obj, const std::string& override_guid = "") {
    mirabuf::Info info;

    // The python exporter sets all version numbers to 5.
    // This version number can be used to differentiate between robot exports from
    // the C++ and python exporters respectively.
    info.set_version(6);

    if constexpr (HasName<FusObjPtr>) {
        info.set_name(obj->name());
    }

    if (!override_guid.length()) {
        if constexpr (HasEntityToken<FusObjPtr>) {
            info.set_guid(obj->entityToken());
        } else if constexpr (HasId<FusObjPtr>) {
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

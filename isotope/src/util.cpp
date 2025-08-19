#include "util.h"

#include <Fusion/Components/Component.h>
#include <Fusion/Components/Occurrence.h>

std::string guid_component(const adsk::core::Ptr<adsk::fusion::Component>& component) {
    std::string output;
    output += component->entityToken();
    output += "_";
    output += component->id();
    return output;
}

std::string guid_occurrence(const adsk::core::Ptr<adsk::fusion::Occurrence>& occurrence) {
    std::string output;
    output += occurrence->entityToken();
    output += "_";
    output += guid_component(occurrence->component());
    return output;
}

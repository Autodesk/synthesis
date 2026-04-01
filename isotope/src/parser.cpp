#include "parser.h"

#include <Core/Application/Document.h>
#include <Core/Application/Product.h>
#include <Core/Materials/MaterialLibraries.h>
#include <Core/UserInterface/ProgressDialog.h>
#include <Fusion/Components/Component.h>
#include <Fusion/Fusion/Design.h>
#include <Fusion/Fusion/FusionDocument.h>

#include <fstream>

#include <google/protobuf/util/json_util.h>

#include "assembly.pb.h"
#include "types.pb.h"

#include "components.h"
#include "joints.h"
#include "materials.h"
#include "util.h"

void export_design(const GlobalContext& gctx) {
    assert(gctx.isValid());
    auto document = gctx.app->activeDocument();
    auto design   = document->query<adsk::fusion::FusionDocument>()->design();

    mirabuf::Assembly assembly;
    assembly.mutable_info()->CopyFrom(create_info_from_fus_obj(design->rootComponent()));
    assembly.mutable_info()->set_guid(design->parentDocument()->name());

    // Determines if the exported design should be treated as a robot or field
    // assembly. Currently since there is no UI present in Isotope we default to
    // robot always. This could be changed if either 1) a UI is added to Isotope
    // or 2) some sort of algorithmic detection is added to determine if the
    // design is a robot or field assembly.
    assembly.set_dynamic(true);

    auto appearances = design->appearances();
    auto materials   = design->materials();
    assembly.mutable_data()->mutable_materials()->CopyFrom(map_all_materials(appearances, materials));

    auto components = design->allComponents();
    assembly.mutable_data()->mutable_parts()->CopyFrom(map_all_parts(components, assembly.data().materials()));

    mirabuf::Node root_node = parse_component_root(design->rootComponent(), assembly.mutable_data()->mutable_parts());
    assembly.mutable_design_hierarchy()->mutable_nodes()->Add()->CopyFrom(root_node);

    const auto [joints, signals] = populate_joints(design);

    assembly.mutable_data()->mutable_joints()->CopyFrom(joints);
    assembly.mutable_data()->mutable_signals()->CopyFrom(signals);

    map_rigid_groups(design->rootComponent(), assembly.mutable_data()->mutable_joints());

    auto joint_hierarchy = create_joint_graph(joints);
    assembly.mutable_joint_hierarchy()->CopyFrom(joint_hierarchy);

    build_joint_part_hierarchy(assembly.mutable_data()->mutable_joints(), design);

    // Print assembly as JSON
    std::string json_output;
    auto _ = google::protobuf::util::MessageToJsonString(assembly, &json_output);

    std::ofstream output_file(std::getenv("HOME") + std::string("/Desktop/assembly_debug.json"));
    if (!output_file.is_open()) {
        gctx.app->userInterface()->messageBox("Failed to open output file for writing.");
        return;
    }

    output_file << json_output;
    output_file.close();

    std::ofstream binary_output(
        std::getenv("HOME") + std::string("/Desktop/test_dozer.mira"), std::ios::out | std::ios::binary);
    if (!binary_output.is_open()) {
        gctx.app->userInterface()->messageBox("Failed to open output file for writing.");
        return;
    }

    if (!assembly.SerializeToOstream(&binary_output)) {
        gctx.app->userInterface()->messageBox("Failed to write binary.");
        return;
    }

    gctx.app->userInterface()->messageBox("Exported assembly!");
}

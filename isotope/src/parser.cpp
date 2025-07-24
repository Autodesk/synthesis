#include "parser.h"

#include "assembly.pb.h"

#include <Core/Application/Document.h>
#include <Core/Application/Product.h>
#include <Core/Materials/MaterialLibraries.h>
#include <Core/UserInterface/ProgressDialog.h>
#include <Fusion/Components/Component.h>
#include <Fusion/Fusion/Design.h>
#include <Fusion/Fusion/FusionDocument.h>

#include "materials.h"
#include "components.h"

#include <google/protobuf/util/json_util.h>

void export_design(const GlobalContext& gctx) {
    assert(gctx.isValid());
    auto document = gctx.app->activeDocument();
    auto design   = document->query<adsk::fusion::FusionDocument>()->design();

    mirabuf::Assembly assembly;
    assembly.mutable_info()->set_name(design->rootComponent()->name());
    assembly.mutable_info()->set_version(1);
    assembly.mutable_info()->set_guid(design->parentDocument()->name());

    // Determines if the exported design should be treated as a robot or field
    // assembly. Currently since there is no UI present in Isotope we default to
    // robot always. This could be changed if either 1) a UI is added to Isotope
    // or 2) some sort of algorithmic detection is added to determine if the
    // design is a robot or field assembly.
    assembly.set_dynamic(true);

    // auto process_dialog = gctx.ui->createProgressDialog();
    // process_dialog->isCancelButtonShown(true);
    // process_dialog->show("Exporting Design", "Exporting design to Isotope format...", 0, 100, 1);
    // process_dialog->progressValue(1);

    gctx.app->userInterface()->messageBox("Exporting design to Isotope format...");
    gctx.app->userInterface()->messageBox("Mapping materials...");

    auto appearances = design->appearances();
    auto materials = design->materials();
    assembly.mutable_data()->mutable_materials()->CopyFrom(map_all_materials(appearances, materials));

    gctx.app->userInterface()->messageBox("Mapping components...");
    auto components = design->allComponents();
    assembly.mutable_data()->mutable_parts()->CopyFrom(map_all_parts(components, assembly.data().materials()));

    gctx.app->userInterface()->messageBox("Done");

    // Print assembly as JSON
    std::string json_output;
    auto _ = google::protobuf::util::MessageToJsonString(assembly, &json_output);
    gctx.app->userInterface()->messageBox("Exported assembly:\n" + json_output);
}

void map_rigid_groups();
void parse_component_roots();
void populate_joints();
void create_joint_graph();
void build_joint_part_hierarchy();

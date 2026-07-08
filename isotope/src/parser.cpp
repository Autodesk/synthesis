#include "parser.h"

#include <Core/Application/Document.h>
#include <Core/Application/Product.h>
#include <Core/Materials/MaterialLibraries.h>
#include <Core/UserInterface/FileDialog.h>
#include <Fusion/Components/Component.h>
#include <Fusion/Fusion/Design.h>
#include <Fusion/Fusion/FusionDocument.h>

#include <fstream>

#include "assembly.pb.h"
#include "types.pb.h"

#include "components.h"
#include "joint_hierarchy.h"
#include "joints.h"
#include "materials.h"
#include "util.h"

void export_design(const GlobalContext& gctx) {
    if (!gctx.isValid()) {
        return;
    }

    auto document = gctx.app->activeDocument();
    if (!document) {
        gctx.ui->messageBox("No active document.");
        return;
    }

    auto fusion_document = document->query<adsk::fusion::FusionDocument>();
    if (!fusion_document) {
        gctx.ui->messageBox("Active document is not a Fusion design.");
        return;
    }

    auto design = fusion_document->design();
    if (!design) {
        gctx.ui->messageBox("Failed to get design from document.");
        return;
    }

    auto parent_document = design->parentDocument();
    if (!parent_document) {
        gctx.ui->messageBox("Failed to get parent document from design.");
        return;
    }

    mirabuf::Assembly assembly;
    assembly.mutable_info()->CopyFrom(create_info_from_fus_obj(design->rootComponent()));
    assembly.mutable_info()->set_guid(parent_document->name());

    assembly.set_dynamic(true);

    const auto materials = map_all_materials(design->appearances(), design->materials());
    assembly.mutable_data()->mutable_materials()->CopyFrom(materials);

    const auto [parts, root_node] = map_parts(design->allComponents(), design->rootComponent(), materials);
    assembly.mutable_data()->mutable_parts()->CopyFrom(parts);
    assembly.mutable_design_hierarchy()->mutable_nodes()->Add()->CopyFrom(root_node);

    const auto [joints, signals] = populate_joints(design);

    assembly.mutable_data()->mutable_joints()->CopyFrom(joints);
    assembly.mutable_data()->mutable_signals()->CopyFrom(signals);

    map_rigid_groups(design->rootComponent(), assembly.mutable_data()->mutable_joints());

    auto joint_hierarchy = create_joint_graph(joints);
    assembly.mutable_joint_hierarchy()->CopyFrom(joint_hierarchy);

    build_joint_part_hierarchy(assembly.mutable_data()->mutable_joints(), design);

    auto file_dialog = gctx.ui->createFileDialog();
    if (!file_dialog) {
        gctx.ui->messageBox("Failed to create file dialog.");
        return;
    }

    file_dialog->isMultiSelectEnabled(false);
    file_dialog->title("Export Robot");
    file_dialog->filter("Mirabuf Files (*.mira)");
    file_dialog->filterIndex(0);
    file_dialog->initialFilename(parent_document->name());

    if (file_dialog->showSave() != adsk::core::DialogResults::DialogOK) {
        return;
    }

    std::string output_path = file_dialog->filename();
    if (output_path.size() < 5 || output_path.substr(output_path.size() - 5) != ".mira") {
        output_path += ".mira";
    }

    std::string binary_data;
    if (!assembly.SerializeToString(&binary_data)) {
        gctx.ui->messageBox("Failed to serialize assembly.");
        return;
    }

    std::ofstream output(output_path, std::ios::out | std::ios::binary | std::ios::trunc);
    if (!output.is_open()) {
        gctx.ui->messageBox("Failed to open output file for writing.");
        return;
    }

    output.write(binary_data.data(), static_cast<std::streamsize>(binary_data.size()));
    output.close();

    if (output.fail()) {
        gctx.ui->messageBox("Failed to write output file.");
        return;
    }

    gctx.ui->messageBox("Exported robot!");
}

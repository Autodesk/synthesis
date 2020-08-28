import adsk, adsk.core, adsk.fusion, traceback

from apper import AppObjects
import time

def writeColor(f, prop):
    f.write(" ".join(["(" +
                      str(value.red) + " " +
                      str(value.green) + " " +
                      str(value.blue) + " " +
                      str(value.opacity) + ")"
                      for value in prop.values]) + ",")

def writeNum(f, prop):
    if prop.hasMultipleValues:
        f.write(" ".join([value for value in prop.values]) + ",")
    else:
        f.write(str(prop.value) + ",")

        #todo opaque_mfp??

def writeAppearanceProps(e, props):
    type = props.itemById("interior_model").value
    if type == 0:
        e.write("False,")
        writeColor(e, props.itemById("opaque_albedo"))
        writeNum(e, props.itemById("surface_roughness"))
        # writeNum(e, appear.itemById("opaque_f0"))

        if props.itemById("opaque_emission"):
            writeNum(e, props.itemById("opaque_luminance"))
            writeColor(e, props.itemById("opaque_luminance_modifier"))
            #todo opaque translucency??
    elif type == 1:
        e.write("True,")
        writeColor(e, props.itemById("metal_f0"))
        writeNum(e, props.itemById("surface_roughness"))
    elif type == 2:
        e.write("False,")
        writeColor(e, props.itemById("layered_diffuse"))
        writeNum(e, props.itemById("surface_roughness"))
        # writeColor(e, appear.itemById("layered_f0"))
    elif type == 3:
        e.write("False,")
        writeColor(e, props.itemById("transparent_color"))
        writeNum(e, props.itemById("surface_roughness"))

        writeNum(e, props.itemById("transparent_distance")) # translate to alpha?
        # writeNum(e, appear.itemById("transparent_ior"))
    elif type == 4:
        pass #todo ??
    elif type == 5:
        e.write("False,")
        writeColor(e, props.itemById("glazing_transmission_color"))
        writeNum(e, props.itemById("surface_roughness"))

    elif type is None:
        return
    else:
        AppObjects().ui.messageBox(str(type))
        return

def recursion(f, occur):
    for body in occur.bRepBodies:
        if body.appearanceSourceType == 4:
            f.write(f"body,{body.name},")
        for face in body.faces:
            if face.appearanceSourceType == 4:
                f.write(f"face,{body.name}-{face.tempId},")
    for childOccur in occur.childOccurrences:
        recursion(f, childOccur)


def exportAppearancePropertiesToFile():
    ao = AppObjects()

    filePath = '{0}{1}_{2}.{3}'.format('C:/temp/', "mats", int(time.time()), "csv")
    filePathExp = '{0}{1}_{2}.{3}'.format('C:/temp/', "exp", int(time.time()), "csv")

    # todo: report fusion crashes if request values for float property with one value only


    with open(filePath, "wt") as f:
        # for occur in ao.root_comp.occurrences:
        #     recursion(f, occur)
        with open(filePathExp, "wt") as e:
            e.write("ID,NAME,METAL,COLOR,ROUGHNESS\n")
            for lib in ao.app.materialLibraries:
                for appear in lib.appearances:
                    f.write("ID,NAME,HAS_TEXTURE\n")
                    f.write(appear.id + ",")
                    f.write(appear.name + ",")
                    f.write(str(appear.hasTexture) + ",\n\n")
                    f.write("ID,NAME,TYPE,VALUE\n")
                    e.write(appear.id + ",")
                    e.write(appear.name + ",")
                    writeAppearanceProps(e, appear.appearanceProperties)
                    for prop in appear.appearanceProperties:
                        if prop.name == "URN" or prop.name == "IMAGE":
                            continue
                        f.write(prop.id + ",")
                        f.write(prop.name + ",")
                        f.write(prop.objectType.split("adsk::core::")[1] + ",")
                        if prop.objectType == "adsk::core::ColorProperty":
                            f.write(" ".join(["(" +
                                             str(value.red) + " " +
                                             str(value.green) + " " +
                                             str(value.blue) + " " +
                                             str(value.opacity) + ")"
                                             for value in prop.values]) + ",")
                        elif prop.objectType == "adsk::core::BooleanProperty" or prop.objectType == "adsk::core::StringProperty":
                            f.write(str(prop.value) + ",")
                        elif prop.objectType == "adsk::core::FloatProperty" or prop.objectType == "adsk::core::IntegerProperty":
                            if prop.hasMultipleValues:
                                f.write(" ".join([value for value in prop.values]) + ",")
                            else:
                                f.write(str(prop.value) + ",")
                        elif prop.objectType == "adsk::core::ChoiceProperty":
                            (returnValue, names, choices) = prop.getChoices()
                            f.write(prop.value+"  ("+" ".join([name+":"+choice for (name, choice) in zip(names, choices)])+")")

                        f.write("\n")
                    f.write("\n\n")
                    e.write("\n")



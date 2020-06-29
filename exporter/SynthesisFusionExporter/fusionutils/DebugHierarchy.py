import adsk, adsk.core, adsk.fusion, traceback


def printTabs(num):
    for i in range(num):
        print("    ", end="")


def printOccurrence(occur, depth):
    printTabs(depth)
    print("[O] " + occur.name)
    printComponent(occur.component, depth)


def printComponent(comp, depth):
    printTabs(depth)
    print("[C] " + comp.name)
    if comp.joints.count > 0:
        printTabs(depth)
        print("Joints")
        for joint in comp.joints:
            printTabs(depth)
            print("| " + joint.name)
    for occur in comp.occurrences:
        printOccurrence(occur, depth + 1)


def printHierarchy(root_comp):
    print("------Full Hierarchy-------")
    printComponent(root_comp, 0)
    print("---------------------------")

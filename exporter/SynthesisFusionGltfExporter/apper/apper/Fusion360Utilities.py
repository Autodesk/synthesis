"""
Fusion360Utilities.py
=========================================================
Tools to leverage when creating a Fusion 360 Add-in

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
:copyright: (c) 2019 by Patrick Rainsberry.
:license: Apache 2.0, see LICENSE for more details.

"""
import adsk.core
import adsk.fusion
import adsk.cam
import traceback

from typing import Optional, List, Union

from functools import wraps

import os
from os.path import expanduser
import json
import uuid

import time

import logging


# Class to quickly access Fusion Application Objects
# TODO Doc string
class AppObjects(object):
    """The AppObjects class wraps many common application objects required when writing a Fusion 360 Addin."""

    def __init__(self):

        self.app = adsk.core.Application.cast(adsk.core.Application.get())

        # Get import manager
        self.import_manager = self.app.importManager

        # Get User Interface
        self.ui = self.app.userInterface

        self.document = self.app.activeDocument
        self.product = self.app.activeProduct

        self._design = self.design

    @property
    def design(self) -> Optional[adsk.fusion.Design]:
        """adsk.fusion.Design from the active document

        Returns: adsk.fusion.Design from the active document

        """
        design_ = self.document.products.itemByProductType('DesignProductType')
        if design_ is not None:
            return design_
        else:
            return None

    @property
    def cam(self) -> Optional[adsk.cam.CAM]:
        """adsk.cam.CAM from the active document

        Note if the document has never been activated in the CAM environment this will return None

        Returns: adsk.cam.CAM from the active document

        """
        cam_ = self.document.products.itemByProductType('CAMProductType')
        if cam_ is not None:
            return cam_
        else:
            return None

    @property
    def units_manager(self) -> Optional[adsk.core.UnitsManager]:
        """adsk.core.UnitsManager from the active document

        Returns: adsk.core.UnitsManager from the active document

        """
        if self.product.productType == 'DesignProductType':
            units_manager_ = self._design.fusionUnitsManager
        else:
            units_manager_ = self.product.unitsManager

        if units_manager_ is not None:
            return units_manager_
        else:
            return None

    @property
    def export_manager(self) -> Optional[adsk.fusion.ExportManager]:
        """adsk.fusion.ExportManager from the active document

        Returns: adsk.fusion.ExportManager from the active document

        """
        if self._design is not None:
            export_manager_ = self._design.exportManager
            return export_manager_
        else:
            return None

    @property
    def root_comp(self) -> Optional[adsk.fusion.Component]:
        """Every adsk.fusion.Design has exactly one Root Component

        It should also be noted that the Root Component in the Design does not have an associated Occurrence

        Returns: The Root Component of the adsk.fusion.Design

        """
        if self.product.productType == 'DesignProductType':
            root_comp_ = self.design.rootComponent
            return root_comp_
        else:
            return None

    @property
    def time_line(self) -> Optional[adsk.fusion.Timeline]:
        """adsk.fusion.Timeline from the active adsk.fusion.Design

        Returns: adsk.fusion.Timeline from the active adsk.fusion.Design

        """
        if self.product.productType == 'DesignProductType':
            if self._design.designType == adsk.fusion.DesignTypes.ParametricDesignType:
                time_line_ = self.product.timeline

                return time_line_

        return None


def start_group() -> int:
    """Starts a time line group

    Returns:
        The index of the adsk.fusion.Timeline where the adsk.fusion.TimelineGroup will begin
    """
    ao = AppObjects()

    # Start time line group
    start_index = ao.time_line.markerPosition

    return start_index


def end_group(start_index: int):
    """Ends a adsk.fusion.TimelineGroup

    start_index: adsk.fusion.TimelineG index that is returned from start_group
    """

    # Gets necessary application objects
    ao = AppObjects()

    end_index = ao.time_line.markerPosition - 1

    ao.time_line.timelineGroups.add(start_index, end_index)


def import_dxf(
        dxf_file: str,
        component: adsk.fusion.Component,
        plane: Union[adsk.fusion.ConstructionPlane, adsk.fusion.BRepFace]
) -> adsk.core.ObjectCollection:
    """Import dxf file with one sketch per layer.

    Args:
        dxf_file: The full path to the dxf file
        component: The target component for the new sketch(es)
        plane: The plane on which to import the DXF file.

    Returns:
        An ObjectCollection of the created sketches
    """

    ao = AppObjects()
    import_manager = ao.import_manager
    dxf_options = import_manager.createDXF2DImportOptions(dxf_file, plane)
    import_manager.importToTarget(dxf_options, component)
    sketches = dxf_options.results
    return sketches


def sketch_by_name(sketches: adsk.fusion.Sketches, name: str) -> adsk.fusion.Sketch:
    """Finds a sketch by name in a list of sketches

    Useful for parsing a collection of sketches such as DXF import results.

    Args:
        sketches: A list of sketches. (Likely would be all sketches in active document).
        name: The name of the sketch to find.

    Returns:
        The sketch matching the name if it is found.
    """
    return_sketch = None
    for sketch in sketches:
        if sketch.name == name:
            return_sketch = sketch
    return return_sketch


def extrude_all_profiles(sketch: adsk.fusion.Sketch, distance: float, component: adsk.fusion.Component,
                         operation: adsk.fusion.FeatureOperations) -> adsk.fusion.ExtrudeFeature:
    """Create extrude features of all profiles in a sketch

    The new feature will be created in the given target component and extruded by a distance

    Args:
        sketch: The sketch from which to get profiles
        distance: The distance to extrude the profiles.
        component: The target component for the extrude feature
        operation: The feature operation type from enumerator.

    Returns:
        The new extrude feature.
    """
    profile_collection = adsk.core.ObjectCollection.create()
    for profile in sketch.profiles:
        profile_collection.add(profile)

    extrudes = component.features.extrudeFeatures
    ext_input = extrudes.createInput(profile_collection, operation)
    distance_input = adsk.core.ValueInput.createByReal(distance)
    ext_input.setDistanceExtent(False, distance_input)
    extrude_feature = extrudes.add(ext_input)
    return extrude_feature


def create_component(target_component: adsk.fusion.Component, name: str) -> adsk.fusion.Occurrence:
    """Creates a new empty component in the target component

    Args:
        target_component: The target component for the new component
        name: The name of the new component

    Returns:
        The reference to the occurrence of the newly created component.

    """
    transform = adsk.core.Matrix3D.create()
    new_occurrence = target_component.occurrences.addNewComponent(transform)
    new_occurrence.component.name = name
    return new_occurrence


def rect_body_pattern(target_component: adsk.fusion.Component, bodies: List[adsk.fusion.BRepBody],
                      x_axis: adsk.core.Vector3D, y_axis: adsk.core.Vector3D,
                      x_qty: int, x_distance: float, y_qty: int, y_distance: float) -> adsk.core.ObjectCollection:
    """Creates rectangle pattern of bodies based on vectors

    Args:
        target_component: Component in which to create the patern
        bodies: bodies to pattern
        x_axis: vector defining direction 1
        y_axis: vector defining direction 2
        x_qty: Number of instances in direction 1
        x_distance: Distance between instances in direction 1
        y_qty: Number of instances in direction 2
        y_distance: Distance between instances in direction 2

    """
    move_feats = target_component.features.moveFeatures

    x_bodies = adsk.core.ObjectCollection.create()
    all_bodies = adsk.core.ObjectCollection.create()

    for body in bodies:
        x_bodies.add(body)
        all_bodies.add(body)

    for i in range(1, x_qty):

        # Create a collection of entities for move
        x_source = adsk.core.ObjectCollection.create()

        for body in bodies:
            new_body = body.copyToComponent(target_component)
            x_source.add(new_body)
            x_bodies.add(new_body)
            all_bodies.add(new_body)

        x_transform = adsk.core.Matrix3D.create()
        x_axis.normalize()
        x_axis.scaleBy(x_distance * i)
        x_transform.translation = x_axis

        move_input_x = move_feats.createInput(x_source, x_transform)
        move_feats.add(move_input_x)

    for j in range(1, y_qty):
        # Create a collection of entities for move
        y_source = adsk.core.ObjectCollection.create()

        for body in x_bodies:
            new_body = body.copyToComponent(target_component)
            y_source.add(new_body)
            all_bodies.add(new_body)

        y_transform = adsk.core.Matrix3D.create()
        y_axis.normalize()
        y_axis.scaleBy(y_distance * j)
        y_transform.translation = y_axis

        move_input_y = move_feats.createInput(y_source, y_transform)
        move_feats.add(move_input_y)

    return all_bodies


# Creates Combine Feature in target with all tool bodies as source
def combine_feature(target_body: adsk.fusion.BRepBody, tool_bodies: List[adsk.fusion.BRepBody],
                    operation: adsk.fusion.FeatureOperations):
    """Creates Combine Feature in target with all tool bodies as source

    Args:
        target_body: Target body for the combine feature
        tool_bodies: A list of tool bodies for the combine
        operation: An Enumerator defining the feature operation type
    """

    # Get Combine Features
    combine_features = target_body.parentComponent.features.combineFeatures

    # Define a collection and add all tool bodies to it
    combine_tools = adsk.core.ObjectCollection.create()

    for tool in tool_bodies:
        # todo add error checking
        combine_tools.add(tool)

    # Create Combine Feature
    combine_input = combine_features.createInput(target_body, combine_tools)
    combine_input.operation = operation
    combine_features.add(combine_input)


# Get default directory
def get_default_dir(app_name: str):
    """Creates a directory in the user's home folder to store data related to this app

    Args:
        app_name (str): Name of the Application
    """

    # Get user's home directory
    default_dir = expanduser("~")

    # Create a subdirectory for this application settings
    default_dir = os.path.join(default_dir, app_name, "")

    # Create the folder if it does not exist
    if not os.path.exists(default_dir):
        os.makedirs(default_dir)

    return default_dir


def get_settings_file(app_name: str):
    """Create (or get) a settings file name in the default app directory

    Args:
        app_name: Name of the Application
    """
    default_dir = get_default_dir(app_name)
    file_name = os.path.join(default_dir, ".settings.json")
    return file_name


# Write App Settings
def write_settings(app_name: str, settings: dict):
    """Write a settings file into the default directory for the app

    Args:
        app_name: Name of the Application
        settings: Stores a dictionary as a json string
    """
    settings_text = json.dumps(settings)
    file_name = get_settings_file(app_name)

    f = open(file_name, "w")
    f.write(settings_text)
    f.close()


# Read App Settings
def read_settings(app_name: str):
    """Read a settings file into the default directory for the app

    Args:
        app_name: Name of the Application
    """
    file_name = get_settings_file(app_name)
    if os.path.exists(file_name):
        with open(file_name) as f:
            try:
                settings = json.load(f)
            except:
                settings = {}
    else:
        settings = {}

    return settings


# Creates directory and returns file name for log file
def get_log_file_name(app_name: str):
    """Gets the filename for a default log file

    Args:
        app_name: Name of the Application
    """
    default_dir = get_default_dir(app_name)

    log_dir = os.path.join(default_dir, "logs", "")

    if not os.path.exists(log_dir):
        os.makedirs(log_dir)

    time_stamp = time.strftime("%Y-%m-%d-%H-%M-%S", time.gmtime())

    # Create file name in this path
    log_file_name = app_name + '-Log-' + time_stamp + '.txt'

    file_name = os.path.join(log_dir, log_file_name)

    return file_name


def open_doc(data_file: adsk.core.DataFile):
    """Simple wrapper to open a dataFile in the application window

    Args:
        data_file: The data file to open
    """
    app = adsk.core.Application.get()

    try:
        document = app.documents.open(data_file, True)
        if document is not None:
            document.activate()
    except:
        pass


def get_a_uuid() -> str:
    """Gets a base 64 uuid

    Returns:
         The id that was generated
    """
    r_uuid = str(uuid.uuid4())
    return r_uuid


def item_id(item: adsk.core.Base, group_name: str) -> str:
    """Gets (and possibly assigns) a unique identifier (UUID) to any item in Fusion 360

    Args:
        item: Any Fusion Object that supports attributes
        group_name: Name of the Attribute Group (typically use app_name)

    Returns:
        The id that was generated or was previously existing
    """
    this_id = None

    try:
        attributes = item.attributes

    except:
        return 'None'

    if attributes is not None:
        if attributes.itemByName(group_name, "id") is not None:
            this_id = attributes.itemByName(group_name, "id").value
        else:
            new_id = str(uuid.uuid4())
            attributes.add(group_name, "id", new_id)
            this_id = new_id

    return this_id


def remove_item_id(item: adsk.core.Base, group_name: str) -> bool:
    """Gets (and possibly assigns) a unique identifier (UUID) to any item in Fusion 360

    Args:
        item: Any Fusion Object that supports attributes
        group_name: Name of the Attribute Group (typically use app_name)

    Returns:
        True if successful and False if it failed
    """
    try:
        attributes = item.attributes
        if attributes.itemByName(group_name, "id") is not None:
            attribute: adsk.core.Attribute = attributes.itemByName(group_name, "id")
            attribute.deleteMe()
            return True

    except:
        pass

    return False


def get_item_by_id(this_item_id: str, app_name: str) -> adsk.core.Base:
    """Returns an item based on the assigned ID set with :func:`item_id <item_id>`

    Args:
        this_item_id: The unique id generated originally by calling :func:`item_id <item_id>`
        app_name: Name of the Application
        
    Returns:
        The Fusion 360 object that the id attribute was attached to.
    """
    ao = AppObjects()
    attributes = ao.design.findAttributes(app_name, "id")

    item = None
    for attribute in attributes:
        if attribute.value == this_item_id:
            item = attribute.parent

    return item


def get_log_file(app_name: str):
    """Gets the filename for a default log file

    Args:
        app_name: Name of the Application
    """
    default_dir = get_default_dir(app_name)
    file_name = os.path.join(default_dir, "logger.log")
    return file_name


def get_std_out_file(app_name: str):
    """Get temporary stdout file for the app

    Args:
        app_name: Name of the Application
    """
    default_dir = get_default_dir(app_name)
    file_name = os.path.join(default_dir, "std_out.txt")
    return file_name


def get_std_err_file(app_name: str):
    """Get temporary stderr file for the app

    Args:
        app_name: Name of the Application
    """
    default_dir = get_default_dir(app_name)
    file_name = os.path.join(default_dir, "std_err.txt")
    return file_name


#
# def timed(func):
#     """This decorator prints the execution time for the decorated function."""
#
#     @wraps(func)
#     def wrapper(*args, **kwargs):
#         start = time.time()
#         result = func(*args, **kwargs)
#         end = time.time()
#         logger.debug("{}, ran in ,{}".format(func.__name__, round(end - start, 3)))
#         return result
#
#     return wrapper
#
#
# def create_timed_logger(app_name):
#     logger = logging.getLogger(__name__)
#     logger.handlers = []
#     logger.setLevel("DEBUG")
#     handler = logging.FileHandler(get_log_file())
#     log_format = "%(asctime)s, %(levelname)s, -- ,%(message)s"
#     formatter = logging.Formatter(log_format)
#     handler.setFormatter(formatter)
#     logger.addHandler(handler)


class ProgressDialog:

    def __init__(
            self, cancel_text='cancel', progress_message='Processing: %v of %m',
            max_value=10, min_value=0, quit_message='Operation Cancelled', title='Progress'
    ):

        self.progress_message = progress_message
        self.quit_message = quit_message
        self.condition = False
        self.title = title
        self.min_value = min_value
        self.max_value = max_value

        ao = AppObjects()
        self.progress_dialog = ao.ui.createProgressDialog()
        self.progress_dialog.cancelButtonText = cancel_text
        self.progress_dialog.isBackgroundTranslucent = False
        self.progress_dialog.isCancelButtonShown = True
        # self.progress_dialog.message = progress_message
        self.progress_dialog.minimumValue = min_value
        self.progress_dialog.maximumValue = max_value

        self.progress_dialog.hide()

    def wait_with_progress(self):

        iteration = 0

        while self.condition is False:

            self.progress_dialog.progressValue = iteration

            self.my_wait_function()

            adsk.doEvents()

            if self.progress_dialog.wasCancelled:
                ao = AppObjects()
                ao.ui.messageBox(self.quit_message)

            time.sleep(1)

            iteration += 1

        self.progress_dialog.hide()

    def my_wait_function(self):
        pass

    def update_progress(self, progress_value, progress_message=None, max_value=None, reset=False):

        if max_value is not None:
            self.progress_dialog.maximumValue = max_value

        if progress_message is not None:
            self.progress_dialog.message = progress_message

        if reset:
            self.progress_dialog.reset()
            # self.progress_dialog.show(self.title, self.progress_message, self.min_value, self.max_value, 1)

        adsk.doEvents()

        self.progress_dialog.progressValue = progress_value

        adsk.doEvents()

        if self.progress_dialog.wasCancelled:
            # app = adsk.core.Application.get()
            # ao.ui.messageBox(self.quit_message)
            # app.userInterface.terminateActiveCommand()
            # raise InterruptedError()
            pass

        return True
# EOF

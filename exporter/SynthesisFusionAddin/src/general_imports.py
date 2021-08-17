import os, sys, uuid, json
import adsk.core, adsk.fusion, traceback
import logging.handlers, pathlib

from time import time
from datetime import datetime
from types import FunctionType

# hard coded to bypass errors for now
PROTOBUF = True
DEBUG = True

try:
    from .strings import *
    from .GlobalManager import *
    from .logging import setupLogger

    (root_logger, log_handler) = setupLogger()
except ImportError as e:
    # nothing to really do here
    print(e)

try:
    path = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))

    path_proto_files = os.path.abspath(
        os.path.join(os.path.dirname(__file__), "..", "proto", "proto_out")
    )

    if not path in sys.path:
        sys.path.insert(1, path)

    if not path_proto_files in sys.path:
        sys.path.insert(2, path_proto_files)

    from proto import deps

except:
    logging.getLogger(f"{INTERNAL_ID}.import_manager").error(
        "Failed\n{}".format(traceback.format_exc())
    )

try:
    # simple analytics endpoint
    # A_EP = AnalyticsEndpoint("UA-188467590-1", 1)
    A_EP = None

    # Setup the global state
    gm = GlobalManager()
    my_addin_path = os.path.dirname(os.path.realpath(__file__))
except:
    # should also log this
    logging.getLogger(f"{INTERNAL_ID}.import_manager").error(
        "Failed\n{}".format(traceback.format_exc())
    )

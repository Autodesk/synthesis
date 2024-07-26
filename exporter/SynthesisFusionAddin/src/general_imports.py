import json
import os
import pathlib
import sys
import traceback
import uuid
from datetime import datetime
from time import time
from types import FunctionType

import adsk.core
import adsk.fusion

from .GlobalManager import *
from .Logging import getLogger
from .strings import *

logger = getLogger()

# hard coded to bypass errors for now
PROTOBUF = True
DEBUG = True

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

    deps.installDependencies()

except:
    logger.error("Failed:\n{}".format(traceback.format_exc()))

try:
    # Setup the global state
    gm: GlobalManager = GlobalManager()
    my_addin_path = os.path.dirname(os.path.realpath(__file__))
except:
    logger.error("Failed:\n{}".format(traceback.format_exc()))

""" Stores data and fields from config.ini """

import logging.handlers
import traceback
import uuid
from configparser import ConfigParser

from .Logging import getLogger
from .strings import INTERNAL_ID
from .Types.OString import OString

logger = getLogger()

try:
    config = ConfigParser()

    # gets the parent directory
    file_path = OString.AddinPath("config.ini")

    # Reads Configuration File
    config.read(str(file_path))

    # MAIN
    DEBUG = config.getboolean("main", "DEBUG", fallback=False)

    # ANALYTICS
    ANALYTICS = config.getboolean("analytics", "analytics", fallback=False)
    NOTIFIED = config.getboolean("analytics", "notified", fallback=False)

    # PROTOBUF
    if DEBUG:
        AR = config.getboolean("main", "AR", fallback=False)

    if DEBUG:
        CID = "debug_user"
    else:
        if config.has_option("analytics", "c_id"):
            CID = config.get("analytics", "c_id")
        else:
            # in here we need to ask for analytics
            CID = uuid.uuid4()
            config.set("analytics", "c_id", str(CID))  # default values - add exception handling
except:
    logger.error(f"Failed\n{traceback.format_exc()}")


def setAnalytics(enabled: bool):
    logger.info(f"First run , Analytics set to {enabled}")
    ANALYTICS = enabled
    ans = "yes" if ANALYTICS else "no"
    write_configuration("analytics", "analytics", ans)
    return True


def get_value(key: str, section="main") -> any:
    """Gets a value from the config file

    Args:
        key (str): id of the value
        section (str, optional): section value is stored in. Defaults to "main".

    Returns:
        any: value
    """
    return config.get(section, key)


def write_configuration(section: str, key: str, value: any):
    """Write a configuration flag to the ini file

    Args:
        section (str): section e.g. [main]
        key (str): value identifier e.g. DEBUG
        value (any): value storead e.g. false
    """
    config.set(section, key, value)


def unload_config():
    """Unloads and writes the config to the file for next load"""
    with open(str(file_path), "w") as f:
        config.write(f)

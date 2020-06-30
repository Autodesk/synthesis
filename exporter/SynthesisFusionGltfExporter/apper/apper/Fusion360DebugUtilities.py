"""
Fusion360DebugUtilities.py
=========================================================
Utilities to aid in debugging a Fusion 360 Addin

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
:copyright: (c) 2019 by Patrick Rainsberry.
:license: Apache 2.0, see LICENSE for more details.

"""
import time
import os
from os.path import expanduser

import adsk.core
import adsk.fusion
import traceback


def variables_message(variables: list):
    """Print a list of list of variables

    Format of variables should be [[Variable name 1, variable value 1], [Variable name 2, variable value 2], ...]

    Args:
        variables: A list of lists of any string based variables from your add-in.
    """
    message_string = ''
    for variable in variables:
        message_string += variable[0] + ' = ' + str(variable[1]) + '\n'

    app = adsk.core.Application.get()
    ui = app.userInterface

    if ui:
        ui.messageBox(message_string)


def variable_message(variable, extra_info=''):
    """Displays the value of any single variable as long as the value can be converted to text

    Args:
        variable: variable to print
        extra_info: Any other info to display in the message box
    """
    message_string = str(variable)

    if len(extra_info) > 0:
        message_string += '   :   '

        message_string += extra_info

    app = adsk.core.Application.get()
    ui = app.userInterface

    if ui:
        ui.messageBox(message_string)


def perf_log(log, function_reference, command, identifier=''):
    """Performance time logging function
    Args:
        log:
        function_reference:
        command:
        identifier:
    """
    log.append((function_reference, command, identifier, time.process_time()))


def perf_message(log):
    """Performance time logging function
    Args:
        log: tbd
    """
    minimum_perf_time = .01
    message_string = ''

    log_file_name = get_log_file_name()
    log_file = open(log_file_name, 'w')

    total_t = log[-1][3] - log[0][3]

    message_string += 'Total Time = ' + "%0.6f" % total_t + '\n'

    for index, entry in enumerate(log[1:]):
        delta_t = entry[3] - log[index][3]

        if delta_t > minimum_perf_time:
            message_string += entry[0] + ' ' + entry[1] + ' ' + entry[2] + ' = ' + "%0.6f" % delta_t + '\n'

        log_file.write(entry[0] + ',' + entry[1] + ',' + entry[2] + ',' + str(delta_t) + '\n')

    log_file.close()

    app = adsk.core.Application.get()
    ui = app.userInterface

    if ui:
        ui.messageBox(message_string)


def get_log_file_name():
    """Creates directory and returns file name for log file
    Args:
        log: tbd
    """
    # Get Home directory
    home = expanduser("~")
    home += '/Fusion360DebugUtilities/'

    # Create if doesn't exist
    if not os.path.exists(home):
        os.makedirs(home)

    time_stamp = time.strftime("%Y-%m-%d-%H-%M-%S", time.gmtime())

    # Create file name in this path
    log_file_name = home + 'FusionDebugUtilities-PerfLog-' + time_stamp + '.csv'
    return log_file_name


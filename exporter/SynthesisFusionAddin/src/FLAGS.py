DEBUG = True  # Is current Debug session active ?
""" ## DEBUG flag to determine if developer build. 

    TODO: Should be built by some build system like cmake with a FLAGS.in file.
"""

PROGRESS = True  # Is showing the progress bar ?
""" ## PROGRESS flag to indicate if export progress bar shows up 

    TODO: Should be built by some build system like cmake with a FLAGS.in file.
"""

STAGING = False  # Is in the Staging build for Fusion ?
""" ## STAGING flag to indicate if being used in fusion staging builds 

    TODO: Should be built by some build system like cmake with a FLAGS.in file.
"""

GENERATEDOCS = True
""" ## Generate the Code Documentation using pdoc3 to the docs folder using the fusion python instance 

"""

PHYSICS = True  # Is exporting Physics model information ?
""" ## PHYSICS flag to indicate if exporting physics model information
    - COM (center of mass)
        - x
        - y
        - z
    - Density
    - Mass
    - Surface Area
    - etc

    This is part of the ParserOptions structure now - but this is a manual override

    TODO: Should be built by some build system like cmake with a FLAGS.in file.
"""

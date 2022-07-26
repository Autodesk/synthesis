class UiGlobal:

    """
    UiGlobal.INPUTS_ROOT (adsk.fusion.CommandInputs):
        - Provides access to the set of all commandInput ui elements in the panel
    """

    INPUTS_ROOT = None

    compress = False

    gamepiece_list_global = []
    wheel_list_global = []
    joint_list_global = []

    @staticmethod
    def wheel_table():
        """### Returns the wheel table command input

        Returns:
            adsk.fusion.TableCommandInput
        """
        return UiGlobal.INPUTS_ROOT.itemById("wheel_table")

    @staticmethod
    def joint_table():
        """### Returns the joint table command input

        Returns:
            adsk.fusion.TableCommandInput
        """
        return UiGlobal.INPUTS_ROOT.itemById("joint_table")

    @staticmethod
    def gamepiece_table():
        """### Returns the gamepiece table command input

        Returns:
            adsk.fusion.TableCommandInput
        """
        return UiGlobal.INPUTS_ROOT.itemById("gamepiece_table")

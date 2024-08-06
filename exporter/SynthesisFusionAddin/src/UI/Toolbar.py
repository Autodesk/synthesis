from src import INTERNAL_ID, gm
from src.Logging import logFailure


class Toolbar:
    """# Creates new tabbed toolbar
    - holds commands
    - holds handlers
    """

    uid = None
    tab = None
    panels = []
    controls = []

    @logFailure
    def __init__(self, name: str):
        self.uid = f"{name}_{INTERNAL_ID}_toolbar"
        self.name = name

        designWorkspace = gm.ui.workspaces.itemById("FusionSolidEnvironment")

        if designWorkspace:
            allDesignTabs = designWorkspace.toolbarTabs

            self.tab = allDesignTabs.itemById(self.uid)

            if self.tab is None:
                self.tab = allDesignTabs.add(self.uid, name)

            self.tab.activate()

    def getPanel(self, name: str, visibility: bool = True) -> str | None:
        """# Gets a control for a panel to the tabbed toolbar
        - optional param for visibility
        """
        panel_uid = f"{name}_{self.uid}"

        panel = self.tab.toolbarPanels.itemById(panel_uid)

        if panel is None:
            panel = self.tab.toolbarPanels.add(panel_uid, name)

        if panel:
            #    panel.isVisible = visibility
            self.panels.append(panel_uid)
            return panel_uid
        else:
            return None

    @logFailure
    @staticmethod
    def getNewPanel(name: str, tab_id: str, toolbar_id: str, visibility: bool = True) -> str | None:
        """# Gets a control for a panel to the tabbed toolbar visibility"""
        designWorkspace = gm.ui.workspaces.itemById("FusionSolidEnvironment")
        allDesignTabs = designWorkspace.toolbarTabs
        toolbar = allDesignTabs.itemById(toolbar_id)
        toolbar.activate()

        panel_uid = f"{name}_{INTERNAL_ID}_tooltab"
        panel = toolbar.toolbarPanels.itemById(panel_uid)

        if panel is None:
            panel = toolbar.toolbarPanels.add(panel_uid, name)

        gm.tabs.append(panel)
        return panel_uid

    def toggleVisibility(self, visible: bool) -> None:
        """# Toggles the visibility of the toolbar to the visibility param
        - Param: bool (visibility)
        """
        if self.tab is not False:
            self.tab.isVisible = visible

    def deleteMe(self) -> None:
        """# Execute in stop to delete fully
        - removes all controls for all child panels if they exist
        """
        tab = gm.ui.allToolbarTabs.itemById(self.uid)

        if tab:
            for panel in self.panels:
                panel_obj = gm.ui.allToolbarPanels.itemById(panel)
                if panel_obj is not None:
                    for control in panel_obj.controls:
                        if control.isValid:
                            control.deleteMe()
                    panel_obj.deleteMe()
            if tab.isValid:
                tab.deleteMe()

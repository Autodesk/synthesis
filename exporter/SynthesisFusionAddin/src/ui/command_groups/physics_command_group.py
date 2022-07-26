from .command_group import CommandGroup
from .. import icon_paths


class PhysicsCommandGroup(CommandGroup):

    def __init__(self, parent):
        super().__init__()
        self.parent = parent

    def configure(self):
        physics_settings = self.parent.advanced_settings.children.addGroupCommandInput(
                "physics_settings", "Physics Settings"
            )

        physics_settings.isExpanded = False
        physics_settings.isEnabled = True
        physics_settings.tooltip = "tooltip"  # TODO: update tooltip
        physics_settings = physics_settings.children

        self.parent.create_boolean_input(  # density checkbox
            "density",
            "Density",
            physics_settings,
            checked=True,
            tooltip="tooltip",  # TODO: update tooltip
            enabled=True,
        )

        self.parent.create_boolean_input(  # SA checkbox
            "surface_area",
            "Surface Area",
            physics_settings,
            checked=True,
            tooltip="tooltip",  # TODO: update tooltip
            enabled=True,
        )

        self.parent.create_boolean_input(  # restitution checkbox
            "restitution",
            "Restitution",
            physics_settings,
            checked=True,
            tooltip="tooltip",  # TODO: update tooltip
            enabled=True,
        )

        friction_override_table = self.parent.create_table_input(
            "friction_override_table",
            "",
            physics_settings,
            2,
            "1:2",
            1,
            column_spacing=25,
        )
        friction_override_table.tablePresentationStyle = 2
        friction_override_table.isFullWidth = True

        friction_override = self.parent.create_boolean_input(
            "friction_override",
            "",
        physics_settings,
            checked=False,
            tooltip="Manually override the default friction values on the bodies in the assembly.",
            enabled=True,
            is_check_box=False,
        )
        friction_override.resourceFolder = icon_paths.stringIcons["friction_override-enabled"]
        friction_override.isFullWidth = True

        valueList = [1]
        for i in range(20):
            valueList.append(i / 20)

        frictionCoeff = physics_settings.addFloatSliderListCommandInput(
            "friction_coeff_override", "Friction Coefficient", "", valueList
        )
        frictionCoeff.isVisible = False
        frictionCoeff.valueOne = 0.5
        frictionCoeff.tooltip = "Friction coefficient of field element."
        frictionCoeff.tooltipDescription = (
            "<i>Friction coefficients range from 0 (ice) to 1 (rubber).</i>"
        )

        friction_override_table.addCommandInput(friction_override, 0, 0)
        friction_override_table.addCommandInput(frictionCoeff, 0, 1)


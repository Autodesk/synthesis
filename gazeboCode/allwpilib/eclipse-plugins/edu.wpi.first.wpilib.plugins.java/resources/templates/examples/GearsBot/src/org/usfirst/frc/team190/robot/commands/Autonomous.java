
package $package.commands;

import edu.wpi.first.wpilibj.command.CommandGroup;

/**
 * The main autonomous command to pickup and deliver the
 * soda to the box.
 */
public class Autonomous extends CommandGroup {
    public Autonomous() {
    	addSequential(new PrepareToPickup());
        addSequential(new Pickup());
        addSequential(new SetDistanceToBox(0.10));
        // addSequential(new DriveStraight(4)); // Use Encoders if ultrasonic is broken
        addSequential(new Place());
        addSequential(new SetDistanceToBox(0.60));
        // addSequential(new DriveStraight(-2)); // Use Encoders if ultrasonic is broken
        addParallel(new SetWristSetpoint(-45));
        addSequential(new CloseClaw());
    }
}

package $package.commands;

import $package.Robot;

import edu.wpi.first.wpilibj.command.Command;

/**
 * Wait until the collector senses that it has the ball. This command does
 * nothing and is intended to be used in command groups to wait for this
 * condition.
 */
public class WaitForBall extends Command {

    public WaitForBall() {
    	requires(Robot.collector);
    }

    // Called just before this Command runs the first time
    protected void initialize() {}

    // Called repeatedly when this Command is scheduled to run
    protected void execute() {}

    // Make this return true when this Command no longer needs to run execute()
    protected boolean isFinished() {
        return Robot.collector.hasBall();
    }

    // Called once after isFinished returns true
    protected void end() {}

    // Called when another command which requires one or more of the same
    // subsystems is scheduled to run
    protected void interrupted() {}
}

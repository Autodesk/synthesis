package com.qualcomm.robotcore.eventloop.opmode;

/**
 * Clean-room shim of the real FTC SDK class. waitForStart/opModeIsActive/
 * idle/isStopRequested/runOpMode are verified byte-identical in the real SDK
 * across RobotCore 7.0.0 -> 11.1.0 (javap diff), so this is the most
 * load-bearing class in the shim -- it's the one piece guaranteed not to
 * drift out from under real team code.
 *
 * isStarted/stopRequested are package-private and driven by
 * {@link OpModeManagerBridge}, which stands in for the real SDK's internal
 * OpModeManagerImpl (also historically in this package).
 */
public abstract class LinearOpMode extends OpMode {
    /**
     * Real hardware loops land around 50-100Hz because actual I2C/USB motor
     * controller I/O has latency ours doesn't. Without this, an opmode with
     * no idle()/sleep of its own (e.g. ExampleDozerArcadeDrive) spins effectively
     * unthrottled and floods the Fission WS connection with a power update
     * every microsecond -- confirmed via the OpModeRunner smoke test before
     * this was added (1.7MB of traffic in ~5 seconds for 4 motors).
     */
    private static final long LOOP_PERIOD_MILLIS = 10;

    volatile boolean isStarted;
    volatile boolean stopRequested;

    public abstract void runOpMode() throws InterruptedException;

    public void waitForStart() {
        while (!isStarted && !stopRequested) {
            idle();
        }
    }

    public final boolean opModeIsActive() {
        boolean active = isStarted && !stopRequested;
        if (active) {
            idle();
        }
        return active;
    }

    public final boolean isStopRequested() {
        return stopRequested;
    }

    public final void idle() {
        try {
            Thread.sleep(LOOP_PERIOD_MILLIS);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }

    public final void requestOpModeStop() {
        stopRequested = true;
    }
}

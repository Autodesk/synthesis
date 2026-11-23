package com.qualcomm.robotcore.eventloop.opmode;

/**
 * Clean-room shim of the real FTC SDK class.
 */
public abstract class LinearOpMode extends OpMode {
    /**
     * Real hardware loops land around 50-100Hz because actual I2C/USB motor
     * controller I/O has latency, ours doesn't. Without this, an opmode with
     * no idle()/sleep of its own operates it loop unrealistically fast.
     */
    private static final long LOOP_PERIOD_MILLIS = 10;
    private static final double LOOP_PERIOD_SECONDS = LOOP_PERIOD_MILLIS / 1000.0;

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
            advanceRuntime(LOOP_PERIOD_SECONDS);
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

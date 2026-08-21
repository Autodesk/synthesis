package com.qualcomm.robotcore.eventloop.opmode;

/**
 * Our harness's replacement for the real SDK's internal OpModeManagerImpl.
 */
public final class OpModeManagerBridge {
    private OpModeManagerBridge() {
    }

    public static void start(LinearOpMode opMode) {
        opMode.isStarted = true;
    }

    public static void stop(LinearOpMode opMode) {
        opMode.stopRequested = true;
    }
}

package com.qualcomm.robotcore.eventloop.opmode;

/**
 * Not part of the real FTC SDK surface -- this is our harness's replacement
 * for the real SDK's internal OpModeManagerImpl (which lives in this same
 * package on-device and drives OpMode lifecycle via package-private access).
 * Lives in com.qualcomm.robotcore.eventloop.opmode purely to reach
 * LinearOpMode's package-private isStarted/stopRequested fields without
 * widening LinearOpMode's public API with harness-only methods that team
 * code could accidentally call.
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

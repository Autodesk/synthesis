package org.firstinspires.ftc.teamcode.examples;

import com.qualcomm.robotcore.hardware.DcMotorSimple;
import com.qualcomm.robotcore.eventloop.opmode.Autonomous;
import com.qualcomm.robotcore.eventloop.opmode.LinearOpMode;

/**
 * Arcade drive for Dozer, a 6-wheel robot with 3 motors ganged per side.
 * Left stick y drives forward/back, right stick x turns.
 */
@Autonomous(name = "Dozer Arcade Drive")
public class ExampleDozerAutoDrive extends LinearOpMode {
    @Override
    public void runOpMode() {
        DcMotorSimple leftFront   = hardwareMap.get(DcMotorSimple.class, "leftFront");
        DcMotorSimple leftMiddle  = hardwareMap.get(DcMotorSimple.class, "leftMiddle");
        DcMotorSimple leftBack    = hardwareMap.get(DcMotorSimple.class, "leftBack");
        DcMotorSimple rightFront  = hardwareMap.get(DcMotorSimple.class, "rightFront");
        DcMotorSimple rightMiddle = hardwareMap.get(DcMotorSimple.class, "rightMiddle");
        DcMotorSimple rightBack   = hardwareMap.get(DcMotorSimple.class, "rightBack");

        waitForStart();

        while (opModeIsActive()) {
            if (time > 2.0) {
                leftFront.setPower(0.0);
                leftMiddle.setPower(0.0);
                leftBack.setPower(0.0);
                rightFront.setPower(0.0);
                rightMiddle.setPower(0.0);
                rightBack.setPower(0.0);
            }
            else if (time > 1.0) {
                leftFront.setPower(-0.5);
                leftMiddle.setPower(-0.5);
                leftBack.setPower(-0.5);
                rightFront.setPower(-0.5);
                rightMiddle.setPower(-0.5);
                rightBack.setPower(-0.5);
            } else {
                leftFront.setPower(0.5);
                leftMiddle.setPower(0.5);
                leftBack.setPower(0.5);
                rightFront.setPower(0.5);
                rightMiddle.setPower(0.5);
                rightBack.setPower(0.5);
            }
        }
    }
}

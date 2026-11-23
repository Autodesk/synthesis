package org.firstinspires.ftc.teamcode.examples;

import com.qualcomm.robotcore.hardware.DcMotorSimple;
import com.qualcomm.robotcore.eventloop.opmode.TeleOp;
import com.qualcomm.robotcore.eventloop.opmode.LinearOpMode;

/**
 * Arcade drive for Dozer, a 6-wheel robot with 3 motors ganged per side.
 * Left stick y drives forward/back, right stick x turns.
 */
@TeleOp(name = "Dozer Arcade Drive")
public class ExampleDozerArcadeDrive extends LinearOpMode {
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
            double drive = -gamepad1.left_stick_y;
            double turn  = gamepad1.right_stick_x;

            double leftPower  = drive + turn;
            double rightPower = drive - turn;

            double max = Math.max(1.0, Math.max(Math.abs(leftPower), Math.abs(rightPower)));
            leftPower  /= max;
            rightPower /= max;

            leftFront.setPower(leftPower);
            leftMiddle.setPower(leftPower);
            leftBack.setPower(leftPower);
            rightFront.setPower(rightPower);
            rightMiddle.setPower(rightPower);
            rightBack.setPower(rightPower);
        }
    }
}

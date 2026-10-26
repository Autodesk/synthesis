package org.firstinspires.ftc.teamcode.examples;

import com.qualcomm.robotcore.eventloop.opmode.Autonomous;
import com.qualcomm.robotcore.eventloop.opmode.LinearOpMode;
import com.qualcomm.robotcore.hardware.DcMotorSimple;

@Autonomous(name = "Dozer Autonomous Drive")
public class ExampleDozerAutoDrive extends LinearOpMode {

    private enum DriveState {
        DRIVE_FORWARD,
        DRIVE_BACKWARD,
        STOPPED
    }

    @Override
    public void runOpMode() {
        // Hardware initialization
        DcMotorSimple leftFront   = hardwareMap.get(DcMotorSimple.class, "leftFront");
        DcMotorSimple leftMiddle  = hardwareMap.get(DcMotorSimple.class, "leftMiddle");
        DcMotorSimple leftBack    = hardwareMap.get(DcMotorSimple.class, "leftBack");
        DcMotorSimple rightFront  = hardwareMap.get(DcMotorSimple.class, "rightFront");
        DcMotorSimple rightMiddle = hardwareMap.get(DcMotorSimple.class, "rightMiddle");
        DcMotorSimple rightBack   = hardwareMap.get(DcMotorSimple.class, "rightBack");

        DriveState currentState = DriveState.DRIVE_FORWARD;

        waitForStart();

        while (opModeIsActive()) {
            if (time > 2.0) {
                currentState = DriveState.STOPPED;
            } else if (time > 1.0) {
                currentState = DriveState.DRIVE_BACKWARD;
            } else {
                currentState = DriveState.DRIVE_FORWARD;
            }

            double power = switch (currentState) {
                case DRIVE_FORWARD -> 0.5;
                case DRIVE_BACKWARD -> -0.5;
                case STOPPED -> 0.0;
            };

            setAllPower(leftFront, leftMiddle, leftBack, rightFront, rightMiddle, rightBack, power);
        }
    }

    // Helper method to reduce code repetition when setting motor power
    private void setAllPower(DcMotorSimple lf, DcMotorSimple lm, DcMotorSimple lb, 
                             DcMotorSimple rf, DcMotorSimple rm, DcMotorSimple rb, double power) {
        lf.setPower(power);
        lm.setPower(power);
        lb.setPower(power);
        rf.setPower(power);
        rm.setPower(power);
        rb.setPower(power);
    }
}

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
        DcMotorSimple leftBack    = hardwareMap.get(DcMotorSimple.class, "leftBack");
        DcMotorSimple rightFront  = hardwareMap.get(DcMotorSimple.class, "rightFront");
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

            switch (currentState) {
                case DRIVE_FORWARD:
                    setAllPower(leftFront, leftBack, rightFront, rightBack, 0.5);
                    break;

                case DRIVE_BACKWARD:
                    setAllPower(leftFront, leftBack, rightFront, rightBack, -0.5);
                    break;

                case STOPPED:
                    setAllPower(leftFront, leftBack, rightFront, rightBack, 0.0);
                    break;
            }
        }
    }

    // Helper method to reduce code repetition when setting motor power
    private void setAllPower(DcMotorSimple lf, DcMotorSimple lb, DcMotorSimple rf, DcMotorSimple rb, double power) {
        lf.setPower(power);
        lb.setPower(power);
        rf.setPower(power);
        rb.setPower(power);
    }
}

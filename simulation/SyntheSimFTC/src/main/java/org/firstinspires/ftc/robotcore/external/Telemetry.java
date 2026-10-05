package org.firstinspires.ftc.robotcore.external;

/**
 * Clean-room shim of the real FTC SDK interface, trimmed to the methods
 * teleop OpModes actually call.
 */
public interface Telemetry {
    void addData(String caption, Object value);

    void addData(String caption, String format, Object... args);

    void addLine(String lineCaption);

    boolean update();

    void clear();
}

package com.autodesk.synthesis.ftc;

import java.util.LinkedHashMap;
import java.util.Map;
import org.firstinspires.ftc.robotcore.external.Telemetry;

/** Prints to stdout on update() -- good enough until Fission has a telemetry readout panel. */
public class ConsoleTelemetry implements Telemetry {
    private final Map<String, Object> data = new LinkedHashMap<>();

    @Override
    public void addData(String caption, Object value) {
        data.put(caption, value);
    }

    @Override
    public void addData(String caption, String format, Object... args) {
        data.put(caption, String.format(format, args));
    }

    @Override
    public void addLine(String lineCaption) {
        data.put(lineCaption, "");
    }

    @Override
    public boolean update() {
        StringBuilder sb = new StringBuilder("[telemetry] ");
        data.forEach((k, v) -> sb.append(k).append(": ").append(v).append("  "));
        System.out.println(sb);
        data.clear();
        return true;
    }

    @Override
    public void clear() {
        data.clear();
    }
}

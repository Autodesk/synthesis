package com.autodesk.synthesis.ftc;

import com.google.gson.Gson;
import com.google.gson.JsonObject;
import com.qualcomm.robotcore.hardware.Gamepad;
import java.net.InetSocketAddress;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import org.java_websocket.WebSocket;
import org.java_websocket.handshake.ClientHandshake;
import org.java_websocket.server.WebSocketServer;

public class FTCWsBridge extends WebSocketServer {
    public static final int DEFAULT_PORT = 3301;
    private static final String PATH = "/ftcsimws";

    public interface ConnectionListener {
        void onFissionConnected();

        void onFissionDisconnected();

        void onDriverStationState(boolean enabled, boolean autonomous);
    }

    private final Gson gson = new Gson();
    private final Gamepad gamepad1 = new Gamepad();
    private final Gamepad gamepad2 = new Gamepad();
    private final Map<String, SynthesisDcMotor> dcMotors = new ConcurrentHashMap<>();
    private volatile ConnectionListener listener;

    private volatile boolean dsEnabled;
    private volatile boolean dsAutonomous;

    public FTCWsBridge(int port) {
        super(new InetSocketAddress(port));
    }

    public Gamepad gamepad1() {
        return gamepad1;
    }

    public Gamepad gamepad2() {
        return gamepad2;
    }

    public void setConnectionListener(ConnectionListener listener) {
        this.listener = listener;
    }

    public boolean isDriverStationEnabled() {
        return dsEnabled;
    }

    public boolean isDriverStationAutonomous() {
        return dsAutonomous;
    }

    public void registerDcMotor(String deviceName, SynthesisDcMotor motor) {
        dcMotors.put(deviceName, motor);
        Map<String, Object> init = new HashMap<>();
        init.put("<init", true);
        send("CANMotor", deviceName, init);
        send("CANEncoder", deviceName, init);
    }

    public void sendMotorPower(String deviceName, double power) {
        Map<String, Object> data = new HashMap<>();
        data.put("<percentOutput", power);
        send("CANMotor", deviceName, data);
    }

    //Announces the driver station deice
    private void registerDriverStation() {
        Map<String, Object> init = new HashMap<>();
        init.put("<init", true);
        send("DriverStation", "", init);
    }

    private void send(String type, String device, Map<String, Object> data) {
        JsonObject message = new JsonObject();
        message.addProperty("type", type);
        message.addProperty("device", device);
        message.add("data", gson.toJsonTree(data));
        String json = gson.toJson(message);

        for (WebSocket conn : getConnections()) {
            conn.send(json);
        }
    }

    @Override
    public void onOpen(WebSocket conn, ClientHandshake handshake) {
        if (!PATH.equals(handshake.getResourceDescriptor())) {
            conn.close(1002, "expected path " + PATH);
            return;
        }
        System.out.println("[FTCWsBridge] Fission connected from " + conn.getRemoteSocketAddress());
        registerDriverStation();
        ConnectionListener l = listener;
        if (l != null) {
            l.onFissionConnected();
        }
    }

    @Override
    public void onClose(WebSocket conn, int code, String reason, boolean remote) {
        System.out.println("[FTCWsBridge] Fission disconnected: " + reason);
        dcMotors.clear();
        ConnectionListener l = listener;
        if (l != null) {
            l.onFissionDisconnected();
        }
    }

    @Override
    public void onMessage(WebSocket conn, String message) {
        JsonObject json;
        try {
            json = gson.fromJson(message, JsonObject.class);
        } catch (Exception e) {
            System.err.println("[FTCWsBridge] Malformed message: " + message);
            return;
        }
        if (json == null || !json.has("type") || !json.has("data")) {
            return;
        }

        String type = json.get("type").getAsString();
        if ("Gamepad".equals(type)) {
            String device = json.has("device") ? json.get("device").getAsString() : "1";
            applyGamepadUpdate("2".equals(device) ? gamepad2 : gamepad1, json.getAsJsonObject("data"));
        } else if ("CANEncoder".equals(type) && json.has("device")) {
            SynthesisDcMotor motor = dcMotors.get(json.get("device").getAsString());
            if (motor != null) {
                motor.applyEncoderUpdate(json.getAsJsonObject("data"));
            }
        } else if ("DriverStation".equals(type)) {
            applyDriverStationUpdate(json.getAsJsonObject("data"));
        }
    }

    private void applyDriverStationUpdate(JsonObject data) {
        boolean changed = false;
        if (data.has(">enabled")) {
            dsEnabled = data.get(">enabled").getAsBoolean();
            changed = true;
        }
        if (data.has(">autonomous")) {
            dsAutonomous = data.get(">autonomous").getAsBoolean();
            changed = true;
        }
        if (!changed) {
            return;
        }

        ConnectionListener l = listener;
        if (l != null) {
            l.onDriverStationState(dsEnabled, dsAutonomous);
        }
    }

    private void applyGamepadUpdate(Gamepad gamepad, JsonObject data) {
        if (data.has("left_stick_x")) gamepad.left_stick_x = data.get("left_stick_x").getAsFloat();
        if (data.has("left_stick_y")) gamepad.left_stick_y = data.get("left_stick_y").getAsFloat();
        if (data.has("right_stick_x")) gamepad.right_stick_x = data.get("right_stick_x").getAsFloat();
        if (data.has("right_stick_y")) gamepad.right_stick_y = data.get("right_stick_y").getAsFloat();
        if (data.has("left_trigger")) gamepad.left_trigger = data.get("left_trigger").getAsFloat();
        if (data.has("right_trigger")) gamepad.right_trigger = data.get("right_trigger").getAsFloat();
        if (data.has("a")) gamepad.a = data.get("a").getAsBoolean();
        if (data.has("b")) gamepad.b = data.get("b").getAsBoolean();
        if (data.has("x")) gamepad.x = data.get("x").getAsBoolean();
        if (data.has("y")) gamepad.y = data.get("y").getAsBoolean();
        if (data.has("dpad_up")) gamepad.dpad_up = data.get("dpad_up").getAsBoolean();
        if (data.has("dpad_down")) gamepad.dpad_down = data.get("dpad_down").getAsBoolean();
        if (data.has("dpad_left")) gamepad.dpad_left = data.get("dpad_left").getAsBoolean();
        if (data.has("dpad_right")) gamepad.dpad_right = data.get("dpad_right").getAsBoolean();
        if (data.has("left_bumper")) gamepad.left_bumper = data.get("left_bumper").getAsBoolean();
        if (data.has("right_bumper")) gamepad.right_bumper = data.get("right_bumper").getAsBoolean();
        if (data.has("start")) gamepad.start = data.get("start").getAsBoolean();
        if (data.has("back")) gamepad.back = data.get("back").getAsBoolean();
    }

    @Override
    public void onError(WebSocket conn, Exception ex) {
        System.err.println("[FTCWsBridge] Error: " + ex.getMessage());
    }

    @Override
    public void onStart() {
        System.out.println("[FTCWsBridge] Listening on ws://localhost:" + getPort() + PATH);
    }
}

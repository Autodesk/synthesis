package com.autodesk.synthesis.cscore;

import java.net.InetSocketAddress;
import java.util.Base64;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

import org.java_websocket.WebSocket;
import org.java_websocket.handshake.ClientHandshake;
import org.java_websocket.server.WebSocketServer;

/**
 * A small localhost WebSocket server that receives rendered camera frames from Synthesis.
 *
 * Frame bytes are too large to travel over the HALSim {@link edu.wpi.first.hal.SimDevice}
 * channel (which only carries numbers and booleans), so Synthesis streams them here
 * instead — mirroring how it already connects out to the HALSim WebSocket. Each message is
 * the text {@code "<device>\n<base64-jpeg>"}; the most recent frame per device is kept and
 * served to {@link Camera#grabFrame}.
 */
public class CameraFrameServer extends WebSocketServer {

    /** Port Synthesis connects to in order to stream camera frames. */
    public static final int PORT = 5808;

    private static CameraFrameServer instance;

    private final Map<String, byte[]> m_frames = new ConcurrentHashMap<>();

    private CameraFrameServer(int port) {
        super(new InetSocketAddress("localhost", port));
        setReuseAddr(true);
    }

    /**
     * Returns the shared frame server, lazily starting it on first use.
     *
     * @return The singleton {@link CameraFrameServer}.
     */
    public static synchronized CameraFrameServer getInstance() {
        if (instance == null) {
            instance = new CameraFrameServer(PORT);
            instance.setDaemon(true);
            instance.start();
        }
        return instance;
    }

    /**
     * Gets the most recent frame received for a device.
     *
     * @param device The sim device key, e.g. {@code "USB Camera 0[0]"}.
     * @return The most recent encoded JPEG bytes, or null if none have been received.
     */
    public byte[] getFrame(String device) {
        return m_frames.get(device);
    }

    @Override
    public void onMessage(WebSocket conn, String message) {
        int idx = message.indexOf('\n');
        if (idx < 0) {
            return;
        }

        String device = message.substring(0, idx);
        String encoded = message.substring(idx + 1);
        try {
            m_frames.put(device, Base64.getDecoder().decode(encoded));
        } catch (IllegalArgumentException e) {
            // Ignore malformed frames.
        }
    }

    @Override
    public void onOpen(WebSocket conn, ClientHandshake handshake) {
    }

    @Override
    public void onClose(WebSocket conn, int code, String reason, boolean remote) {
    }

    @Override
    public void onError(WebSocket conn, Exception ex) {
    }

    @Override
    public void onStart() {
    }
}

package com.autodesk.synthesis.cscore;

import java.net.InetSocketAddress;
import java.util.Base64;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

import org.java_websocket.WebSocket;
import org.java_websocket.handshake.ClientHandshake;
import org.java_websocket.server.WebSocketServer;

/**
 * Localhost WebSocket server that receives rendered camera frames from Synthesis. Frames
 * can't ride HALSim (SimDevice carries only numbers/booleans), so Synthesis streams them
 * here as {@code "<device>\n<base64-jpeg>"}; the latest frame per device is kept for
 * {@link Camera#grabFrame}.
 */
public class CameraFrameServer extends WebSocketServer {

    public static final int PORT = 5808;

    private static CameraFrameServer instance;

    private final Map<String, byte[]> m_frames = new ConcurrentHashMap<>();

    private CameraFrameServer(int port) {
        super(new InetSocketAddress("localhost", port));
        setReuseAddr(true);
    }

    public static synchronized CameraFrameServer getInstance() {
        if (instance == null) {
            instance = new CameraFrameServer(PORT);
            instance.setDaemon(true);
            instance.start();
        }
        return instance;
    }

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
            // ignore malformed frames
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

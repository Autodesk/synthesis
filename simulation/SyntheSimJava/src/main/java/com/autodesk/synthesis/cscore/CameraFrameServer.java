package com.autodesk.synthesis.cscore;

import java.net.InetSocketAddress;
import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.util.Arrays;
import java.util.Base64;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

import org.java_websocket.WebSocket;
import org.java_websocket.handshake.ClientHandshake;
import org.java_websocket.server.WebSocketServer;

/**
 * localhost WebSocket server that receives rendered camera frames from Synthesis. Frames
 * can't use HALSim, so Synthesis streams them here as binary {@code "<device>\n<jpeg-bytes>"}
 * messages; the latest frame per device is kept for {@link Camera#grabFrame}.
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
    public void onMessage(WebSocket conn, ByteBuffer message) {
        byte[] data = new byte[message.remaining()];
        message.get(data);

        int idx = -1;
        for (int i = 0; i < data.length; i++) {
            if (data[i] == '\n') {
                idx = i;
                break;
            }
        }
        if (idx < 0) {
            return;
        }

        String device = new String(data, 0, idx, StandardCharsets.UTF_8);
        m_frames.put(device, Arrays.copyOfRange(data, idx + 1, data.length));
    }

    @Override
    public void onMessage(WebSocket conn, String message) {
        int idx = message.indexOf('\n');
        if (idx < 0) {
            return;
        }
        try {
            m_frames.put(message.substring(0, idx), Base64.getDecoder().decode(message.substring(idx + 1)));
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

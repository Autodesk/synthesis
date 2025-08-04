package com.autodesk.synthesis;

import org.java_websocket.WebSocket;
import org.java_websocket.handshake.ClientHandshake;
import org.java_websocket.server.WebSocketServer;

import java.net.InetSocketAddress;

/**
 * WebSocket server that receives messages from the Fission simulator
 * and forwards them to the WebSocketMessageHandler
 */
public class SynthesisWebSocketServer extends WebSocketServer {
    private static SynthesisWebSocketServer instance;
    private boolean isRunning = false;
    
    private SynthesisWebSocketServer(InetSocketAddress address) {
        super(address);
    }
    
    public static SynthesisWebSocketServer getInstance() {
        if (instance == null) {
            instance = new SynthesisWebSocketServer(new InetSocketAddress("localhost", 3300));
        }
        return instance;
    }
    
    @Override
    public void onOpen(WebSocket conn, ClientHandshake handshake) {
        System.out.println("WebSocket connection opened: " + conn.getRemoteSocketAddress());
    }
    
    @Override
    public void onClose(WebSocket conn, int code, String reason, boolean remote) {
        System.out.println("WebSocket connection closed: " + conn.getRemoteSocketAddress());
    }

    public void onServerStop() {
        isRunning = false;
    }
    
    @Override
    public void onMessage(WebSocket conn, String message) {
        WebSocketMessageHandler.getInstance().handleMessage(message);
    }
    
    @Override
    public void onError(WebSocket conn, Exception ex) {
        System.err.println("WebSocket error: " + ex.getMessage());
    }
    
    @Override
    public void onStart() {
        isRunning = true;
        System.out.println("WebSocket server started on port 3300");
        System.out.println("Listening for camera frames from Fission simulator...");
    }

    public void startServer() {
        if (!isRunning) {
            try {
                start();
                System.out.println("Synthesis WebSocket server starting on ws://localhost:3300/wpilibws");
            } catch (Exception e) {
                System.err.println("Error starting WebSocket server: " + e.getMessage());
            }
        } else {
            System.out.println("WebSocket server is already running");
        }
    }
    
    public void stopServer() {
        if (isRunning) {
            try {
                stop();
                onServerStop();
                System.out.println("WebSocket server stopped");
            } catch (Exception e) {
                System.err.println("Error stopping WebSocket server: " + e.getMessage());
            }
        }
    }
}
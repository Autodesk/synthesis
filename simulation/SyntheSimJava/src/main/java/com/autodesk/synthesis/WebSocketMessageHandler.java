package com.autodesk.synthesis;

import org.json.JSONObject;

/**
 * Handles incoming WebSocket messages from the simulation.
 * This processes various message types including camera frames.
 */
public class WebSocketMessageHandler {
    private static WebSocketMessageHandler instance;
    
    private WebSocketMessageHandler() {}
    
    public static WebSocketMessageHandler getInstance() {
        if (instance == null) {
            instance = new WebSocketMessageHandler();
        }
        return instance;
    }
    
    /**
     * Process incoming WebSocket message
     * @param messageJson JSON string containing the message
     */
    public void handleMessage(String messageJson) {
        try {
            JSONObject message = new JSONObject(messageJson);
            String type = message.getString("type");
            
            System.out.println("DEBUG: Received WebSocket message type: " + type);
            
            switch (type) {
                case "CAMERA_FRAME":
                    System.out.println("DEBUG: Processing camera frame message...");
                    handleCameraFrame(message);
                    break;
                default:
                    System.out.println("DEBUG: Unhandled message type: " + type);
                    break;
            }
        } catch (Exception e) {
            System.err.println("ERROR: Error processing WebSocket message: " + e.getMessage());
        }
    }
    
    /**
     * Handle camera frame message
     */
    private void handleCameraFrame(JSONObject message) {
        try {
            String device = message.getString("device");
            JSONObject data = message.getJSONObject("data");
            
            String frameData = data.getString("frame");
            int width = data.getInt("width");
            int height = data.getInt("height");
            
            // Forward to camera frame handler
            CameraFrameHandler.getInstance().handleFrame(device, frameData, width, height);
            
        } catch (Exception e) {
            System.err.println("ERROR: Error processing camera frame: " + e.getMessage());
        }
    }
    
    /**
     * Simulate receiving a camera frame
     */
    public void simulateTestMessage() {
        String testMessage = """
            {
                "type": "CAMERA_FRAME",
                "device": "USB Camera 0",
                "data": {
                    "frame": "",
                    "width": 640,
                    "height": 480,
                    "timestamp": %d
                }
            }
            """.formatted(System.currentTimeMillis());
        
        System.out.println("Simulating test WebSocket message...");
        handleMessage(testMessage);
    }
} 
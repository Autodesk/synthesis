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
        JSONObject message = new JSONObject(messageJson);
        String type = message.getString("type");
            
        System.out.println("DEBUG: Received WebSocket message type: " + type);
            
        switch (type) {
            case "CAMERA_FRAME":
                System.out.println("DEBUG: Processing camera frame message...");
                handleCameraFrame(message);
                break;
            default:
                // Handle other message types here
                System.out.println("DEBUG: Unhandled message type: " + type);
                break;
            }
    }
    
    /**
     * Handle camera frame message
     */
    private void handleCameraFrame(JSONObject message) {
        String device = message.getString("device");
        JSONObject data = message.getJSONObject("data");
            
        String frameData = data.getString("frame");
        int width = data.getInt("width");
        int height = data.getInt("height");
            
        // Forward to camera frame handler
        CameraFrameHandler.getInstance().handleFrame(device, frameData, width, height);
            
    }
} 
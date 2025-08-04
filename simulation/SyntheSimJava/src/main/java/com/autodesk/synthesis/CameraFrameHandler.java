package com.autodesk.synthesis;

import edu.wpi.first.cscore.CvSource;
import org.opencv.core.Mat;
import org.opencv.core.CvType;
import org.opencv.core.MatOfByte;
import org.opencv.imgcodecs.Imgcodecs;
import java.util.Base64;
import java.util.concurrent.ConcurrentHashMap;

/**
 * Handles camera frames from the simulation and feeds them to WPILib CvSource objects.
 * This bridges the gap between Synthesis 3D rendering and WPILib camera streaming.
 */
public class CameraFrameHandler {
    private static CameraFrameHandler instance;
    private final ConcurrentHashMap<String, CvSource> cameraSources = new ConcurrentHashMap<>();
    
    private CameraFrameHandler() {}
    
    public static CameraFrameHandler getInstance() {
        if (instance == null) {
            instance = new CameraFrameHandler();
        }
        return instance;
    }
    
    /**
     * Register a CvSource for a camera device
     * @param deviceName The name of the camera device (e.g., "USB Camera 0")
     * @param source The CvSource to feed frames to
     */
    public void registerCamera(String deviceName, CvSource source) {
        cameraSources.put(deviceName, source);
        System.out.println("Camera registered for: " + deviceName);
    }
    
    /**
     * Handle incoming camera frame from simulation
     * @param deviceName The camera device name
     * @param base64Frame Base64 encoded JPEG frame data
     * @param width Frame width
     * @param height Frame height
     */
    public void handleFrame(String deviceName, String base64Frame, int width, int height) {
        CvSource source = cameraSources.get(deviceName);
        
        try {
            byte[] frameData = Base64.getDecoder().decode(base64Frame);
            
            // Feed frame to CvSource and automatically streams to dashboards
            source.putFrame(frame);
            
            // Clean up
            frame.release();
            matOfByte.release();
            
        } catch (Exception e) {
            System.err.println("ERROR: Error processing camera frame for " + deviceName + ": " + e.getMessage());
        }
    }
    
    /**
     * Get registered camera count
     */
    public int getCameraCount() {
        return cameraSources.size();
    }
} 
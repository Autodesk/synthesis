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
        if (source == null) {
            System.out.println("DEBUG: Camera not registered for " + deviceName);
            return;
        }
        if (base64Frame == null || base64Frame.trim().isEmpty()) {
            System.out.println("DEBUG: Skipping empty frame for " + deviceName);
            return;
        }

        System.out.println("DEBUG: Received real 3D frame for " + deviceName + 
                         " (" + width + "x" + height + ", " + base64Frame.length() + " chars)");
        
        try {
            byte[] frameData = Base64.getDecoder().decode(base64Frame);
            
            // Check if decoded data is empty
            if (frameData.length == 0) {
                System.out.println("INFO: Skipping frame with empty data for " + deviceName);
                return;
            }
            
            // Convert JPEG bytes to OpenCV Mat
            MatOfByte matOfByte = new MatOfByte(frameData);
            Mat frame = Imgcodecs.imdecode(matOfByte, Imgcodecs.IMREAD_COLOR);
            
            if (frame.empty()) {
                System.err.println("WARNING: Failed to decode camera frame for " + deviceName);
                return;
            }
            
            // Ensure frame has correct dimensions
            if (frame.rows() != height || frame.cols() != width) {
                System.err.println("WARNING: Frame size mismatch for " + deviceName + 
                                 ": expected " + width + "x" + height + 
                                 ", got " + frame.cols() + "x" + frame.rows());
            }
            
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
     * Create a test pattern frame 
     */
    public void sendTestFrame(String deviceName, int width, int height) {
        CvSource source = cameraSources.get(deviceName);
        if (source == null) return;
        
        Mat testFrame = new Mat(height, width, CvType.CV_8UC3);
        testFrame.setTo(new org.opencv.core.Scalar(100, 150, 200)); // Light blue
        
        source.putFrame(testFrame);
        testFrame.release();
    }
    
    /**
     * Get registered camera count
     */
    public int getCameraCount() {
        return cameraSources.size();
    }
} 
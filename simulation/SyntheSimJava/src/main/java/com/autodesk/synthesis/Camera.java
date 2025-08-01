package com.autodesk.synthesis;

import edu.wpi.first.hal.SimBoolean;
import edu.wpi.first.hal.SimDevice;
import edu.wpi.first.hal.SimDevice.Direction;
import edu.wpi.first.hal.SimDouble;

public class Camera {
    private SimDevice m_device;

    private SimBoolean m_connected;
    private SimDouble m_width;
    private SimDouble m_height;
    private SimDouble m_fps;

    /**
     * Creates a Camera sim device in accordance with the WebSocket API Specification.
     * 
     * @param name Name of the Camera. This is generally the class name of the originating camera.
     * @param deviceId ID of the Camera.
     */
    public Camera(String name, int deviceId) {
        m_device = SimDevice.create("Camera:" + name, deviceId);

        m_connected = m_device.createBoolean("connected", Direction.kInput, false);
        m_width = m_device.createDouble("width", Direction.kBidir, 320);
        m_height = m_device.createDouble("height", Direction.kBidir, 240);
        m_fps = m_device.createDouble("fps", Direction.kBidir, 30);
    }

    /**
     * Get whether the camera is connected.
     *
     * @return true if connected, false otherwise
     */
    public boolean isConnected() {
        return m_connected.get();
    }

    /**
     * Set the camera connected status.
     *
     * @param connected connection status
     */
    public void setConnected(boolean connected) {
        m_connected.set(connected);
    }

    /**
     * Get the camera width.
     *
     * @return width in pixels
     */
    public double getWidth() {
        return m_width.get();
    }

    /**
     * Set the camera width.
     *
     * @param width width in pixels
     */
    public void setWidth(double width) {
        m_width.set(width);
    }

    /**
     * Get the camera height.
     *
     * @return height in pixels
     */
    public double getHeight() {
        return m_height.get();
    }

    /**
     * Set the camera height.
     *
     * @param height height in pixels
     */
    public void setHeight(double height) {
        m_height.set(height);
    }

    /**
     * Get the camera frame rate.
     *
     * @return fps (frames per second)
     */
    public double getFPS() {
        return m_fps.get();
    }

    /**
     * Set the camera frame rate.
     *
     * @param fps frames per second
     */
    public void setFPS(double fps) {
        m_fps.set(fps);
    }
}



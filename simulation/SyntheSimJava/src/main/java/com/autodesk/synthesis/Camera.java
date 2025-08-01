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
    private SimDouble m_brightness;
    private SimDouble m_exposure;
    private SimBoolean m_autoExposure;

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
        m_brightness = m_device.createDouble("brightness", Direction.kBidir, 50);
        m_exposure = m_device.createDouble("exposure", Direction.kBidir, 50);
        m_autoExposure = m_device.createBoolean("auto_exposure", Direction.kBidir, true);
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

    /**
     * Get the camera brightness.
     *
     * @return brightness (0-100)
     */
    public double getBrightness() {
        return m_brightness.get();
    }

    /**
     * Set the camera brightness.
     *
     * @param brightness brightness (0-100)
     */
    public void setBrightness(double brightness) {
        m_brightness.set(brightness);
    }

    /**
     * Get the camera exposure.
     *
     * @return exposure value
     */
    public double getExposure() {
        return m_exposure.get();
    }

    /**
     * Set the camera exposure.
     *
     * @param exposure exposure value
     */
    public void setExposure(double exposure) {
        m_exposure.set(exposure);
    }

    /**
     * Get whether auto exposure is enabled.
     *
     * @return true if auto exposure is enabled
     */
    public boolean getAutoExposure() {
        return m_autoExposure.get();
    }

    /**
     * Set auto exposure mode.
     *
     * @param autoExposure true to enable auto exposure
     */
    public void setAutoExposure(boolean autoExposure) {
        m_autoExposure.set(autoExposure);
    }
}



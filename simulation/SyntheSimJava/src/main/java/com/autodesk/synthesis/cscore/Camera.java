package com.autodesk.synthesis.cscore;

import org.opencv.core.Mat;
import org.opencv.core.MatOfByte;
import org.opencv.imgcodecs.Imgcodecs;

import edu.wpi.first.hal.SimBoolean;
import edu.wpi.first.hal.SimDevice;
import edu.wpi.first.hal.SimDevice.Direction;
import edu.wpi.first.hal.SimInt;

/**
 * Backing sim device for a simulated USB camera.
 *
 * The requested resolution / fps / connected state are published as HALSim outputs (read
 * by Synthesis to size and throttle its render). The rendered frame itself cannot travel
 * over HALSim — {@link SimDevice} only supports numeric and boolean values — so frames
 * are streamed separately by Synthesis to {@link CameraFrameServer} and matched back to
 * this device by name.
 *
 * See https://github.com/wpilibsuite/allwpilib/blob/main/simulation/halsim_ws_core/doc/hardware_ws_api.md
 * for documentation on the WebSocket API Specification.
 */
public class Camera {

    private SimDevice m_device;

    private SimInt m_width;
    private SimInt m_height;
    private SimInt m_fps;
    private SimBoolean m_connected;

    private final String m_deviceName;
    private final CameraFrameServer m_frameServer;

    /**
     * Creates a Camera sim device. The resulting sim device key seen by Synthesis is
     * {@code "<name>[<deviceId>]"}; Synthesis streams frames tagged with that same key.
     *
     * @param name     Name of the camera (matches the name configured in Synthesis).
     * @param deviceId USB device index.
     * @param width    Requested frame width in pixels.
     * @param height   Requested frame height in pixels.
     * @param fps      Requested frame rate.
     */
    public Camera(String name, int deviceId, int width, int height, int fps) {
        m_device = SimDevice.create("Camera:" + name, deviceId);

        if (m_device != null) {
            m_width = m_device.createInt("width", Direction.kOutput, width);
            m_height = m_device.createInt("height", Direction.kOutput, height);
            m_fps = m_device.createInt("fps", Direction.kOutput, fps);
            m_connected = m_device.createBoolean("connected", Direction.kOutput, true);
        }

        m_deviceName = name + "[" + deviceId + "]";
        m_frameServer = CameraFrameServer.getInstance();
    }

    /**
     * Sets the requested resolution, read by Synthesis to size its render.
     *
     * @param width  Frame width in pixels.
     * @param height Frame height in pixels.
     */
    public void setResolution(int width, int height) {
        if (m_width != null) {
            m_width.set(width);
            m_height.set(height);
        }
    }

    /**
     * Sets the requested frame rate, read by Synthesis to throttle its render.
     *
     * @param fps Frames per second.
     */
    public void setFPS(int fps) {
        if (m_fps != null) {
            m_fps.set(fps);
        }
    }

    /**
     * Sets whether the camera is connected.
     *
     * @param connected Whether the camera is connected.
     */
    public void setConnected(boolean connected) {
        if (m_connected != null) {
            m_connected.set(connected);
        }
    }

    /**
     * Decodes the latest frame supplied by Synthesis into the provided destination Mat.
     *
     * @param dst Destination matrix to receive the decoded BGR image.
     * @return true if a frame was available and decoded, false otherwise.
     */
    public boolean grabFrame(Mat dst) {
        byte[] bytes = m_frameServer.getFrame(m_deviceName);
        if (bytes == null || bytes.length == 0) {
            return false;
        }

        MatOfByte buffer = new MatOfByte(bytes);
        Mat decoded = Imgcodecs.imdecode(buffer, Imgcodecs.IMREAD_COLOR);
        buffer.release();

        if (decoded.empty()) {
            decoded.release();
            return false;
        }

        decoded.copyTo(dst);
        decoded.release();
        return true;
    }
}

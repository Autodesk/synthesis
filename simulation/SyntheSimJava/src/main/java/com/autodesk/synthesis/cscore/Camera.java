package com.autodesk.synthesis.cscore;

import org.opencv.core.Mat;
import org.opencv.core.MatOfByte;
import org.opencv.imgcodecs.Imgcodecs;

import edu.wpi.first.hal.SimBoolean;
import edu.wpi.first.hal.SimDevice;
import edu.wpi.first.hal.SimDevice.Direction;
import edu.wpi.first.hal.SimInt;

/**
 * Sim device for a USB camera.
 * Config published over HALSim, frame streamed from Fission to {@link CameraFrameServer}
 */
public class Camera {

    private SimDevice m_device;

    private SimInt m_width;
    private SimInt m_height;
    private SimInt m_fps;
    private SimBoolean m_connected;

    private final int m_defaultWidth;
    private final int m_defaultHeight;
    private final int m_defaultFps;

    private final String m_deviceName;
    private final CameraFrameServer m_frameServer;

    public Camera(String name, int deviceId, int width, int height, int fps) {
        m_device = SimDevice.create("Camera:" + name, deviceId);

        // null outside of simulation
        if (m_device != null) {
            m_width = m_device.createInt("width", Direction.kOutput, width);
            m_height = m_device.createInt("height", Direction.kOutput, height);
            m_fps = m_device.createInt("fps", Direction.kOutput, fps);
            m_connected = m_device.createBoolean("connected", Direction.kOutput, true);
        }

        m_defaultWidth = width;
        m_defaultHeight = height;
        m_defaultFps = fps;

        m_deviceName = name + "[" + deviceId + "]";
        m_frameServer = m_device != null ? CameraFrameServer.getInstance() : null;
    }

    /** @return true if running in simulation (frames come from Fission) */
    public boolean isSimulated() {
        return m_device != null;
    }

    public void setResolution(int width, int height) {
        if (m_width != null) {
            m_width.set(width);
            m_height.set(height);
        }
    }

    public void setFPS(int fps) {
        if (m_fps != null) {
            m_fps.set(fps);
        }
    }

    public void setConnected(boolean connected) {
        if (m_connected != null) {
            m_connected.set(connected);
        }
    }

    public int getWidth() {
        return m_width != null ? m_width.get() : m_defaultWidth;
    }

    public int getHeight() {
        return m_height != null ? m_height.get() : m_defaultHeight;
    }

    public int getFPS() {
        return m_fps != null ? m_fps.get() : m_defaultFps;
    }

    /**
     * Latest JPEG frame received from Fission, or null. Callers must track the returned
     * reference themselves to detect new frames (multiple consumers may grab concurrently).
     */
    public byte[] getLatestFrame() {
        return m_frameServer != null ? m_frameServer.getFrame(m_deviceName) : null;
    }

    /** @return true if {@code bytes} decoded into {@code dst} */
    public static boolean decodeFrame(byte[] bytes, Mat dst) {
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

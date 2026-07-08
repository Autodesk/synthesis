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

    private final String m_deviceName;
    private final CameraFrameServer m_frameServer;

    private byte[] m_lastFrame;

    public Camera(String name, int deviceId, int width, int height, int fps) {
        m_device = SimDevice.create("Camera:" + name, deviceId);

        // null outside of simulation
        if (m_device != null) {
            m_width = m_device.createInt("width", Direction.kOutput, width);
            m_height = m_device.createInt("height", Direction.kOutput, height);
            m_fps = m_device.createInt("fps", Direction.kOutput, fps);
            m_connected = m_device.createBoolean("connected", Direction.kOutput, true);
        }

        m_deviceName = name + "[" + deviceId + "]";
        m_frameServer = m_device != null ? CameraFrameServer.getInstance() : null;
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

    /** @return true if a frame was available and decoded into {@code dst} */
    public boolean grabFrame(Mat dst) {
        if (m_frameServer == null) return false;

        byte[] bytes = m_frameServer.getFrame(m_deviceName);
        // check bytes ref against m_lastFrame, only republish if new frame
        if (bytes == null || bytes.length == 0 || bytes == m_lastFrame) {
            return false;
        }
        m_lastFrame = bytes;

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

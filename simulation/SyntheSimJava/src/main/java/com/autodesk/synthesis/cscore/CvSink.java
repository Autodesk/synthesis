package com.autodesk.synthesis.cscore;

import org.opencv.core.Mat;

import edu.wpi.first.util.WPIUtilJNI;

/**
 * Swap-in for {@code edu.wpi.first.cscore.CvSink}
 */
public class CvSink extends edu.wpi.first.cscore.CvSink {

    private Camera m_camera;
    private byte[] m_lastFrame;

    /** If camera is null, behave as real cscore CvSink */
    public CvSink(String name, Camera camera) {
        super(name);
        this.m_camera = camera;
    }

    @Override
    public long grabFrame(Mat image) {
        return m_camera == null ? super.grabFrame(image) : grabFrameNoTimeout(image);
    }

    @Override
    public long grabFrame(Mat image, double timeout) {
        return m_camera == null ? super.grabFrame(image, timeout) : grabFrameNoTimeout(image);
    }

    @Override
    public long grabFrameNoTimeout(Mat image) {
        if (m_camera == null) {
            return super.grabFrameNoTimeout(image);
        }

        byte[] bytes = m_camera.getLatestFrame();
        if (bytes == null || bytes == m_lastFrame || !Camera.decodeFrame(bytes, image)) {
            return 0L;
        }
        m_lastFrame = bytes;

        return WPIUtilJNI.now();
    }
}

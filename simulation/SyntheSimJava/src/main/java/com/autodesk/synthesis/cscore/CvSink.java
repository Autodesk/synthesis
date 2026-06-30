package com.autodesk.synthesis.cscore;

import org.opencv.core.Mat;

/**
 * Swap-in for {@code edu.wpi.first.cscore.CvSink}: the frame-grab methods return the frame
 * Synthesis rendered for the associated {@link Camera} instead of a physical source, so
 * existing OpenCV processing keeps working.
 */
public class CvSink extends edu.wpi.first.cscore.CvSink {

    private Camera m_camera;

    public CvSink(String name, Camera camera) {
        super(name);
        this.m_camera = camera;
    }

    @Override
    public long grabFrame(Mat image) {
        return grabFrameNoTimeout(image);
    }

    @Override
    public long grabFrame(Mat image, double timeout) {
        return grabFrameNoTimeout(image);
    }

    @Override
    public long grabFrameNoTimeout(Mat image) {
        return m_camera.grabFrame(image) ? 1L : 0L;
    }
}

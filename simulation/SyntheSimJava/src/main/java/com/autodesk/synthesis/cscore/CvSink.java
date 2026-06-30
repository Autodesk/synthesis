package com.autodesk.synthesis.cscore;

import org.opencv.core.Mat;

/**
 * CvSink wrapper to add WPILib HALSim camera support. Rather than reading frames from a
 * physical video source, the frame-grabbing methods are overridden to return the frame
 * rendered by Synthesis for the associated {@link Camera}.
 *
 * Mirrors the swap-in pattern used for motors (see
 * {@code com.autodesk.synthesis.revrobotics.spark.SparkMax}): use this in place of
 * {@code edu.wpi.first.cscore.CvSink} and existing OpenCV processing keeps working.
 *
 * See original documentation for more information
 * https://github.wpilib.org/allwpilib/docs/release/java/edu/wpi/first/cscore/CvSink.html
 */
public class CvSink extends edu.wpi.first.cscore.CvSink {

    private Camera m_camera;

    /**
     * Creates a new simulation-backed CvSink.
     *
     * @param name   Name of the sink.
     * @param camera The Synthesis camera supplying frames.
     */
    public CvSink(String name, Camera camera) {
        super(name);
        this.m_camera = camera;
    }

    /**
     * Grabs the most recent frame supplied by Synthesis.
     *
     * @param image Destination matrix.
     * @return A non-zero value on success, 0 if no frame was available.
     */
    @Override
    public long grabFrame(Mat image) {
        return grabFrameNoTimeout(image);
    }

    /**
     * Grabs the most recent frame supplied by Synthesis, ignoring the timeout.
     *
     * @param image   Destination matrix.
     * @param timeout Ignored; the simulated frame is always the latest available.
     * @return A non-zero value on success, 0 if no frame was available.
     */
    @Override
    public long grabFrame(Mat image, double timeout) {
        return grabFrameNoTimeout(image);
    }

    /**
     * Grabs the most recent frame supplied by Synthesis.
     *
     * @param image Destination matrix.
     * @return A non-zero value on success, 0 if no frame was available.
     */
    @Override
    public long grabFrameNoTimeout(Mat image) {
        return m_camera.grabFrame(image) ? 1L : 0L;
    }
}

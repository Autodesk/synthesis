package com.autodesk.synthesis.cscore;

import org.opencv.core.Mat;

/**
 * UsbCamera wrapper to add WPILib HALSim camera support. Use this in place of
 * {@code edu.wpi.first.cscore.UsbCamera}; instead of a physical USB device, frames are
 * supplied by Synthesis, which renders the scene from the camera's configured pose on the
 * robot.
 *
 * Mirrors the swap-in pattern used for motors (see
 * {@code com.autodesk.synthesis.revrobotics.spark.SparkMax}).
 *
 * Typical usage:
 * <pre>
 *     var camera = new com.autodesk.synthesis.cscore.UsbCamera("USB Camera 0", 0);
 *     var sink = camera.getVideo();
 *     Mat frame = new Mat();
 *     if (sink.grabFrame(frame) != 0) {
 *         // ... run vision processing on frame ...
 *     }
 * </pre>
 *
 * See original documentation for more information
 * https://github.wpilib.org/allwpilib/docs/release/java/edu/wpi/first/cscore/UsbCamera.html
 */
public class UsbCamera extends edu.wpi.first.cscore.UsbCamera {

    private Camera m_camera;

    /**
     * Creates a new simulation-backed UsbCamera with a default 640x480 @ 30fps stream.
     *
     * @param name Name of the camera (matches the name configured in Synthesis).
     * @param dev  USB device index (matches the index configured in Synthesis).
     */
    public UsbCamera(String name, int dev) {
        this(name, dev, 640, 480, 30);
    }

    /**
     * Creates a new simulation-backed UsbCamera.
     *
     * @param name   Name of the camera (matches the name configured in Synthesis).
     * @param dev    USB device index (matches the index configured in Synthesis).
     * @param width  Requested frame width in pixels.
     * @param height Requested frame height in pixels.
     * @param fps    Requested frame rate.
     */
    public UsbCamera(String name, int dev, int width, int height, int fps) {
        super(name, dev);
        this.m_camera = new Camera(name, dev, width, height, fps);
    }

    /**
     * Gets the underlying Synthesis camera sim device.
     *
     * @return The backing {@link Camera}.
     */
    public Camera getCamera() {
        return this.m_camera;
    }

    /**
     * Gets a simulation-backed CvSink for grabbing frames from this camera.
     *
     * @return A {@link CvSink} that returns Synthesis-rendered frames.
     */
    public CvSink getVideo() {
        return new CvSink(this.getName() + " - sink", this.m_camera);
    }

    /**
     * Convenience method to grab the most recent frame directly from this camera.
     *
     * @param dst Destination matrix.
     * @return A non-zero value on success, 0 if no frame was available.
     */
    public long grabFrame(Mat dst) {
        return this.m_camera.grabFrame(dst) ? 1L : 0L;
    }

    /**
     * Sets the requested resolution on both the real and simulated camera.
     *
     * @param width  Frame width in pixels.
     * @param height Frame height in pixels.
     * @return true.
     */
    @Override
    public boolean setResolution(int width, int height) {
        this.m_camera.setResolution(width, height);
        return super.setResolution(width, height);
    }

    /**
     * Sets the requested frame rate on both the real and simulated camera.
     *
     * @param fps Frames per second.
     * @return true.
     */
    @Override
    public boolean setFPS(int fps) {
        this.m_camera.setFPS(fps);
        return super.setFPS(fps);
    }
}

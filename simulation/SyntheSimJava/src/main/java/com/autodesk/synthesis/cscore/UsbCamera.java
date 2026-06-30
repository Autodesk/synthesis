package com.autodesk.synthesis.cscore;

/**
 * swap-in for {@code edu.wpi.first.cscore.UsbCamera} (mirrors the SparkMax wrapper
 * pattern): frames come from Synthesis instead of a physical device.
 *
 * <pre>
 *     var camera = new com.autodesk.synthesis.cscore.UsbCamera("USB Camera 0", 0);
 *     var sink = camera.getVideo();
 *     Mat frame = new Mat();
 *     if (sink.grabFrame(frame) != 0) {
 *         // ... run vision processing on frame ...
 *     }
 * </pre>
 */
public class UsbCamera extends edu.wpi.first.cscore.UsbCamera {

    private Camera m_camera;

    public UsbCamera(String name, int dev) {
        this(name, dev, 640, 480, 30);
    }

    public UsbCamera(String name, int dev, int width, int height, int fps) {
        super(name, dev);
        this.m_camera = new Camera(name, dev, width, height, fps);
    }

    public CvSink getVideo() {
        return new CvSink(this.getName() + " - sink", this.m_camera);
    }

    @Override
    public boolean setResolution(int width, int height) {
        this.m_camera.setResolution(width, height);
        return super.setResolution(width, height);
    }

    @Override
    public boolean setFPS(int fps) {
        this.m_camera.setFPS(fps);
        return super.setFPS(fps);
    }
}

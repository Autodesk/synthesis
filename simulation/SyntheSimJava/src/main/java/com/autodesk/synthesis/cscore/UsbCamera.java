package com.autodesk.synthesis.cscore;

/**
 * Swap-in for {@code edu.wpi.first.cscore.UsbCamera}
 *
 * <pre>
 *     var camera = CameraServer.startAutomaticCapture();
 *     var sink = CameraServer.getVideo();
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

    Camera getCamera() {
        return this.m_camera;
    }

    public CvSink getVideo() {
        return CameraServer.getVideo(this);
    }

    @Override
    public boolean setResolution(int width, int height) {
        this.m_camera.setResolution(width, height);

        if (!this.m_camera.isSimulated()) {
            return super.setResolution(width, height);
        }

        return true;
    }

    @Override
    public boolean setFPS(int fps) {
        this.m_camera.setFPS(fps);
        return super.setFPS(fps);
    }
}

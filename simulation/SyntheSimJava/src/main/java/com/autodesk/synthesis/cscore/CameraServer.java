package com.autodesk.synthesis.cscore;

import edu.wpi.first.cscore.CameraServerJNI;
import edu.wpi.first.cscore.CvSource;
import edu.wpi.first.cscore.MjpegServer;
import edu.wpi.first.cscore.VideoException;
import edu.wpi.first.cscore.VideoMode;
import edu.wpi.first.cscore.VideoSink;
import edu.wpi.first.cscore.VideoSource;
import edu.wpi.first.networktables.NetworkTable;
import edu.wpi.first.networktables.NetworkTableInstance;
import edu.wpi.first.util.PixelFormat;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.atomic.AtomicInteger;
import org.opencv.core.Mat;

/**
 * Swap-in for {@code edu.wpi.first.cameraserver.CameraServer}; mirrors its API so user code only
 * changes imports. Implemented locally (no delegation to the WPILib class) because both keep
 * port counters starting at {@code kBasePort} (collision) and WPILib's global VideoListener
 * would publish conflicting NetworkTables entries for our sources.
 *
 * In simulation, USB camera frames come from Fission over {@link CameraFrameServer}; a pump
 * thread republishes them through a CvSource + MjpegServer so dashboards get a real MJPEG
 * stream. Stream URLs are published under {@code CameraPublisher/<name>/streams} with a
 * {@code 127.0.0.1} address, since CSCore's default mDNS hostname usually can't be resolved by
 * Shuffleboard/Glass when running in Synthesis.
 */
public final class CameraServer {
    public static final int kBasePort = 1181;

    private static final String kPublishName = "CameraPublisher";

    private static final AtomicInteger m_defaultUsbDevice = new AtomicInteger();
    private static String m_primarySourceName;
    private static final Map<String, VideoSource> m_sources = new HashMap<>();
    private static final Map<String, VideoSink> m_sinks = new HashMap<>();
    private static int m_nextPort = kBasePort;

    private CameraServer() {}

    /** @return backing sim Camera if {@code camera} is a simulated UsbCamera, else null */
    private static Camera simCamera(VideoSource camera) {
        if (!(camera instanceof UsbCamera)) {
            return null;
        }
        Camera c = ((UsbCamera) camera).getCamera();
        return c.isSimulated() ? c : null;
    }

    private static void publishCamera(String name, boolean usb, boolean sim, VideoMode mode, int port) {
        String address = sim ? "127.0.0.1" : CameraServerJNI.getHostname() + ".local";
        String modeString = mode.width + "x" + mode.height + " MJPEG " + mode.fps + " fps";
        NetworkTable table = NetworkTableInstance.getDefault().getTable(kPublishName).getSubTable(name);
        table.getEntry("source").setString(usb ? "usb:" + name : "cv:");
        table.getEntry("description").setString(name);
        table.getEntry("connected").setBoolean(true);
        table.getEntry("mode").setString(modeString);
        table.getEntry("modes").setStringArray(new String[] { modeString });
        table.getEntry("streams")
                .setStringArray(new String[] { "mjpg:http://" + address + ":" + port + "/?action=stream" });
    }

    /** Decode Fission websocket frames and push into a CvSource the MjpegServer can serve. */
    private static CvSource startFramePump(UsbCamera camera) {
        Camera simCamera = camera.getCamera();
        CvSource source = new CvSource(camera.getName() + "_synthesis", PixelFormat.kMJPEG,
                simCamera.getWidth(), simCamera.getHeight(), simCamera.getFPS());

        Thread pump = new Thread(() -> {
            Mat frame = new Mat();
            byte[] lastFrame = null;
            while (!Thread.interrupted()) {
                byte[] bytes = simCamera.getLatestFrame();
                if (bytes != null && bytes != lastFrame) {
                    lastFrame = bytes;
                    if (Camera.decodeFrame(bytes, frame)) {
                        source.putFrame(frame);
                    }
                }
                try {
                    Thread.sleep(1000 / Math.max(1, simCamera.getFPS()));
                } catch (InterruptedException e) {
                    return;
                }
            }
        }, "Synthesis camera pump: " + camera.getName());
        pump.setDaemon(true);
        pump.start();

        return source;
    }

    /**
     * Start automatically capturing images to send to the dashboard.
     *
     * <p>The first time this overload is called, it calls {@link #startAutomaticCapture(int)} with
     * device 0, creating a camera named "USB Camera 0". Subsequent calls increment the device
     * number (e.g. 1, 2, etc).
     *
     * @return The USB camera capturing images.
     */
    public static UsbCamera startAutomaticCapture() {
        return startAutomaticCapture(m_defaultUsbDevice.getAndIncrement());
    }

    /**
     * Start automatically capturing images to send to the dashboard.
     *
     * @param dev The device number of the camera interface
     * @return The USB camera capturing images.
     */
    public static UsbCamera startAutomaticCapture(int dev) {
        UsbCamera camera = new UsbCamera("USB Camera " + dev, dev);
        startAutomaticCapture(camera);
        return camera;
    }

    /**
     * Start automatically capturing images to send to the dashboard.
     *
     * @param name The name to give the camera
     * @param dev The device number of the camera interface
     * @return The USB camera capturing images.
     */
    public static UsbCamera startAutomaticCapture(String name, int dev) {
        UsbCamera camera = new UsbCamera(name, dev);
        startAutomaticCapture(camera);
        return camera;
    }

    // TODO: startAutomaticCapture(String name, String path)

    /**
     * Start automatically capturing images to send to the dashboard from an existing camera.
     *
     * @param camera Camera
     * @return The MJPEG server serving images from the given camera.
     */
    public static synchronized MjpegServer startAutomaticCapture(VideoSource camera) {
        addCamera(camera);
        MjpegServer server = addServer("serve_" + camera.getName());

        boolean usb = camera instanceof UsbCamera;
        boolean sim = simCamera(camera) != null;
        VideoMode mode;
        if (sim) {
            CvSource pumpSource = startFramePump((UsbCamera) camera);
            server.setSource(pumpSource);
            mode = pumpSource.getVideoMode();
        } else {
            server.setSource(camera);
            try {
                mode = camera.getVideoMode();
            } catch (VideoException e) {
                mode = new VideoMode(PixelFormat.kMJPEG, 0, 0, 0);
            }
        }

        publishCamera(camera.getName(), usb, sim, mode, server.getPort());
        return server;
    }

    /**
     * Adds a virtual camera for switching between two streams. Calling setSource() on the returned
     * object can be used to switch the actual source of the stream.
     *
     * @param name The name to give the camera
     * @return The MJPEG server serving images from the given camera.
     */
    public static MjpegServer addSwitchedCamera(String name) {
        CvSource source = new CvSource(name, PixelFormat.kMJPEG, 160, 120, 30);
        return startAutomaticCapture(source);
    }

    /**
     * Get OpenCV access to the primary camera feed. This allows you to get images from the camera
     * for image processing.
     *
     * <p>This is only valid to call after a camera feed has been added with
     * startAutomaticCapture() or addServer().
     *
     * @return OpenCV sink for the primary camera feed
     */
    public static CvSink getVideo() {
        VideoSource source;
        synchronized (CameraServer.class) {
            if (m_primarySourceName == null) {
                throw new VideoException("no camera available");
            }
            source = m_sources.get(m_primarySourceName);
        }
        if (source == null) {
            throw new VideoException("no camera available");
        }
        return getVideo(source);
    }

    /**
     * Get OpenCV access to the specified camera. This allows you to get images from the camera for
     * image processing.
     *
     * @param camera Camera (e.g. as returned by startAutomaticCapture).
     * @return OpenCV sink for the specified camera
     */
    public static CvSink getVideo(VideoSource camera) {
        String name = "opencv_" + camera.getName();

        synchronized (CameraServer.class) {
            VideoSink sink = m_sinks.get(name);
            if (sink != null) {
                if (!(sink instanceof CvSink)) {
                    throw new VideoException("expected OpenCV sink, but got " + sink.getKind());
                }
                return (CvSink) sink;
            }
        }

        CvSink newsink = new CvSink(name, simCamera(camera));
        newsink.setSource(camera);
        addServer(newsink);
        return newsink;
    }

    /**
     * Get OpenCV access to the specified camera. This allows you to get images from the camera for
     * image processing.
     *
     * @param name Camera name
     * @return OpenCV sink for the specified camera
     */
    public static CvSink getVideo(String name) {
        VideoSource source;
        synchronized (CameraServer.class) {
            source = m_sources.get(name);
        }
        if (source == null) {
            throw new VideoException("could not find camera " + name);
        }
        return getVideo(source);
    }

    /**
     * Create a MJPEG stream with OpenCV input. This can be called to pass custom annotated images
     * to the dashboard.
     *
     * @param name Name to give the stream
     * @param width Width of the image being sent
     * @param height Height of the image being sent
     * @return OpenCV source for the MJPEG stream
     */
    public static CvSource putVideo(String name, int width, int height) {
        CvSource source = new CvSource(name, PixelFormat.kMJPEG, width, height, 30);
        startAutomaticCapture(source);
        return source;
    }

    /**
     * Adds a MJPEG server at the next available port.
     *
     * @param name Server name
     * @return The MJPEG server
     */
    public static MjpegServer addServer(String name) {
        int port;
        synchronized (CameraServer.class) {
            port = m_nextPort;
            m_nextPort++;
        }
        return addServer(name, port);
    }

    /**
     * Adds a MJPEG server.
     *
     * @param name Server name
     * @param port Server port
     * @return The MJPEG server
     */
    public static MjpegServer addServer(String name, int port) {
        MjpegServer server = new MjpegServer(name, port);
        addServer(server);
        return server;
    }

    /**
     * Adds an already created server.
     *
     * @param server Server
     */
    public static void addServer(VideoSink server) {
        synchronized (CameraServer.class) {
            m_sinks.put(server.getName(), server);
        }
    }

    /**
     * Removes a server by name.
     *
     * @param name Server name
     */
    public static void removeServer(String name) {
        synchronized (CameraServer.class) {
            m_sinks.remove(name);
        }
    }

    /**
     * Get server for the primary camera feed.
     *
     * <p>This is only valid to call after a camera feed has been added with
     * startAutomaticCapture() or addServer().
     *
     * @return The server for the primary camera feed
     */
    public static VideoSink getServer() {
        synchronized (CameraServer.class) {
            if (m_primarySourceName == null) {
                throw new VideoException("no camera available");
            }
            return getServer("serve_" + m_primarySourceName);
        }
    }

    /**
     * Gets a server by name.
     *
     * @param name Server name
     * @return The server
     */
    public static VideoSink getServer(String name) {
        synchronized (CameraServer.class) {
            return m_sinks.get(name);
        }
    }

    /**
     * Adds an already created camera.
     *
     * @param camera Camera
     */
    public static void addCamera(VideoSource camera) {
        String name = camera.getName();
        synchronized (CameraServer.class) {
            if (m_primarySourceName == null) {
                m_primarySourceName = name;
            }
            m_sources.put(name, camera);
        }
    }

    /**
     * Removes a camera by name.
     *
     * @param name Camera name
     */
    public static void removeCamera(String name) {
        synchronized (CameraServer.class) {
            m_sources.remove(name);
        }
    }
}

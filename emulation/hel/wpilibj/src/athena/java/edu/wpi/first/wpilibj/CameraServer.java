/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.cscore.AxisCamera;
import edu.wpi.cscore.CameraServerJNI;
import edu.wpi.cscore.CvSink;
import edu.wpi.cscore.CvSource;
import edu.wpi.cscore.MjpegServer;
import edu.wpi.cscore.UsbCamera;
import edu.wpi.cscore.VideoEvent;
import edu.wpi.cscore.VideoException;
import edu.wpi.cscore.VideoListener;
import edu.wpi.cscore.VideoMode;
import edu.wpi.cscore.VideoMode.PixelFormat;
import edu.wpi.cscore.VideoProperty;
import edu.wpi.cscore.VideoSink;
import edu.wpi.cscore.VideoSource;
import edu.wpi.first.wpilibj.networktables.NetworkTable;
import edu.wpi.first.wpilibj.networktables.NetworkTablesJNI;
import edu.wpi.first.wpilibj.tables.ITable;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Singleton class for creating and keeping camera servers.
 * Also publishes camera information to NetworkTables.
 */
public class CameraServer {
  public static final int kBasePort = 1181;

  @Deprecated
  public static final int kSize640x480 = 0;
  @Deprecated
  public static final int kSize320x240 = 1;
  @Deprecated
  public static final int kSize160x120 = 2;

  private static final String kPublishName = "/CameraPublisher";
  private static CameraServer server;

  /**
   * Get the CameraServer instance.
   */
  public static synchronized CameraServer getInstance() {
    if (server == null) {
      server = new CameraServer();
    }
    return server;
  }

  private AtomicInteger m_defaultUsbDevice;
  private String m_primarySourceName;
  private final Hashtable<String, VideoSource> m_sources;
  private final Hashtable<String, VideoSink> m_sinks;
  private final Hashtable<Integer, ITable> m_tables;  // indexed by source handle
  private final ITable m_publishTable;
  private final VideoListener m_videoListener; //NOPMD
  private final int m_tableListener; //NOPMD
  private int m_nextPort;
  private String[] m_addresses;

  @SuppressWarnings("JavadocMethod")
  private static String makeSourceValue(int source) {
    switch (VideoSource.getKindFromInt(CameraServerJNI.getSourceKind(source))) {
      case kUsb:
        return "usb:" + CameraServerJNI.getUsbCameraPath(source);
      case kHttp: {
        String[] urls = CameraServerJNI.getHttpCameraUrls(source);
        if (urls.length > 0) {
          return "ip:" + urls[0];
        } else {
          return "ip:";
        }
      }
      case kCv:
        // FIXME: Should be "cv:", but LabVIEW dashboard requires "usb:".
        // https://github.com/wpilibsuite/allwpilib/issues/407
        return "usb:";
      default:
        return "unknown:";
    }
  }

  @SuppressWarnings("JavadocMethod")
  private static String makeStreamValue(String address, int port) {
    return "mjpg:http://" + address + ":" + port + "/?action=stream";
  }

  @SuppressWarnings({"JavadocMethod", "PMD.AvoidUsingHardCodedIP"})
  private synchronized String[] getSinkStreamValues(int sink) {
    // Ignore all but MjpegServer
    if (VideoSink.getKindFromInt(CameraServerJNI.getSinkKind(sink)) != VideoSink.Kind.kMjpeg) {
      return new String[0];
    }

    // Get port
    int port = CameraServerJNI.getMjpegServerPort(sink);

    // Generate values
    ArrayList<String> values = new ArrayList<String>(m_addresses.length + 1);
    String listenAddress = CameraServerJNI.getMjpegServerListenAddress(sink);
    if (!listenAddress.isEmpty()) {
      // If a listen address is specified, only use that
      values.add(makeStreamValue(listenAddress, port));
    } else {
      // Otherwise generate for hostname and all interface addresses
      values.add(makeStreamValue(CameraServerJNI.getHostname() + ".local", port));
      for (String addr : m_addresses) {
        if (addr.equals("127.0.0.1")) {
          continue;  // ignore localhost
        }
        values.add(makeStreamValue(addr, port));
      }
    }

    return values.toArray(new String[0]);
  }

  @SuppressWarnings({"JavadocMethod", "PMD.AvoidUsingHardCodedIP"})
  private synchronized String[] getSourceStreamValues(int source) {
    // Ignore all but HttpCamera
    if (VideoSource.getKindFromInt(CameraServerJNI.getSourceKind(source))
            != VideoSource.Kind.kHttp) {
      return new String[0];
    }

    // Generate values
    String[] values = CameraServerJNI.getHttpCameraUrls(source);
    for (int j = 0; j < values.length; j++) {
      values[j] = "mjpg:" + values[j];
    }

    // Look to see if we have a passthrough server for this source
    for (VideoSink i : m_sinks.values()) {
      int sink = i.getHandle();
      int sinkSource = CameraServerJNI.getSinkSource(sink);
      if (source == sinkSource
          && VideoSink.getKindFromInt(CameraServerJNI.getSinkKind(sink)) == VideoSink.Kind.kMjpeg) {
        // Add USB-only passthrough
        String[] finalValues = new String[values.length + 1];
        for (int j = 0; j < values.length; j++) {
          finalValues[j] = values[j];
        }
        int port = CameraServerJNI.getMjpegServerPort(sink);
        finalValues[values.length] = makeStreamValue("172.22.11.2", port);
        return finalValues;
      }
    }

    return values;
  }

  @SuppressWarnings({"JavadocMethod", "PMD.AvoidUsingHardCodedIP"})
  private synchronized void updateStreamValues() {
    // Over all the sinks...
    for (VideoSink i : m_sinks.values()) {
      int sink = i.getHandle();

      // Get the source's subtable (if none exists, we're done)
      int source = CameraServerJNI.getSinkSource(sink);
      if (source == 0) {
        continue;
      }
      ITable table = m_tables.get(source);
      if (table != null) {
        // Don't set stream values if this is a HttpCamera passthrough
        if (VideoSource.getKindFromInt(CameraServerJNI.getSourceKind(source))
            == VideoSource.Kind.kHttp) {
          continue;
        }

        // Set table value
        String[] values = getSinkStreamValues(sink);
        if (values.length > 0) {
          table.putStringArray("streams", values);
        }
      }
    }

    // Over all the sources...
    for (VideoSource i : m_sources.values()) {
      int source = i.getHandle();

      // Get the source's subtable (if none exists, we're done)
      ITable table = m_tables.get(source);
      if (table != null) {
        // Set table value
        String[] values = getSourceStreamValues(source);
        if (values.length > 0) {
          table.putStringArray("streams", values);
        }
      }
    }
  }

  @SuppressWarnings("JavadocMethod")
  private static String pixelFormatToString(PixelFormat pixelFormat) {
    switch (pixelFormat) {
      case kMJPEG:
        return "MJPEG";
      case kYUYV:
        return "YUYV";
      case kRGB565:
        return "RGB565";
      case kBGR:
        return "BGR";
      case kGray:
        return "Gray";
      default:
        return "Unknown";
    }
  }

  @SuppressWarnings("JavadocMethod")
  private static PixelFormat pixelFormatFromString(String pixelFormatStr) {
    switch (pixelFormatStr) {
      case "MJPEG":
      case "mjpeg":
      case "JPEG":
      case "jpeg":
        return PixelFormat.kMJPEG;
      case "YUYV":
      case "yuyv":
        return PixelFormat.kYUYV;
      case "RGB565":
      case "rgb565":
        return PixelFormat.kRGB565;
      case "BGR":
      case "bgr":
        return PixelFormat.kBGR;
      case "GRAY":
      case "Gray":
      case "gray":
        return PixelFormat.kGray;
      default:
        return PixelFormat.kUnknown;
    }
  }

  private static final Pattern reMode =
      Pattern.compile("(?<width>[0-9]+)\\s*x\\s*(?<height>[0-9]+)\\s+(?<format>.*?)\\s+"
          + "(?<fps>[0-9.]+)\\s*fps");

  /// Construct a video mode from a string description.
  @SuppressWarnings("JavadocMethod")
  private static VideoMode videoModeFromString(String modeStr) {
    Matcher matcher = reMode.matcher(modeStr);
    if (!matcher.matches()) {
      return new VideoMode(PixelFormat.kUnknown, 0, 0, 0);
    }
    PixelFormat pixelFormat = pixelFormatFromString(matcher.group("format"));
    int width = Integer.parseInt(matcher.group("width"));
    int height = Integer.parseInt(matcher.group("height"));
    int fps = (int) Double.parseDouble(matcher.group("fps"));
    return new VideoMode(pixelFormat, width, height, fps);
  }

  /// Provide string description of video mode.
  /// The returned string is "{width}x{height} {format} {fps} fps".
  @SuppressWarnings("JavadocMethod")
  private static String videoModeToString(VideoMode mode) {
    return mode.width + "x" + mode.height + " " + pixelFormatToString(mode.pixelFormat)
        + " " + mode.fps + " fps";
  }

  @SuppressWarnings("JavadocMethod")
  private static String[] getSourceModeValues(int sourceHandle) {
    VideoMode[] modes = CameraServerJNI.enumerateSourceVideoModes(sourceHandle);
    String[] modeStrings = new String[modes.length];
    for (int i = 0; i < modes.length; i++) {
      modeStrings[i] = videoModeToString(modes[i]);
    }
    return modeStrings;
  }

  @SuppressWarnings("JavadocMethod")
  private static void putSourcePropertyValue(ITable table, VideoEvent event, boolean isNew) {
    String name;
    String infoName;
    if (event.name.startsWith("raw_")) {
      name = "RawProperty/" + event.name;
      infoName = "RawPropertyInfo/" + event.name;
    } else {
      name = "Property/" + event.name;
      infoName = "PropertyInfo/" + event.name;
    }

    switch (event.propertyKind) {
      case kBoolean:
        if (isNew) {
          table.setDefaultBoolean(name, event.value != 0);
        } else {
          table.putBoolean(name, event.value != 0);
        }
        break;
      case kInteger:
      case kEnum:
        if (isNew) {
          table.setDefaultNumber(name, event.value);
          table.putNumber(infoName + "/min",
              CameraServerJNI.getPropertyMin(event.propertyHandle));
          table.putNumber(infoName + "/max",
              CameraServerJNI.getPropertyMax(event.propertyHandle));
          table.putNumber(infoName + "/step",
              CameraServerJNI.getPropertyStep(event.propertyHandle));
          table.putNumber(infoName + "/default",
              CameraServerJNI.getPropertyDefault(event.propertyHandle));
        } else {
          table.putNumber(name, event.value);
        }
        break;
      case kString:
        if (isNew) {
          table.setDefaultString(name, event.valueStr);
        } else {
          table.putString(name, event.valueStr);
        }
        break;
      default:
        break;
    }
  }

  @SuppressWarnings({"JavadocMethod", "PMD.UnusedLocalVariable"})
  private CameraServer() {
    m_defaultUsbDevice = new AtomicInteger();
    m_sources = new Hashtable<String, VideoSource>();
    m_sinks = new Hashtable<String, VideoSink>();
    m_tables = new Hashtable<Integer, ITable>();
    m_publishTable = NetworkTable.getTable(kPublishName);
    m_nextPort = kBasePort;
    m_addresses = new String[0];

    // We publish sources to NetworkTables using the following structure:
    // "/CameraPublisher/{Source.Name}/" - root
    // - "source" (string): Descriptive, prefixed with type (e.g. "usb:0")
    // - "streams" (string array): URLs that can be used to stream data
    // - "description" (string): Description of the source
    // - "connected" (boolean): Whether source is connected
    // - "mode" (string): Current video mode
    // - "modes" (string array): Available video modes
    // - "Property/{Property}" - Property values
    // - "PropertyInfo/{Property}" - Property supporting information

    // Listener for video events
    m_videoListener = new VideoListener(event -> {
      switch (event.kind) {
        case kSourceCreated: {
          // Create subtable for the camera
          ITable table = m_publishTable.getSubTable(event.name);
          m_tables.put(event.sourceHandle, table);
          table.putString("source", makeSourceValue(event.sourceHandle));
          table.putString("description",
              CameraServerJNI.getSourceDescription(event.sourceHandle));
          table.putBoolean("connected", CameraServerJNI.isSourceConnected(event.sourceHandle));
          table.putStringArray("streams", getSourceStreamValues(event.sourceHandle));
          try {
            VideoMode mode = CameraServerJNI.getSourceVideoMode(event.sourceHandle);
            table.setDefaultString("mode", videoModeToString(mode));
            table.putStringArray("modes", getSourceModeValues(event.sourceHandle));
          } catch (VideoException ex) {
            // Do nothing. Let the other event handlers update this if there is an error.
          }
          break;
        }
        case kSourceDestroyed: {
          ITable table = m_tables.get(event.sourceHandle);
          if (table != null) {
            table.putString("source", "");
            table.putStringArray("streams", new String[0]);
            table.putStringArray("modes", new String[0]);
          }
          break;
        }
        case kSourceConnected: {
          ITable table = m_tables.get(event.sourceHandle);
          if (table != null) {
            // update the description too (as it may have changed)
            table.putString("description",
                CameraServerJNI.getSourceDescription(event.sourceHandle));
            table.putBoolean("connected", true);
          }
          break;
        }
        case kSourceDisconnected: {
          ITable table = m_tables.get(event.sourceHandle);
          if (table != null) {
            table.putBoolean("connected", false);
          }
          break;
        }
        case kSourceVideoModesUpdated: {
          ITable table = m_tables.get(event.sourceHandle);
          if (table != null) {
            table.putStringArray("modes", getSourceModeValues(event.sourceHandle));
          }
          break;
        }
        case kSourceVideoModeChanged: {
          ITable table = m_tables.get(event.sourceHandle);
          if (table != null) {
            table.putString("mode", videoModeToString(event.mode));
          }
          break;
        }
        case kSourcePropertyCreated: {
          ITable table = m_tables.get(event.sourceHandle);
          if (table != null) {
            putSourcePropertyValue(table, event, true);
          }
          break;
        }
        case kSourcePropertyValueUpdated: {
          ITable table = m_tables.get(event.sourceHandle);
          if (table != null) {
            putSourcePropertyValue(table, event, false);
          }
          break;
        }
        case kSourcePropertyChoicesUpdated: {
          ITable table = m_tables.get(event.sourceHandle);
          if (table != null) {
            String[] choices = CameraServerJNI.getEnumPropertyChoices(event.propertyHandle);
            table.putStringArray("PropertyInfo/" + event.name + "/choices", choices);
          }
          break;
        }
        case kSinkSourceChanged:
        case kSinkCreated:
        case kSinkDestroyed:
        case kNetworkInterfacesChanged: {
          m_addresses = CameraServerJNI.getNetworkInterfaces();
          updateStreamValues();
          break;
        }
        default:
          break;
      }
    }, 0x4fff, true);

    // Listener for NetworkTable events
    // We don't currently support changing settings via NT due to
    // synchronization issues, so just update to current setting if someone
    // else tries to change it.
    m_tableListener = NetworkTablesJNI.addEntryListener(kPublishName + "/",
      (uid, key, eventValue, flags) -> {
        String relativeKey = key.substring(kPublishName.length() + 1);

        // get source (sourceName/...)
        int subKeyIndex = relativeKey.indexOf('/');
        if (subKeyIndex == -1) {
          return;
        }
        String sourceName = relativeKey.substring(0, subKeyIndex);
        VideoSource source = m_sources.get(sourceName);
        if (source == null) {
          return;
        }

        // get subkey
        relativeKey = relativeKey.substring(subKeyIndex + 1);

        // handle standard names
        String propName;
        if (relativeKey.equals("mode")) {
          // reset to current mode
          NetworkTablesJNI.putString(key, videoModeToString(source.getVideoMode()));
          return;
        } else if (relativeKey.startsWith("Property/")) {
          propName = relativeKey.substring(9);
        } else if (relativeKey.startsWith("RawProperty/")) {
          propName = relativeKey.substring(12);
        } else {
          return;  // ignore
        }

        // everything else is a property
        VideoProperty property = source.getProperty(propName);
        switch (property.getKind()) {
          case kNone:
            return;
          case kBoolean:
            // reset to current setting
            NetworkTablesJNI.putBoolean(key, property.get() != 0);
            return;
          case kInteger:
          case kEnum:
            // reset to current setting
            NetworkTablesJNI.putDouble(key, property.get());
            return;
          case kString:
            // reset to current setting
            NetworkTablesJNI.putString(key, property.getString());
            return;
          default:
            return;
        }
      }, ITable.NOTIFY_IMMEDIATE | ITable.NOTIFY_UPDATE);
  }

  /**
   * Start automatically capturing images to send to the dashboard.
   *
   * <p>You should call this method to see a camera feed on the dashboard.
   * If you also want to perform vision processing on the roboRIO, use
   * getVideo() to get access to the camera images.
   *
   * <p>The first time this overload is called, it calls
   * {@link #startAutomaticCapture(int)} with device 0, creating a camera
   * named "USB Camera 0".  Subsequent calls increment the device number
   * (e.g. 1, 2, etc).
   */
  public UsbCamera startAutomaticCapture() {
    return startAutomaticCapture(m_defaultUsbDevice.getAndIncrement());
  }

  /**
   * Start automatically capturing images to send to the dashboard.
   *
   * <p>This overload calls {@link #startAutomaticCapture(String, int)} with
   * a name of "USB Camera {dev}".
   *
   * @param dev The device number of the camera interface
   */
  public UsbCamera startAutomaticCapture(int dev) {
    UsbCamera camera = new UsbCamera("USB Camera " + dev, dev);
    startAutomaticCapture(camera);
    return camera;
  }

  /**
   * Start automatically capturing images to send to the dashboard.
   *
   * @param name The name to give the camera
   * @param dev The device number of the camera interface
   */
  public UsbCamera startAutomaticCapture(String name, int dev) {
    UsbCamera camera = new UsbCamera(name, dev);
    startAutomaticCapture(camera);
    return camera;
  }

  /**
   * Start automatically capturing images to send to the dashboard.
   *
   * @param name The name to give the camera
   * @param path The device path (e.g. "/dev/video0") of the camera
   */
  public UsbCamera startAutomaticCapture(String name, String path) {
    UsbCamera camera = new UsbCamera(name, path);
    startAutomaticCapture(camera);
    return camera;
  }

  /**
   * Start automatically capturing images to send to the dashboard from
   * an existing camera.
   *
   * @param camera Camera
   */
  public void startAutomaticCapture(VideoSource camera) {
    addCamera(camera);
    VideoSink server = addServer("serve_" + camera.getName());
    server.setSource(camera);
  }

  /**
   * Adds an Axis IP camera.
   *
   * <p>This overload calls {@link #addAxisCamera(String, String)} with
   * name "Axis Camera".
   *
   * @param host Camera host IP or DNS name (e.g. "10.x.y.11")
   */
  public AxisCamera addAxisCamera(String host) {
    return addAxisCamera("Axis Camera", host);
  }

  /**
   * Adds an Axis IP camera.
   *
   * <p>This overload calls {@link #addAxisCamera(String, String[])} with
   * name "Axis Camera".
   *
   * @param hosts Array of Camera host IPs/DNS names
   */
  public AxisCamera addAxisCamera(String[] hosts) {
    return addAxisCamera("Axis Camera", hosts);
  }

  /**
   * Adds an Axis IP camera.
   *
   * @param name The name to give the camera
   * @param host Camera host IP or DNS name (e.g. "10.x.y.11")
   */
  public AxisCamera addAxisCamera(String name, String host) {
    AxisCamera camera = new AxisCamera(name, host);
    // Create a passthrough MJPEG server for USB access
    startAutomaticCapture(camera);
    return camera;
  }

  /**
   * Adds an Axis IP camera.
   *
   * @param name The name to give the camera
   * @param hosts Array of Camera host IPs/DNS names
   */
  public AxisCamera addAxisCamera(String name, String[] hosts) {
    AxisCamera camera = new AxisCamera(name, hosts);
    // Create a passthrough MJPEG server for USB access
    startAutomaticCapture(camera);
    return camera;
  }

  /**
   * Get OpenCV access to the primary camera feed.  This allows you to
   * get images from the camera for image processing on the roboRIO.
   *
   * <p>This is only valid to call after a camera feed has been added
   * with startAutomaticCapture() or addServer().
   */
  public CvSink getVideo() {
    VideoSource source;
    synchronized (this) {
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
   * Get OpenCV access to the specified camera.  This allows you to get
   * images from the camera for image processing on the roboRIO.
   *
   * @param camera Camera (e.g. as returned by startAutomaticCapture).
   */
  public CvSink getVideo(VideoSource camera) {
    String name = "opencv_" + camera.getName();

    synchronized (this) {
      VideoSink sink = m_sinks.get(name);
      if (sink != null) {
        VideoSink.Kind kind = sink.getKind();
        if (kind != VideoSink.Kind.kCv) {
          throw new VideoException("expected OpenCV sink, but got " + kind);
        }
        return (CvSink) sink;
      }
    }

    CvSink newsink = new CvSink(name);
    newsink.setSource(camera);
    addServer(newsink);
    return newsink;
  }

  /**
   * Get OpenCV access to the specified camera.  This allows you to get
   * images from the camera for image processing on the roboRIO.
   *
   * @param name Camera name
   */
  public CvSink getVideo(String name) {
    VideoSource source;
    synchronized (this) {
      source = m_sources.get(name);
      if (source == null) {
        throw new VideoException("could not find camera " + name);
      }
    }
    return getVideo(source);
  }

  /**
   * Create a MJPEG stream with OpenCV input. This can be called to pass custom
   * annotated images to the dashboard.
   *
   * @param name Name to give the stream
   * @param width Width of the image being sent
   * @param height Height of the image being sent
   */
  public CvSource putVideo(String name, int width, int height) {
    CvSource source = new CvSource(name, VideoMode.PixelFormat.kMJPEG, width, height, 30);
    startAutomaticCapture(source);
    return source;
  }

  /**
   * Adds a MJPEG server at the next available port.
   *
   * @param name Server name
   */
  public MjpegServer addServer(String name) {
    int port;
    synchronized (this) {
      port = m_nextPort;
      m_nextPort++;
    }
    return addServer(name, port);
  }

  /**
   * Adds a MJPEG server.
   *
   * @param name Server name
   */
  public MjpegServer addServer(String name, int port) {
    MjpegServer server = new MjpegServer(name, port);
    addServer(server);
    return server;
  }

  /**
   * Adds an already created server.
   *
   * @param server Server
   */
  public void addServer(VideoSink server) {
    synchronized (this) {
      m_sinks.put(server.getName(), server);
    }
  }

  /**
   * Removes a server by name.
   *
   * @param name Server name
   */
  public void removeServer(String name) {
    synchronized (this) {
      m_sinks.remove(name);
    }
  }

  /**
   * Get server for the primary camera feed.
   *
   * <p>This is only valid to call after a camera feed has been added
   * with startAutomaticCapture() or addServer().
   */
  public VideoSink getServer() {
    synchronized (this) {
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
   */
  public VideoSink getServer(String name) {
    synchronized (this) {
      return m_sinks.get(name);
    }
  }

  /**
   * Adds an already created camera.
   *
   * @param camera Camera
   */
  public void addCamera(VideoSource camera) {
    String name = camera.getName();
    synchronized (this) {
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
  public void removeCamera(String name) {
    synchronized (this) {
      m_sources.remove(name);
    }
  }

  /**
   * Sets the size of the image to use. Use the public kSize constants to set the correct mode, or
   * set it directly on a camera and call the appropriate startAutomaticCapture method.
   *
   * @deprecated Use setResolution on the UsbCamera returned by startAutomaticCapture() instead.
   * @param size The size to use
   */
  @Deprecated
  public void setSize(int size) {
    VideoSource source = null;
    synchronized (this) {
      if (m_primarySourceName == null) {
        return;
      }
      source = m_sources.get(m_primarySourceName);
      if (source == null) {
        return;
      }
    }
    switch (size) {
      case kSize640x480:
        source.setResolution(640, 480);
        break;
      case kSize320x240:
        source.setResolution(320, 240);
        break;
      case kSize160x120:
        source.setResolution(160, 120);
        break;
      default:
        throw new IllegalArgumentException("Unsupported size: " + size);
    }
  }
}

/*----------------------------------------------------------------------------*/
/* Copyright (c) 2014-2017 FIRST. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include <stdint.h>

#include <atomic>
#include <memory>
#include <mutex>
#include <string>
#include <vector>

#include <llvm/DenseMap.h>
#include <llvm/StringMap.h>
#include <llvm/StringRef.h>

#include "ErrorBase.h"
#include "cscore.h"
#include "networktables/NetworkTable.h"

namespace frc {

/**
 * Singleton class for creating and keeping camera servers.
 * Also publishes camera information to NetworkTables.
 */
class CameraServer : public ErrorBase {
 public:
  static constexpr uint16_t kBasePort = 1181;
  static constexpr int kSize640x480 = 0;
  static constexpr int kSize320x240 = 1;
  static constexpr int kSize160x120 = 2;

  /**
   * Get the CameraServer instance.
   */
  static CameraServer* GetInstance();

#ifdef __linux__
  // USBCamera does not work on anything except linux
  /**
   * Start automatically capturing images to send to the dashboard.
   *
   * <p>You should call this method to see a camera feed on the dashboard.
   * If you also want to perform vision processing on the roboRIO, use
   * getVideo() to get access to the camera images.
   *
   * The first time this overload is called, it calls
   * {@link #StartAutomaticCapture(int)} with device 0, creating a camera
   * named "USB Camera 0".  Subsequent calls increment the device number
   * (e.g. 1, 2, etc).
   */
  cs::UsbCamera StartAutomaticCapture();

  /**
   * Start automatically capturing images to send to the dashboard.
   *
   * <p>This overload calls {@link #StartAutomaticCapture(String, int)} with
   * a name of "USB Camera {dev}".
   *
   * @param dev The device number of the camera interface
   */
  cs::UsbCamera StartAutomaticCapture(int dev);

  /**
   * Start automatically capturing images to send to the dashboard.
   *
   * @param name The name to give the camera
   * @param dev The device number of the camera interface
   */
  cs::UsbCamera StartAutomaticCapture(llvm::StringRef name, int dev);

  /**
   * Start automatically capturing images to send to the dashboard.
   *
   * @param name The name to give the camera
   * @param path The device path (e.g. "/dev/video0") of the camera
   */
  cs::UsbCamera StartAutomaticCapture(llvm::StringRef name,
                                      llvm::StringRef path);
#endif

  /**
   * Start automatically capturing images to send to the dashboard from
   * an existing camera.
   *
   * @param camera Camera
   */
  void StartAutomaticCapture(const cs::VideoSource& camera);

  /**
   * Adds an Axis IP camera.
   *
   * <p>This overload calls {@link #AddAxisCamera(String, String)} with
   * name "Axis Camera".
   *
   * @param host Camera host IP or DNS name (e.g. "10.x.y.11")
   */
  cs::AxisCamera AddAxisCamera(llvm::StringRef host);

  /**
   * Adds an Axis IP camera.
   *
   * <p>This overload calls {@link #AddAxisCamera(String, String)} with
   * name "Axis Camera".
   *
   * @param host Camera host IP or DNS name (e.g. "10.x.y.11")
   */
  cs::AxisCamera AddAxisCamera(const char* host);

  /**
   * Adds an Axis IP camera.
   *
   * <p>This overload calls {@link #AddAxisCamera(String, String)} with
   * name "Axis Camera".
   *
   * @param host Camera host IP or DNS name (e.g. "10.x.y.11")
   */
  cs::AxisCamera AddAxisCamera(const std::string& host);

  /**
   * Adds an Axis IP camera.
   *
   * <p>This overload calls {@link #AddAxisCamera(String, String[])} with
   * name "Axis Camera".
   *
   * @param hosts Array of Camera host IPs/DNS names
   */
  cs::AxisCamera AddAxisCamera(llvm::ArrayRef<std::string> hosts);

  /**
   * Adds an Axis IP camera.
   *
   * <p>This overload calls {@link #AddAxisCamera(String, String[])} with
   * name "Axis Camera".
   *
   * @param hosts Array of Camera host IPs/DNS names
   */
  template <typename T>
  cs::AxisCamera AddAxisCamera(std::initializer_list<T> hosts);

  /**
   * Adds an Axis IP camera.
   *
   * @param name The name to give the camera
   * @param host Camera host IP or DNS name (e.g. "10.x.y.11")
   */
  cs::AxisCamera AddAxisCamera(llvm::StringRef name, llvm::StringRef host);

  /**
   * Adds an Axis IP camera.
   *
   * @param name The name to give the camera
   * @param host Camera host IP or DNS name (e.g. "10.x.y.11")
   */
  cs::AxisCamera AddAxisCamera(llvm::StringRef name, const char* host);

  /**
   * Adds an Axis IP camera.
   *
   * @param name The name to give the camera
   * @param host Camera host IP or DNS name (e.g. "10.x.y.11")
   */
  cs::AxisCamera AddAxisCamera(llvm::StringRef name, const std::string& host);

  /**
   * Adds an Axis IP camera.
   *
   * @param name The name to give the camera
   * @param hosts Array of Camera host IPs/DNS names
   */
  cs::AxisCamera AddAxisCamera(llvm::StringRef name,
                               llvm::ArrayRef<std::string> hosts);

  /**
   * Adds an Axis IP camera.
   *
   * @param name The name to give the camera
   * @param hosts Array of Camera host IPs/DNS names
   */
  template <typename T>
  cs::AxisCamera AddAxisCamera(llvm::StringRef name,
                               std::initializer_list<T> hosts);

  /**
   * Get OpenCV access to the primary camera feed.  This allows you to
   * get images from the camera for image processing on the roboRIO.
   *
   * <p>This is only valid to call after a camera feed has been added
   * with startAutomaticCapture() or addServer().
   */
  cs::CvSink GetVideo();

  /**
   * Get OpenCV access to the specified camera.  This allows you to get
   * images from the camera for image processing on the roboRIO.
   *
   * @param camera Camera (e.g. as returned by startAutomaticCapture).
   */
  cs::CvSink GetVideo(const cs::VideoSource& camera);

  /**
   * Get OpenCV access to the specified camera.  This allows you to get
   * images from the camera for image processing on the roboRIO.
   *
   * @param name Camera name
   */
  cs::CvSink GetVideo(llvm::StringRef name);

  /**
   * Create a MJPEG stream with OpenCV input. This can be called to pass custom
   * annotated images to the dashboard.
   *
   * @param name Name to give the stream
   * @param width Width of the image being sent
   * @param height Height of the image being sent
   */
  cs::CvSource PutVideo(llvm::StringRef name, int width, int height);

  /**
   * Adds a MJPEG server at the next available port.
   *
   * @param name Server name
   */
  cs::MjpegServer AddServer(llvm::StringRef name);

  /**
   * Adds a MJPEG server.
   *
   * @param name Server name
   */
  cs::MjpegServer AddServer(llvm::StringRef name, int port);

  /**
   * Adds an already created server.
   *
   * @param server Server
   */
  void AddServer(const cs::VideoSink& server);

  /**
   * Removes a server by name.
   *
   * @param name Server name
   */
  void RemoveServer(llvm::StringRef name);

  /**
   * Get server for the primary camera feed.
   *
   * <p>This is only valid to call after a camera feed has been added
   * with StartAutomaticCapture() or AddServer().
   */
  cs::VideoSink GetServer();

  /**
   * Gets a server by name.
   *
   * @param name Server name
   */
  cs::VideoSink GetServer(llvm::StringRef name);

  /**
   * Adds an already created camera.
   *
   * @param camera Camera
   */
  void AddCamera(const cs::VideoSource& camera);

  /**
   * Removes a camera by name.
   *
   * @param name Camera name
   */
  void RemoveCamera(llvm::StringRef name);

  /**
   * Sets the size of the image to use. Use the public kSize constants to set
   * the correct mode, or set it directly on a camera and call the appropriate
   * StartAutomaticCapture method.
   *
   * @deprecated Use SetResolution on the UsbCamera returned by
   *     StartAutomaticCapture() instead.
   * @param size The size to use
   */
  void SetSize(int size);

 private:
  CameraServer();

  std::shared_ptr<ITable> GetSourceTable(CS_Source source);
  std::vector<std::string> GetSinkStreamValues(CS_Sink sink);
  std::vector<std::string> GetSourceStreamValues(CS_Source source);
  void UpdateStreamValues();

  static constexpr char const* kPublishName = "/CameraPublisher";

  std::mutex m_mutex;
  std::atomic<int> m_defaultUsbDevice;
  std::string m_primarySourceName;
  llvm::StringMap<cs::VideoSource> m_sources;
  llvm::StringMap<cs::VideoSink> m_sinks;
  llvm::DenseMap<CS_Source, std::shared_ptr<ITable>> m_tables;
  std::shared_ptr<NetworkTable> m_publishTable;
  cs::VideoListener m_videoListener;
  int m_tableListener;
  int m_nextPort;
  std::vector<std::string> m_addresses;
};

}  // namespace frc

#include "CameraServer.inc"

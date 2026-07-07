package com.autodesk.synthesis.cscore;

import edu.wpi.first.cscore.CvSource;
import edu.wpi.first.cscore.MjpegServer;
import edu.wpi.first.cscore.VideoSink;
import edu.wpi.first.networktables.NetworkTableInstance;

/**
 * Swap-in for {@code edu.wpi.first.cameraserver.CameraServer}: creates the MJPEG output exactly
 * like WPILib, then republishes a loopback stream URL.
 *
 * CSCore advertises the stream to dashboards under the host's mDNS name (e.g.
 * {@code "<host>.local"}). Running in Synthesis, that name usually can't be
 * resolved by Shuffleboard/Glass, so the camera appears but the feed never loads. Overriding
 * the {@code CameraPublisher/<name>/streams} entry with a {@code 127.0.0.1} URL points the
 * dashboard at the server actually running on the local machine.
 */
public class CameraServer {

    private CameraServer() {}

    public static CvSource putVideo(String name, int width, int height) {
        CvSource source = edu.wpi.first.cameraserver.CameraServer.putVideo(name, width, height);

        try {
            VideoSink server = edu.wpi.first.cameraserver.CameraServer.getServer("serve_" + name);
            if (server instanceof MjpegServer) {
                int port = ((MjpegServer) server).getPort();
                NetworkTableInstance.getDefault()
                        .getTable("CameraPublisher")
                        .getSubTable(name)
                        .getEntry("streams")
                        .setStringArray(new String[] { "mjpg:http://127.0.0.1:" + port + "/?action=stream" });
            }
        } catch (Exception e) {
            System.err.println("Could not override camera stream URL for '" + name + "': " + e.getMessage());
        }

        return source;
    }
}

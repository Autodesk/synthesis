package javax.microedition.io;

import java.io.File;
import java.io.IOException;
import java.net.URL;

public class Connector {
	public static final int READ = 1;
	public static final int WRITE = 2;
	public static final int ALL_MODES = READ | WRITE;

	public static Connection open(String spec) throws IOException {
		return open(spec, ALL_MODES);
	}

	public static Connection open(String spec, int mode) throws IOException {
		String protocol = spec.substring(0, spec.indexOf("://"));
		String path = spec.substring(spec.indexOf("://") + 3);
		if (protocol.equalsIgnoreCase("serversocket")) {
			int colon = path.indexOf(':');
			String host = colon >= 0 ? path.substring(0, colon) : "";
			String port = colon >= 0 ? path.substring(colon + 1) : "fail";
			return new ServerSocketConnection(host, Integer.valueOf(port));
		} else if (protocol.equalsIgnoreCase("file")) {
			return new FileConnection(new File(path));
		}
		throw new RuntimeException("Unsupported Protocol: " + protocol);
	}
}

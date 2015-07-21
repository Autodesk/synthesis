package javax.microedition.io;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.Socket;

public class SocketConnection {
	private final Socket socket;

	public SocketConnection(final Socket sock) {
		this.socket = sock;
	}

	public OutputStream openOutputStream() throws IOException {
		return socket.getOutputStream();
	}

	public InputStream openInputStream() throws IOException {
		return socket.getInputStream();
	}

	public void close() throws IOException {
		socket.close();
	}
}

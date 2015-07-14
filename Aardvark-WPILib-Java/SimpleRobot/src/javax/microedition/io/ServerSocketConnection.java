package javax.microedition.io;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.ServerSocket;

public class ServerSocketConnection extends Connection {
	private ServerSocket serverSocket;

	public ServerSocketConnection(String host, int port) throws IOException {
		serverSocket = new ServerSocket(port);
	}

	public SocketConnection acceptAndOpen() throws IOException {
		return new SocketConnection(serverSocket.accept());
	}

	@Override
	public InputStream openInputStream() throws IOException {
		throw new RuntimeException("Not implemented");
	}

	@Override
	public OutputStream openOutputStream() throws IOException {
		throw new RuntimeException("Not implemented");
	}

	@Override
	public void close() throws IOException {
		serverSocket.close();
	}
}

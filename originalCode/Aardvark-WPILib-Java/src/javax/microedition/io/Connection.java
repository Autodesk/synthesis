package javax.microedition.io;

import java.io.DataInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

public abstract class Connection {
	public abstract InputStream openInputStream() throws IOException;

	public abstract OutputStream openOutputStream() throws IOException;

	public DataInputStream openDataInputStream() throws IOException {
		return new DataInputStream(openInputStream());
	}

	public abstract void close() throws IOException;
}

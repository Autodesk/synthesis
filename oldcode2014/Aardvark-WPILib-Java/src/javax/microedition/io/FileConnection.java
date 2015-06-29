package javax.microedition.io;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

public class FileConnection extends Connection {
	private final File file;
	private InputStream inStream = null;
	private OutputStream outStream = null;

	public FileConnection(File file) {
		this.file = file;
	}

	@Override
	public InputStream openInputStream() throws IOException {
		if (inStream == null)
			inStream = new FileInputStream(file);
		return inStream;
	}

	@Override
	public OutputStream openOutputStream() throws IOException {
		if (outStream == null)
			outStream = new FileOutputStream(file);
		return outStream;
	}

	@Override
	public void close() throws IOException {
		if (outStream != null)
			outStream.close();
		if (inStream != null)
			inStream.close();
	}

	public boolean exists() {
		return file.exists();
	}
	
	public boolean isOpen() {
		return inStream != null || outStream != null;
	}
}

package main.java.com.osipion.mailTest;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.URLConnection;

import javax.mail.Part;
import javax.mail.util.ByteArrayDataSource;

public class EmbeddedTestAttachment extends TestAttachment {

	public EmbeddedTestAttachment(final String name, final String description, final InputStream content)
			throws IOException {

		super(Part.INLINE, name, description);

		byte[] contentBytes = this.inputStreamToByteArray(content);
		InputStream is = new ByteArrayInputStream(contentBytes);
		String contentType = URLConnection.guessContentTypeFromStream(is);

		if (contentType == null) {
			throw new IOException("Could not determine content type of stream");
		}

		this.dataSource = new ByteArrayDataSource(contentBytes, contentType);
	}
}

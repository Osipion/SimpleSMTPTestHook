package main.java.com.osipion.mailTest;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.URLConnection;

import javax.mail.Part;
import javax.mail.util.ByteArrayDataSource;

import org.apache.commons.lang3.StringUtils;

public class FileTestAttachment extends TestAttachment {

	protected FileTestAttachment(final String name, final String description, String contentType, InputStream content)
			throws IOException {
		super(Part.ATTACHMENT, name, description);

		byte[] contentBytes = this.inputStreamToByteArray(content);
		InputStream is = new ByteArrayInputStream(contentBytes);

		if (StringUtils.isBlank(contentType)) {
			contentType = URLConnection.guessContentTypeFromStream(is);
			if (contentType == null) {
				contentType = "application/octet-stream";
			}
		}

		this.dataSource = new ByteArrayDataSource(contentBytes, contentType);
	}

}

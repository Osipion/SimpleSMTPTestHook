package main.java.com.osipion.mailTest;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;

import javax.activation.DataSource;

public abstract class TestAttachment {
	final String partType;
	final String name;
	final String description;

	protected DataSource dataSource;

	protected TestAttachment(final String partType, final String name, final String description) {
		this.partType = partType;
		this.name = name;
		this.description = description;
	}

	protected byte[] inputStreamToByteArray(InputStream inputStream) throws IOException {
		byte[] buffer = new byte[1024];
		ByteArrayOutputStream tmpStream = new ByteArrayOutputStream();

		int bytesRead;
		while ((bytesRead = inputStream.read(buffer)) != -1) {
			tmpStream.write(buffer, 0, bytesRead);
		}

		return tmpStream.toByteArray();
	}

	String getName() {
		return this.name;
	}

	String getDescription() {
		return this.description;
	}

	String getPartType() {
		return this.partType;
	}

	DataSource getDataSource() {
		return this.dataSource;
	}
}

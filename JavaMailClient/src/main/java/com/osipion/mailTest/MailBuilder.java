package main.java.com.osipion.mailTest;

import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;

import javax.mail.internet.AddressException;
import javax.mail.internet.InternetAddress;

import org.apache.commons.mail.EmailException;
import org.apache.commons.mail.HtmlEmail;

public class MailBuilder {

	private final String hostName;

	public MailBuilder(final String hostName) {
		this.hostName = hostName;
	}

	public HtmlEmail newBlankMailItem() {
		HtmlEmail mailItem = new HtmlEmail();
		mailItem.setHostName(this.hostName);
		return mailItem;
	}

	public HtmlEmail simpleMailItem() throws EmailException, AddressException {
		HtmlEmail mailItem = newBlankMailItem();
		List<InternetAddress> recipients = new ArrayList<InternetAddress>();
		recipients.add(new InternetAddress("recipient.address@fake.dom"));

		mailItem.setCharset(getEncoding().name());
		mailItem.setFrom("source.address@fake.dom");
		mailItem.setTo(recipients);
		mailItem.setSubject("A simple html email");
		mailItem.setHtmlMsg("<!DOCTYPE HTML><html><h1>My content</h1></html>");
		return mailItem;
	}

	public HtmlEmail mailItemWithEmbeddedJpg() throws EmailException, AddressException, IOException {
		HtmlEmail mailItem = this.simpleMailItem();
		InputStream source = this.getResource("attachments/temple.jpg");
		TestAttachment att = new EmbeddedTestAttachment("temple", "a temple", source);
		mailItem.embed(att.getDataSource(), att.getName(), att.getDescription());
		return mailItem;
	}

	public HtmlEmail mailItemWithAttachedJpg() throws EmailException, AddressException, IOException {
		HtmlEmail mailItem = this.simpleMailItem();
		InputStream source = this.getResource("attachments/temple.jpg");
		TestAttachment att = new FileTestAttachment("temple", "a temple", null, source);
		mailItem.attach(att.getDataSource(), att.getName(), att.getDescription());
		return mailItem;
	}

	private InputStream getResource(String path) throws IOException {
		return new FileInputStream("resources/" + path);
	}

	public Charset getEncoding() {
		return StandardCharsets.UTF_8;
	}
}

package main.java.com.osipion.mailTest;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;

public class Program {

	public static void main(String[] args) throws IOException {
		System.out.println("Java Mail Test Client:");
		System.out.println("Options:");
		System.out.println("\t 1: simple html email");
		System.out.println("\t 2: html email with embedded jpg");
		System.out.println("\t 3: html email with jpg file attachment");
		System.out.println("\t pwd: print working directory");
		System.out.println("\t q: quit");

		BufferedReader br = new BufferedReader(new InputStreamReader(System.in));

		MailBuilder mailSource = new MailBuilder("localhost");

		while (true) {
			String input = br.readLine().trim();
			if (input.equals("q"))
				break;

			switch (input) {
			case "pwd":
				System.out.println(System.getProperty("user.dir"));
				break;
			case "1":
				try {
					mailSource.simpleMailItem().send();
					System.out.println("Simple mail sent...");
				} catch (Exception e) {
					printError(e);
				}
				break;
			case "2":
				try {
					mailSource.mailItemWithEmbeddedJpg().send();
					System.out.println("Mail with embedded jpg sent...");
				} catch (Exception e) {
					printError(e);
				}
				break;
			case "3":
				try {
					mailSource.mailItemWithAttachedJpg().send();
					System.out.println("Mail with attached jpg sent...");
				} catch (Exception e) {
					printError(e);
				}
				break;
			default:
				System.out.println("Unrecognized option...");
			}
		}
		System.out.println("Exiting...");
	}

	static void printError(Exception e) {
		System.err.println(e.getMessage());
		e.printStackTrace();
	}

}

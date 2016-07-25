#include <stdio.h>
#include <iostream>
#include <string>
#include <Windows.h>

using namespace std;

int main(int argc, char** argv) {
	string adapterName;
	string teamNumber;
	string ignore;

	if (argc > 1) {
		if (argc == 3) {
			adapterName = argv[1];
			teamNumber = argv[2];
		} else {
			cout << "Usage: SetupLoopbackAdapter.exe \"<adapter name>\" <team number>" << endl;
			return getline(cin, ignore), 0;
		}
	} else {
		fprintf(stdout, "\nEnter the name of your adapter (see the tutorial): ");
		fflush(stdout);
		getline(cin, adapterName);
		fprintf(stdout, "\nEnter your team number: ");
		fflush(stdout);
		getline(cin, teamNumber);
	}

	if (teamNumber.length() > 4) {
		fprintf(stdout, "Invalid team number, exiting.");
		return getline(cin, ignore), 0;
	}

	fprintf(stdout, "\n\nIf the program fails, ensure that you are running it as an admin and have installed the loopback adapter. Starting the installation process.\n\n");
	
	string ip1, ip2;
	switch (teamNumber.length()) {
	case 1:
		ip1 = "0";
		ip2 = teamNumber;
		break;
	case 2:
		ip1 = "0";
		ip2 = teamNumber;
	case 3:
		ip1 = teamNumber.substr(0, 1);
		ip2 = teamNumber.substr(1, 2);
		break;
	case 4:
		ip1 = teamNumber.substr(0, 2);
		ip2 = teamNumber.substr(2, 2);
		break;
	default:
		fprintf(stdout, "Invalid team number, exiting.");
		return getline(cin, teamNumber), 0;
		break;
	}

	// examples:
	// netsh interface ipv4 set address "Ethernet 2" static 10.99.99.2 255.255.255.0 none
	// netsh interface ipv4 add address "Ethernet 2" 10.99.99.5 255.255.255.0
	string command1 = string("netsh interface ipv4 set address \"").append(adapterName).append("\" static 10.").append(ip1).append(".").append(ip2).append(".2 255.255.255.0 none");
	string command2 = string("netsh interface ipv4 add address \"").append(adapterName).append("\" 10.").append(ip1).append(".").append(ip2).append(".5 255.255.255.0");

	//fprintf(stdout, "%s\n", command1.c_str());
	system(command1.c_str());
	//fprintf(stdout, "%s\n", command2.c_str());
	system(command2.c_str());

	fprintf(stdout, "Installation completed, if there are no errors it completed successfully.");
	return getline(cin, ignore), 0;
}
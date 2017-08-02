#include <stdio.h>
#include <iostream>
#include <string>
#include <Windows.h>

void pause() {
    std::string ignore;
    std::getline(std::cin, ignore);
}

int main(int argc, char** argv) {
    std::string adapterName;
    std::string teamNumber;
    std::string ignore;

	if (argc > 1) {
		if (argc == 3) {
			adapterName = argv[1];
			teamNumber = argv[2];
		}
        else {
            std::cout << "Usage: SetupLoopbackAdapter.exe \"<adapter name>\" <team number>" << std::endl;
            pause();
			return -1;
		}
	}
    else {
        std::cout << "\nEnter the name of your adapter (see the tutorial): " << std::flush;
        std::getline(std::cin, adapterName);
        std::cout << "\nEnter your team number: " << std::flush;
        std::getline(std::cin, teamNumber);
	}

	if (teamNumber.length() > 4) {
        std::cout << "Invalid team number, exiting." << std::endl;
        pause();
        return -1;
	}

    std::cout << "\n\nIf the program fails, ensure that you are running it as an admin and have installed the loopback adapter. Starting the installation process." << std::endl << std::endl;
	
    std::string ip1, ip2;
	switch (teamNumber.length()) {
	case 1:
		ip1 = "0";
		ip2 = teamNumber;
		break;
	case 2:
		ip1 = "0";
		ip2 = teamNumber;
		ip1 = teamNumber.substr(0, 1);
		ip2 = teamNumber.substr(1, 2);
		break;
	case 4:
		ip1 = teamNumber.substr(0, 2);
		ip2 = teamNumber.substr(2, 2);
		break;
	default:
        std::cout << "Invalid team number, exiting." << std::endl;
        pause();
        return -1;
	}

	// examples:
	// netsh interface ipv4 set address "Ethernet 2" static 10.99.99.2 255.255.255.0 none
	// netsh interface ipv4 add address "Ethernet 2" 10.99.99.5 255.255.255.0
    std::string command1 = std::string("netsh interface ipv4 set address \"").append(adapterName).append("\" static 10.").append(ip1).append(".").append(ip2).append(".2 255.255.255.0 none");
    std::string command2 = std::string("netsh interface ipv4 add address \"").append(adapterName).append("\" 10.").append(ip1).append(".").append(ip2).append(".5 255.255.255.0");

	if(system(command1.c_str()) != 0) {
        pause();
        return -1;
    }
	if(system(command2.c_str()) != 0) {
        pause();
        return -1;
    }

    std::cout << "Installation completed, if there are no errors it completed successfully." << std::endl;
    pause();
    return 0;
}

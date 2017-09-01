#this makefile is used to automate the emulation part of the installer build
#process. run `make install` to copy files to the install directory.
#this will build all the hel code except for the build tool and copy it to
#../installer

.PHONY: install installHELBuildTool

#chain the makefile to build and install stuff for the HEL
install: installHELBuildTool
	@$(MAKE) -C hel install TARGET=release

installHELBuildTool:
	@mkdir -p ../installer/SynthesisDrive/HELBuildTool
	@echo -e "\e[1m\e[32mINSTALL\e[39m HELBuildTool/Makefile\e[0m"
	@cp -f HELBuildTool/Makefile ../installer/SynthesisDrive/HELBuildTool/Makefile
	@echo -e "\e[1m\e[32mINSTALL\e[39m HELBuildTool/Makefile.java\e[0m"
	@cp -f HELBuildTool/Makefile.java ../installer/SynthesisDrive/HELBuildTool/Makefile.java
	@if [ -a HELBuildTool/HELBuildTool/bin/x64/Release/b.exe ]; then \
		echo -e "\e[1m\e[32mINSTALL\e[39m HELBuildTool/HELBuildTool.exe\e[0m"; \
		cp -f HELBuildTool/HELBuildTool/bin/x64/Release/b.exe ../installer/SynthesisDrive/HELBuildTool/HELBuildTool.exe; \
	else \
		echo -e "\e[32mBuild HELBuildTool in release mode before running make install\e[0m"; \
		exit 1; \
	fi

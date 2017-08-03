.PHONY: install installHELBuildTool

install: installHELBuildTool
	@$(MAKE) -C hel install

installHELBuildTool:
	@mkdir -p ../installer/SynthesisDrive/HELBuildTool
	@echo -e "\e[1m\e[32mINSTALL\e[39m HELBuildTool/Makefile\e[0m"
	@cp -f HELBuildTool/Makefile ../installer/SynthesisDrive/HELBuildTool/Makefile
	@if [ -a HELBuildTool/HELBuildTool/bin/Release/b.exe ]; then \
		echo -e "\e[1m\e[32mINSTALL\e[39m HELBuildTool/HELBuildTool.exe\e[0m"; \
		cp -f HELBuildTool/HELBuildTool/bin/Release/b.exe ../installer/SynthesisDrive/HELBuildTool/HELBuildTool.exe; \
	else \
		echo -e "\e[32mBuild HELBuildTool in release mode before running make install\e[0m"; \
		exit 1; \
	fi

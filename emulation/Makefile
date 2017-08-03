.PHONY: install installHELBuildTool installSetupLoopbackAdapter

install: installHELBuildTool installSetupLoopbackAdapter
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

installSetupLoopbackAdapter:
	@mkdir -p ../installer/SynthesisDrive
	@echo -e "\e[1m\e[32mINSTALL\e[39m SetupLoopbackAdapter.exe\e[0m"
	@if [ -a SetupLoopbackAdapter/Release/SetupLoopbackAdapter.exe ]; then \
		echo -e "\e[1m\e[32mINSTALL\e[39m HELBuildTool/HELBuildTool.exe\e[0m"; \
		cp -f SetupLoopbackAdapter/Release/SetupLoopbackAdapter.exe ../installer/SynthesisDrive/SetupLoopbackAdapter.exe; \
	else \
		echo -e "\e[32mBuild SetupLoopbackAdapter in release mode before running make install\e[0m"; \
		exit 1; \
	fi


#!/bin/bash
cat ../../emulator_building.md | sed -n 's/\(\$.\)/\1/p' | cut -c3- > ../build_vm.sh; chmod +x ../build_vm.sh

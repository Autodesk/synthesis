#!/bin/bash
if [ ! -d ../lib/ctre ] ; then
	git clone --branch Phoenix_v5.3.1.0 https://github.com/CrossTheRoadElec/Phoenix-frc-lib ../lib/ctre
fi

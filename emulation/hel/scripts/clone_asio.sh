#!/bin/bash
if [ ! -d ../../external/ASIO ] ; then
	git clone https://github.com/chriskohlhoff/asio/ ../lib/ASIO
fi

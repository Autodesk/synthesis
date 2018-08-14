#!/bin/bash

# Checks if ASIO has been cloned, clones if it has not been

if [ ! -d ../lib/ASIO ] ; then
	git clone https://github.com/chriskohlhoff/asio/ ../lib/ASIO
fi

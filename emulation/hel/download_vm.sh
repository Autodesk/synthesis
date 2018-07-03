#!/bin/bash

wget -O synthesis_image.tar.gz #INSERT URL HERE
mkdir synthesis_image -p;
tar -xvzf synthesis_image.tar.gz --directory synthesis_image;
cd synthesis_image;
mv new_root_files/* . && rm -rf new_root_files;
cd ../;

#!/bin/bash

wget -O synthesis_image.tar.gz https://uc1047ee6720d098b469c6f3b731.dl.dropboxusercontent.com/cd/0/get/AKjGidGE2hBx8Jx2DQX3E1OWVSjxCNXbIcraSeaUty-rOoiEcHOTQeTP5amIUw8YGYwFrowQaGAiT9s8LyRCRr3mXw06WmVd3DmqHZI0WJOUA7MXI4-tMfFqwfPkfMXobTx0jgRpVH9TzAx2V8c4x6ZXX0gfm4nFHxWY_2h-1J-wp0PtbKhtBWWLkGfzg2PAnEk/file?dl=1
mkdir synthesis_image -p;
tar -xvzf synthesis_image.tar.gz --directory synthesis_image;
cd synthesis_image;
mv new_root_files/* . && rm -rf new_root_files;
cd ../;

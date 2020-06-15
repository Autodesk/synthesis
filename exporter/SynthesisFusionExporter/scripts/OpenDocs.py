# ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
#  Copyright (c) 2020 by Patrick Rainsberry.                                   ~
#  :license: MIT, see LICENSE for more details.                            ~
#  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
#  OpenDocs.py                                                                 ~
#  This file is a component of ApperSample.                                    ~
# ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

import webbrowser
import pathlib
from multiprocessing.connection import Listener


def open_file(local_path):
    file_uri = pathlib.Path(local_path).as_uri()
    fusion_path = "fusion360://command=open&file=" + file_uri[8:]
    webbrowser.open(fusion_path)


def main():

    # TODO change these paths
    local_paths = [
        '/Users/rainsbp/Downloads/test1.f3d',
        '/Users/rainsbp/Downloads/test2.f3d',
        '/Users/rainsbp/Downloads/test3.f3d',
    ]

    address = ('localhost', 6000)     # family is deduced to be 'AF_INET'
    listener = Listener(address)

    conn = listener.accept()

    for local_path in local_paths:
        open_file(local_path)
        msg = conn.recv()
        print(str(msg))


if __name__ == "__main__":
    main()

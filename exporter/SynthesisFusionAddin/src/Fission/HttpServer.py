from http.server import BaseHTTPRequestHandler
import logging

from ..general_imports import INTERNAL_ID
# from socketserver import BaseRequestHandler

class MyHTTPHandler(BaseHTTPRequestHandler):

    def __init__(self, a, b, c):
        print('init')
        pass

    def do_GET(self):
        logging.getLogger(f'{INTERNAL_ID}').debug('Request')
        self.send_response(200)
        self.end_headers()
        self.wfile.write(bytes('test'))

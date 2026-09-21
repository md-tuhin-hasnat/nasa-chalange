#!/usr/bin/env python3
import http.server
import socketserver
import os
import sys

PORT = 8080
DIRECTORY = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "Build", "WebGL"))

class Handler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=DIRECTORY, **kwargs)

    def end_headers(self):
        # Enable Cross-Origin Isolation for high-performance WebAssembly
        self.send_header("Cross-Origin-Opener-Policy", "same-origin")
        self.send_header("Cross-Origin-Embedder-Policy", "require-corp")
        self.send_header("Access-Control-Allow-Origin", "*")
        super().end_headers()

if __name__ == "__main__":
    if not os.path.exists(DIRECTORY):
        print(f"Error: WebGL build directory not found at: {DIRECTORY}")
        sys.exit(1)

    with socketserver.TCPServer(("", PORT), Handler) as httpd:
        print(f"Serving Unity WebGL at http://localhost:{PORT}")
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\nServer stopped.")

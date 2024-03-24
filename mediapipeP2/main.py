import socket
from body import BodyThread
import global_vars
from sys import exit
import time
import body
import socket
from threading import Thread
import body

def receive_broadcast_message(port, ip_received_callback):
    ipFlag = False  # ipFlag is for when we have received a successful IP broadcast we stop checking for it
    # Create a UDP socket
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
    
    try:
        # Bind the socket to the port
        s.bind(('', port))
        if global_vars.MAINDEBUG:
            print(f"Listening for broadcast messages on port {port}...")

        while True:
            # Receive message
            data, addr = s.recvfrom(1024)
            if data and not ipFlag:
                ip_address = data.decode()  # Assuming the message is the IP address
                if global_vars.MAINDEBUG:
                    print(f"Received broadcast message from {addr}: {ip_address}")
                ip_received_callback(ip_address)  # Pass the IP address to the callback function
                ipFlag = True
            elif data.decode() == "STOP":
                s.close()
                if global_vars.MAINDEBUG:
                    print("Exiting…")
                global_vars.KILL_THREADS = True
                time.sleep(0.5)
                exit()
    
    except Exception as e:
        if global_vars.MAINDEBUG:
            print("Error:", e)
    finally:
        s.close()

# Define the BodyThread class here...

# Callback function to handle received IP address
def handle_ip_received(ip_address):
    if ip_address:
        body.ipAddress = ip_address
        thread = BodyThread()
        thread.start()
        
# Example usage
port = 5020  # Choose the port number
broadcast_thread = Thread(target=receive_broadcast_message, args=(port, handle_ip_received))
broadcast_thread.start()

# Wait for user input
input("Press Enter to exit...")
print("Exiting…")
global_vars.KILL_THREADS = True
time.sleep(0.5)
exit()











#IMPORTS
import socket
from body import BodyThread
import global_vars
from sys import exit
import time
import body
import socket
from threading import Thread
import body
#FUNCTIONS

#function for receieiving the broadcast messages
def receive_broadcast_message(port, ip_received_callback):
    ipFlag = False  # ipFlag is for when we have received a successful IP broadcast we stop checking for it
    # Create a UDP socket for listening for broadcast
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
                ipFlag = True  #stops accepting the broadcast messages as Ip address has been recieved
            elif data.decode() == "STOP": #now this thread is only on for recieiving a stop message 
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

# Callback function to handle received IP address
def handle_ip_received(ip_address):
    # Once broadcast signal recieved it sets the IP address for the body
    # And starts the camera thread
    if ip_address:
        body.ipAddress = ip_address
        thread = BodyThread()
        thread.start()



#NONE THREADED CODE



#threads broadcast reciever 
port = 5020  # Choose the port number
broadcast_thread = Thread(target=receive_broadcast_message, args=(port, handle_ip_received))
broadcast_thread.start()

#exit code
# Wait for user input to close (manual close)
input("Press Enter to exit...")
print("Exiting…")
global_vars.KILL_THREADS = True
time.sleep(0.5)
exit()

#END









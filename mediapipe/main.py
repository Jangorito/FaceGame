from body import BodyThread
import time
import struct
import global_vars
import threading
import Reciever
from sys import exit
import signal  # Import signal module to handle termination signal

# Example usage
port1 = 5010
forward_ip1 = '127.0.0.1'  # Local IP address of the device
forward_port1 = 5018  # Port to forward the data to

port2 = 5012
forward_ip2 = '127.0.0.1'  # Local IP address of the device
forward_port2 = 5015  # Port to forward the data to

broadcastPort = 5020
# Function to run receiver in a separate thread
def run_receiver():
    Reciever.receive_and_forward_messages(port1, forward_ip1, forward_port1, port2, forward_ip2, forward_port2)


def run_broadcast():
   while Reciever.broadcastFlag:
    Reciever.send_broadcast_message(broadcastPort)
    time.sleep(1)
# Create and start receiver thread
receiver_thread = threading.Thread(target=run_receiver)
receiver_thread.start()


broadcast_thread = threading.Thread(target=run_broadcast)
broadcast_thread.start()
# Start body thread
thread = BodyThread()
thread.start()


# Wait for input to exit
i = input("Press Enter to exit...\n")
print("Exiting…")
Reciever.send_stop_message(broadcastPort)
global_vars.KILL_THREADS = True
time.sleep(0.5)
exit()












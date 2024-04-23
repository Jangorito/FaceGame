#IMPORTS

from body import BodyThread
import time
import global_vars
import threading
import Reciever
from sys import exit
#GLOBALVARS

#PORT1(RAW IMAGE DATA)
port1 = 5010 #raw image data receieved at this port of P2
forward_ip1 = '127.0.0.1'  # Local IP address of the device
forward_port1 = 5018  # Port to forward the raw image data of P2 to unity

#PORT1(MEDIAPIPE DATA)
port2 = 5012 #mediaPipe landmarks receieved at this port of P2  
forward_ip2 = '127.0.0.1'  # Local IP address of the device
forward_port2 = 5015  # Port to forward the landmark data to Unity (NOTE C# people this is where you recieve P2 OSC data for LM)


#PORT3(BROADCAST MESSAGE)
broadcastPort = 5020


#FUNCTIONS
# Function to run receiver in a separate thread
def run_receiver():
    Reciever.receive_and_forward_messages(port1, forward_ip1, forward_port1, port2, forward_ip2, forward_port2)# calls the reciever and forward function 
def run_broadcast():
   while Reciever.broadcastFlag:
    Reciever.send_broadcast_message(broadcastPort) #calls the IP broadcaster whenever it has not yet been recieved
    time.sleep(1)



#NONE THREADED CODE   
# Create and start receiver thread
receiver_thread = threading.Thread(target=run_receiver)
receiver_thread.start()
# Create and start broadcast thread
broadcast_thread = threading.Thread(target=run_broadcast)
broadcast_thread.start()
# Start body thread (mediapipe camera thread)
thread = BodyThread()
thread.start()
#exit code
# Wait for input to exit
i = input("Press Enter to exit...\n")
print("Exiting…")
Reciever.send_stop_message(broadcastPort)
global_vars.KILL_THREADS = True
time.sleep(0.5)
Reciever.exit()
time.sleep(0.5)
exit()


#END









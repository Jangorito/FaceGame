#pipe server
from body import BodyThread
import time
import struct
import global_vars



from sys import exit



thread = BodyThread()


import socket
def get_ip_address():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.connect(("8.8.8.8", 80))
    return s.getsockname()[0]


print(get_ip_address())



thread.start()


i = input()
print("Exiting…")        
global_vars.KILL_THREADS = True
time.sleep(0.5)
exit()












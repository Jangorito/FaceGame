# broadcast_receiver.py

import socket
import netifaces
import ipaddress
import time
broadcastFlag = True
def get_local_network_info():
    # Get the local IP address
    local_ip = socket.gethostbyname(socket.gethostname())

    # Get the network interfaces
    interfaces = netifaces.interfaces()

    # Find the interface with the local IP address
    for iface in interfaces:
        iface_details = netifaces.ifaddresses(iface)
        if netifaces.AF_INET in iface_details:
            for addr_info in iface_details[netifaces.AF_INET]:
                if addr_info['addr'] == local_ip:
                    # Get the network address and subnet mask
                    network_address = addr_info['addr']
                    subnet_mask = addr_info['netmask']
                    return network_address, subnet_mask

def calculate_broadcast_address(network_address, subnet_mask):
    # Create an IPv4Network object
    network = ipaddress.IPv4Network(network_address + '/' + subnet_mask, strict=False)
    
    # Get the broadcast address from the network object
    broadcast_address = network.broadcast_address

    return str(broadcast_address)

def send_broadcast_message(port):
    # Get the local network information
    network_address, subnet_mask = get_local_network_info()

    if network_address and subnet_mask:
        # Calculate the broadcast address
        broadcast_address = calculate_broadcast_address(network_address, subnet_mask)
        
        # Create a UDP socket
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)

        try:
            # Send the message to the broadcast address
            s.sendto(network_address.encode(), (broadcast_address, port))
            print("Broadcast message sent successfully.")
        except Exception as e:
            print("Failed to send broadcast message:", e)
        finally:
            s.close()
    else:
        print("Failed to retrieve local network information.")




def send_stop_message(port):
    # Get the local network information
    network_address, subnet_mask = get_local_network_info()

    if network_address and subnet_mask:
        # Calculate the broadcast address
        broadcast_address = calculate_broadcast_address(network_address, subnet_mask)
        
        # Create a UDP socket
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)

        try:
            # Send the message to the broadcast address
            s.sendto("STOP".encode(), (broadcast_address, port))
            print("STOP")
        except Exception as e:
            print("Failed to send broadcast message:", e)
        finally:
            s.close()
    else:
        print("Failed to retrieve local network information.")






def receive_and_forward_messages(port1, forward_ip1, forward_port1, port2, forward_ip2, forward_port2):
    global broadcastFlag 
    # Host to listen on
    host = '0.0.0.0'  # Listen on all available interfaces

    # Create UDP sockets for receiving
    receiver_socket1 = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    receiver_socket2 = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    # Bind the first socket to the host and port1
    receiver_socket1.bind((host, port1))
    print(f"Receiver is waiting for messages on port {port1}...")

    # Bind the second socket to the host and port2
    receiver_socket2.bind((host, port2))
    print(f"Receiver is waiting for messages on port {port2}...")

    # Create UDP sockets for forwarding
    forward_socket1 = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    forward_socket2 = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    while True:
        # Receive data from the first sender
        data1, addr1 = receiver_socket1.recvfrom(230400)
        print(f"Message from {addr1} (Port {port1}): {data1}")
        if(data1):
            broadcastFlag = False
        # Forward the received data to the local IP of the device (for the first socket)
        forward_socket1.sendto(data1, (forward_ip1, forward_port1))

        # Receive data from the second sender
        data2, addr2 = receiver_socket2.recvfrom(230400)
        print(f"Message from {addr2} (Port {port2}): {data2}")

        # Forward the received data to the local IP of the device (for the second socket)
        forward_socket2.sendto(data2, (forward_ip2, forward_port2))

        # Terminate if the message is 'exit' (you may want to handle this differently for two sockets)
        if data1.lower() == b'exit' or data2.lower() == b'exit':
            break

    # Close the sockets
    receiver_socket1.close()
    receiver_socket2.close()
    forward_socket1.close()
    forward_socket2.close()



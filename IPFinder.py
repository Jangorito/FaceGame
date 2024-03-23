import socket

def receive_broadcast_message(port):
    # Create a UDP socket
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
    
    try:
        # Bind the socket to the port
        s.bind(('', port))
        print(f"Listening for broadcast messages on port {port}...")

        while True:
            # Receive message
            data, addr = s.recvfrom(1024)
            print(f"Received message from {addr}: {data.decode()}")
    except Exception as e:
        print("Error:", e)
    finally:
        s.close()

# Example usage
port = 5020  # Choose the port number

receive_broadcast_message(port)

# import socket
# import netifaces
# import ipaddress

# def get_local_network_info():
#     # Get the local IP address
#     local_ip = socket.gethostbyname(socket.gethostname())

#     # Get the network interfaces
#     interfaces = netifaces.interfaces()

#     # Find the interface with the local IP address
#     for iface in interfaces:
#         iface_details = netifaces.ifaddresses(iface)
#         if netifaces.AF_INET in iface_details:
#             for addr_info in iface_details[netifaces.AF_INET]:
#                 if addr_info['addr'] == local_ip:
#                     # Get the network address and subnet mask
#                     network_address = addr_info['addr']
#                     subnet_mask = addr_info['netmask']
#                     return network_address, subnet_mask

# def calculate_broadcast_address(network_address, subnet_mask):
#     # Create an IPv4Network object
#     network = ipaddress.IPv4Network(network_address + '/' + subnet_mask, strict=False)
    
#     # Get the broadcast address from the network object
#     broadcast_address = network.broadcast_address

#     return str(broadcast_address)

# # Get the local network information
# network_address, subnet_mask = get_local_network_info()

# if network_address and subnet_mask:
#     print("Network Address:", network_address)
#     print("Subnet Mask:", subnet_mask)

#     # Calculate and print the broadcast address
#     broadcast_address = calculate_broadcast_address(network_address, subnet_mask)
#     print("Broadcast Address:", broadcast_address)
# else:
#     print("Failed to retrieve local network information.")

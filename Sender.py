import socket
import netifaces
import ipaddress

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

def send_broadcast_message(message, port):
    # Get the local network information
    network_address, subnet_mask = get_local_network_info()

    if network_address and subnet_mask:
        # Calculate the broadcast address
        broadcast_address = calculate_broadcast_address(network_address, subnet_mask)
        print(broadcast_address)
        # Create a UDP socket
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)

        try:
            # Send the message to the broadcast address
            s.sendto(message.encode(), (broadcast_address, port))
            print("Broadcast message sent successfully.")
        except Exception as e:
            print("Failed to send broadcast message:", e)
        finally:
            s.close()
    else:
        print("Failed to retrieve local network information.")

# Example usage
message = "Hello, this is a broadcast message!"
port = 5020  # Choose the port number

send_broadcast_message(message, port)
 
import socket
import netifaces
import ipaddress
import time

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

def send_broadcast_message( port):
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

# Example usage
message = "Hello, this is a broadcast message!"
port = 5020  # Choose the port number

# Start time for the broadcast loop
start_time = time.time()

while True:
    # Send broadcast message
    send_broadcast_message( port)
    
    # Check for stop message every second
    if time.time() - start_time >= 1:
        with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as receive_socket:
            receive_socket.bind(('0.0.0.0', port))
            receive_socket.settimeout(1)  # Set timeout to 1 second
            try:
                data, addr = receive_socket.recvfrom(1024)
                if data.decode() == "stop":
                    print("Received stop message. Stopping broadcast loop.")
                    exit()
            except socket.timeout:
                pass  # Continue the broadcast loop if no message is received within 1 second
    
    # Wait for the next second to send the next broadcast message
    time.sleep(1)



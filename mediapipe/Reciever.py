import socket

# Host and port to listen on
host = '0.0.0.0'  # Listen on all available interfaces

# Configuration for the first socket
port1 = 5010
forward_ip1 = '127.0.0.1'  # Local IP address of the device
forward_port1 = 5018  # Port to forward the data to

# Configuration for the second socket
port2 = 5012
forward_ip2 = '127.0.0.1'  # Local IP address of the device
forward_port2 = 5015  # Port to forward the data to

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
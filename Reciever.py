import socket

# Host and port to listen on
host = '0.0.0.0'  # Listen on all available interfaces
port = 5010

# IP address and port to forward the data
forward_ip = '127.0.0.1'  # Local IP address of the device
forward_port = 5018  # Port to forward the data to

# Create a UDP socket for receiving
receiver_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Bind the socket to the host and port
receiver_socket.bind((host, port))

print("Receiver is waiting for messages...")

# Create a UDP socket for forwarding
forward_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

while True:
    # Receive data from the sender
    data, addr = receiver_socket.recvfrom(230400)

    # Decode and print the received data
    message = data
    print(f"Message from {addr}: {message}")

    # Forward the received data to the local IP of the device
    forward_socket.sendto(data, (forward_ip, forward_port))

    # Terminate if the message is 'exit'
    if message.lower() == 'exit':
        break

# Close the sockets
receiver_socket.close()
forward_socket.close()

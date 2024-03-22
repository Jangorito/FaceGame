import socket

# Receiver's IP address and port
receiver_ip = '192.168.0.115'  # Change this to the IP address of the receiver
receiver_port = 12345  # Change this to the port on which receiver is listening

# Create a socket object
sender_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Connect to the receiver
sender_socket.connect((receiver_ip, receiver_port))

while True:
    # Get input from the user
    message = input("Enter your message: ")

    # Send the message to the receiver
    sender_socket.send(message.encode())

    # Terminate if the message is 'exit'
    if message.lower() == 'exit':
        break

# Close the socket
sender_socket.close()
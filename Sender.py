import socket

def send_broadcast_message(message, port):
    # Create a UDP socket
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)

    try:
        # Send the message to the broadcast address
        s.sendto(message.encode(), ('192.168.0.255', port))
        print("Broadcast message sent successfully.")
    except Exception as e:
        print("Failed to send broadcast message:", e)
    finally:
        s.close()

# Example usage
message = "Hello, this is a broadcast message!"
port = 5020  # Choose the port number

send_broadcast_message(message, port)

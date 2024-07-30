import socket
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation

# Setup socket
HOST = '127.0.0.1'
PORT = 65432

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.bind((HOST, PORT))
sock.listen(1)
conn, addr = sock.accept()

# Initialize plot
fig, ax = plt.subplots()
x_data = list(range(100))
y_data = [[0] * 100 for _ in range(8)]
lines = [ax.plot(x_data, y_data[i], label=f'Coil {i+1}')[0] for i in range(8)]
ax.set_ylim(-10, 10)
ax.legend()

def update(frame):
    data = conn.recv(1024).decode()
    currents = list(map(float, data.split(',')))
    for i, line in enumerate(lines):
        y_data[i].append(currents[i])
        del y_data[i][0]
        line.set_ydata(y_data[i])
    return lines

ani = FuncAnimation(fig, update, interval=100, blit=True)
plt.show()

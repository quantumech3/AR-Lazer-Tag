import pandas as pd

class Parser:
    def __init__(self, path: str):
        self.path = path
        self.data = pd.DataFrame()

    def update(self, fig):
        # Read data from CSV

        self.data = pd.read_csv(self.path)

        # Group by client ID
        grouped = self.data.groupby(['Client ID'])
        clients = grouped.groups.keys()
        num_clients = len(clients)

        axs = fig.get_axes()
        for index, client in enumerate(clients):
            # Filter to client
            group = grouped.get_group(client)

            # Plot positional data
            if len(axs) <= index * 5 + 0:
                fig.add_subplot(num_clients, 5, index * 5 + 1, projection='3d')
                axs = fig.get_axes()
            axs[index * 5 + 0].clear()
            axs[index * 5 + 0].set_title("VIO Position for Client ID: " + str(client))
            axs[index * 5 + 0].plot3D(group['VIO Position X'].tail(1000), group['VIO Position Z'].tail(1000), group['VIO Position Y'].tail(1000), color = 'red', linewidth = 1)
            axs[index * 5 + 0].plot3D(group['RF Position X'].tail(1000), group['RF Position Z'].tail(1000), group['RF Position Y'].tail(1000), color = 'green', linewidth = 1)
            axs[index * 5 + 0].plot3D(group['Fusion Position X'].tail(1000), group['Fusion Position Z'].tail(1000), group['Fusion Position Y'].tail(1000), color = 'blue', linewidth = 1)
            
                
            # Plot VIO rotational data
            if len(axs) <= index * 5 + 1:
                fig.add_subplot(num_clients, 5, index * 5 + 2)
                axs = fig.get_axes()
                axs[index * 5 + 1].set_title("VIO Rotation for Client ID: " + str(client))
                axs[index * 5 + 1].set_xlabel("Timestamp")
                axs[index * 5 + 1].set_ylabel("Rotation in degrees")
                axs[index * 5 + 1].set_ylim([0,360])
                axs[index * 5 + 1].plot([], [], color = 'red')
                axs[index * 5 + 1].plot([], [], color = 'green')
                axs[index * 5 + 1].plot([], [], color = 'blue')
            axs[index * 5 + 1].get_lines()[0].set_data(group['Timestamp'].tail(100), group['VIO Rotation X'].tail(100))
            axs[index * 5 + 1].get_lines()[1].set_data(group['Timestamp'].tail(100), group['VIO Rotation Y'].tail(100))
            axs[index * 5 + 1].get_lines()[2].set_data(group['Timestamp'].tail(100), group['VIO Rotation Z'].tail(100))
            axs[index * 5 + 1].relim()
            axs[index * 5 + 1].autoscale_view()

            # Plot accelerometer data
            if len(axs) <= index * 5 + 2:
                fig.add_subplot(num_clients, 5, index * 5 + 3)
                axs = fig.get_axes()
                axs[index * 5 + 2].set_title("Accelerometer data for Client ID: " + str(client))
                axs[index * 5 + 2].set_xlabel("Timestamp")
                axs[index * 5 + 2].set_ylabel("Meters per second per second")
                axs[index * 5 + 2].plot([], [], color = 'red')
                axs[index * 5 + 2].plot([], [], color = 'green')
                axs[index * 5 + 2].plot([], [], color = 'blue')
            axs[index * 5 + 2].get_lines()[0].set_data(group['Timestamp'].tail(100), group['Accelerometer X'].tail(100))
            axs[index * 5 + 2].get_lines()[1].set_data(group['Timestamp'].tail(100), group['Accelerometer Y'].tail(100))
            axs[index * 5 + 2].get_lines()[2].set_data(group['Timestamp'].tail(100), group['Accelerometer Z'].tail(100))
            axs[index * 5 + 2].relim()
            axs[index * 5 + 2].autoscale_view()
            
            # Plot gyroscope data
            if len(axs) <= index * 5 + 3:
                fig.add_subplot(num_clients, 5, index * 5 + 4)
                axs = fig.get_axes()
                axs[index * 5 + 3].set_title("Gyroscope data for Client ID: " + str(client))
                axs[index * 5 + 3].set_xlabel("Timestamp")
                axs[index * 5 + 3].set_ylabel("Rotation in degrees")
                axs[index * 5 + 3].set_ylim([0,360])
                axs[index * 5 + 3].plot([], [], color = 'red')
                axs[index * 5 + 3].plot([], [], color = 'green')
                axs[index * 5 + 3].plot([], [], color = 'blue')
            axs[index * 5 + 3].get_lines()[0].set_data(group['Timestamp'].tail(100), group['Gyroscope X'].tail(100))
            axs[index * 5 + 3].get_lines()[1].set_data(group['Timestamp'].tail(100), group['Gyroscope Y'].tail(100))
            axs[index * 5 + 3].get_lines()[2].set_data(group['Timestamp'].tail(100), group['Gyroscope Z'].tail(100))
            axs[index * 5 + 3].relim()
            axs[index * 5 + 3].autoscale_view()

            # Plot magnetometer data
            if len(axs) <= index * 5 + 4:
                fig.add_subplot(num_clients, 5, index * 5 + 5)
                axs = fig.get_axes()
                axs[index * 5 + 4].set_title("Magnetometer data for Client ID: " + str(client))
                axs[index * 5 + 4].set_xlabel("Timestamp")
                axs[index * 5 + 4].set_ylabel("Microteslas")
                axs[index * 5 + 4].plot([], [], color = 'red')
                axs[index * 5 + 4].plot([], [], color = 'green')
                axs[index * 5 + 4].plot([], [], color = 'blue')
            axs[index * 5 + 4].get_lines()[0].set_data(group['Timestamp'].tail(100), group['Magnetometer X'].tail(100))
            axs[index * 5 + 4].get_lines()[1].set_data(group['Timestamp'].tail(100), group['Magnetometer Y'].tail(100))
            axs[index * 5 + 4].get_lines()[2].set_data(group['Timestamp'].tail(100), group['Magnetometer Z'].tail(100))
            axs[index * 5 + 4].relim()
            axs[index * 5 + 4].autoscale_view()
        return fig.get_axes()
            



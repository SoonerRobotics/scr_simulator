# SCR IGVC 2020 Simulator

## Linux Run Instructions

1. Download the latest release from https://github.com/SoonerRobotics/igvc_simulator_2020/releases
2. Unzip it to the directory of your choice.
3. Run `sudo chmod +x igvc_simulator_2020.x86_64" to mark the simulator as executable.
4. Run the simulator using `./igvc_simulator_2020.x86_64`

## ROS Setup

Install ROS Bridge (for Melodic) using `sudo apt-get install ros-melodic-rosbridge-server`

Run ROS Bridge for the simulator using 
```
rosparam set port 9090
roslaunch rosbridge_server rosbridge_websocket.launch
```

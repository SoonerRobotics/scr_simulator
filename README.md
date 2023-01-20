# SCR Simulator for ROS

## Linux Run Instructions

1. Download the latest release from https://github.com/SoonerRobotics/scr_simulator/releases
2. Unzip it to the directory of your choice.
3. Run `sudo chmod +x scr_simulator.x86_64` to mark the simulator as executable.
4. Run the simulator using `./scr_simulator.x86_64`

## ROS 1 Setup

Install ROS Bridge (for Noetic) using `sudo apt-get install ros-noetic-rosbridge-server`

Run ROS Bridge for the simulator using 
```
rosparam set port 9090
roslaunch rosbridge_server rosbridge_websocket.launch
```

## ROS 2 Setup

Install ROS2 Bridge (for Humble) using `sudo apt-get install ros-humble-rosbridge-server`

Run ROS Bridge for the simulator using
```
ros2 launch rosbridge_server rosbridge_websocket_launch.xml
```

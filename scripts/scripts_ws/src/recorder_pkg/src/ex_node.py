#!/usr/bin/env python3
import rospy
import csv
from scr_sim_msgs.msg import gps
from geometry_msgs.msg import Pose

gps_hist = []
gps_real_hist = []
last_real_data = [[0,0]]

def gps_callback(gps_data):
    gps_hist.append([gps_data.latitude, gps_data.longitude])
    gps_real_hist.append(last_real_data[0])

def true_pose_callback(pose_data):
    lat = pose_data.position.x / 110944.12 + 35.194881
    lon = -pose_data.position.y / 91071.17 + -97.438621
    last_real_data[0] = [lat, lon]

def main():
    rospy.init_node('main', anonymous=True)

    rospy.Subscriber("/igvc/gps", gps, gps_callback)
    rospy.Subscriber("/sim/true_pose", Pose, true_pose_callback)

    rospy.spin()

    with open("data.csv", 'w') as f:
        writer = csv.writer(f)
        writer.writerow(['noisy_lat', 'noisy_lon', 'real_lat', 'real_lon'])

        for np, rp in zip(gps_hist, gps_real_hist):
            writer.writerow([np[0], np[1], rp[0], rp[1]])

if __name__ == '__main__':
    try:
        main()
    except rospy.ROSInterruptException:
        pass
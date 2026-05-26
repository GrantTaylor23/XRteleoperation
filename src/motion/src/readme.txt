HOW TO RUN:
    1) Build and source this package 
    2) Run the following command to start motion package:
            ros2 launch motion motion.launch.py ur_type:=ur3e onrobot_type:=rg2 use_fake_hardware:=true robot_ip:=192.168.0.197
        Change paramaters as required.
    3) In another terminal, start the ros tcp endpoint:
            ros2 run ros_tcp_endpoint default_server_endpoint --ros-args -p ROS_IP:=0.0.0.0 -p ROS_TCP_PORT:=10000
    4) In another terminal, start the live camera feed node to add the camera feed to the unity world:
            ros2 run usb_cam usb_cam_node_exe --ros-args --remap /image_raw:=/g8_LiveCamFeed -p pixel_format:=mjpeg2rgb -p image_width:=320 -p image_height:=180 -p framerate:=15.0 

TEST ROBOT MOVEMENT:
    1) Test Pose
        ros2 topic pub --once /g8_joint_states sensor_msgs/msg/JointState "{
        header: {stamp: {sec: 0, nanosec: 0}, frame_id: ''},
        name: ['shoulder_pan_joint', 'shoulder_lift_joint', 'elbow_joint', 'wrist_1_joint', 'wrist_2_joint', 'wrist_3_joint'],
        position: [1.54, -1.62, 1.4, -1.2, -1.6, -0.11],
        velocity: [],
        effort: []
        }"

    2) Home Pose
        ros2 topic pub --once /g8_joint_states sensor_msgs/msg/JointState "{
        header: {stamp: {sec: 0, nanosec: 0}, frame_id: ''},
        name: ['shoulder_pan_joint', 'shoulder_lift_joint', 'elbow_joint', 'wrist_1_joint', 'wrist_2_joint', 'wrist_3_joint'],
        position: [0.0, -1.5707, 0.0, -1.5707, 0.0, 0.0],
        velocity: [],
        effort: []
        }"

    3) Open Gripper
        ros2 topic pub --once /g8_gripper_cmd std_msgs/msg/Float32 "{data: 0.1}"

    4) Close Gripper
        ros2 topic pub --once /g8_gripper_cmd std_msgs/msg/Float32 "{data: 0.0}"


DEBUGGING:
    1) Manually change controllers:
        ros2 control list_controllers

        ros2 control switch_controllers \
        --activate scaled_joint_trajectory_controller \
        --deactivate joint_trajectory_controller

    2) Starting Polyscope sim: 
        ros2 run ur_client_library start_ursim.sh -m ur3e
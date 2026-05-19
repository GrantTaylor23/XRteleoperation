#include "movement.h"

Motion::Motion() : Node("motion_node")
{
    
    // This qos must match unity!!
    auto qos = rclcpp::QoS(10);     // history depth 10
    qos.best_effort();              // set reliability to Best-Effort

    // timer_ = this->create_wall_timer(
    //     std::chrono::milliseconds(200),
    //     std::bind(&Motion::publishJoints, this));


    // Subscriber for target joint poses of the robotmsg
    subTargetJointStates_ = this->create_subscription<sensor_msgs::msg::JointState>(
      "/g8_joint_states", qos,
      std::bind(&Motion::targetJointStateCb, this, std::placeholders::_1));

    subGripperCmd_ = this->create_subscription<std_msgs::msg::Float32>(
        "/g8_gripper_cmd", qos,
        std::bind(&Motion::gripperCmdCb, this, std::placeholders::_1));

    // // Publish joint states back to Unity
    // jointStatesToUnityPub_ = this->create_publisher<sensor_msgs::msg::JointState>("/g8_target_joint_states", qos);

    // last_ee_pose_ = std::make_shared<geometry_msgs::msg::PoseStamped>(); // initialise
}

Motion::~Motion(){
    if (motion_thread_.joinable()) {
        motion_thread_.join();
    }
}

void Motion::initMoveIt()
{
    // Initialise move group
    move_group_ = std::make_shared<moveit::planning_interface::MoveGroupInterface>(
        shared_from_this(), "ur_onrobot_manipulator");
    // Initialise gripper group
    gripper_group_ = std::make_shared<moveit::planning_interface::MoveGroupInterface>(
        shared_from_this(), "ur_onrobot_gripper");

    move_group_->setMaxVelocityScalingFactor(0.1);   // 10% speed
    move_group_->setMaxAccelerationScalingFactor(0.1);
    // Add these to initMoveIt() to give more tolerance
    move_group_->setGoalPositionTolerance(0.01);      // 1cm
    move_group_->setGoalOrientationTolerance(0.01);   // ~0.57 degrees
    move_group_->setGoalJointTolerance(0.01);

    gripper_group_->setMaxVelocityScalingFactor(1.0);   // gripper can move fast
    gripper_group_->setGoalJointTolerance(0.002);       // 2mm tolerance
}


void Motion::targetJointStateCb(const sensor_msgs::msg::JointState::SharedPtr msg) {

    if (!move_group_) { 
        RCLCPP_ERROR(this->get_logger(), "Move group not initialized!"); 
        return; 
    }
    if (msg->position.size() < 6) { 
        RCLCPP_WARN(this->get_logger(), "Received joint positions with less than 6 joints"); 
        return; 
    }

    // Drop immediately if still moving
    if (motion_busy_.load()) return;

    // return if another thread just claimed it
    if (motion_busy_.exchange(true)) return;

    // Safe to join now, busy was false so thread is finished
    if (motion_thread_.joinable()) motion_thread_.join();

    auto positions = msg->position;
    motion_thread_ = std::thread([this, positions]() {
        if (!move_group_) { 
            motion_busy_ = false; 
            return; 
        }

        std::vector<double> target = {
            positions[0], positions[1], positions[2],
            positions[3], positions[4], positions[5]
        };

        move_group_->setStartStateToCurrentState();
        move_group_->setJointValueTarget(target);

        moveit::planning_interface::MoveGroupInterface::Plan my_plan;
        bool success = (move_group_->plan(my_plan) ==
            moveit::planning_interface::MoveItErrorCode::SUCCESS);
        if (success) {
            move_group_->execute(my_plan);
            std::this_thread::sleep_for(std::chrono::milliseconds(500));
        } else {
            RCLCPP_WARN(this->get_logger(), "Planning to joint target failed.");
        }
        motion_busy_ = false;
    });

}

void Motion::gripperCmdCb(const std_msgs::msg::Float32::SharedPtr msg)
{
    if (!gripper_group_) {
        RCLCPP_ERROR(this->get_logger(), "Gripper not initialized!");
        return;
    }

    // Drop if arm or gripper is already moving
    if (motion_busy_.load()) return;
    if (motion_busy_.exchange(true)) return;

    if (motion_thread_.joinable()) motion_thread_.join();

    double width = static_cast<double>(msg->data);

    motion_thread_ = std::thread([this, width]() {
        moveGripper(width);
        motion_busy_ = false;
    });
}

void Motion::moveGripper(double width)
{
    if (!gripper_group_) {
        RCLCPP_ERROR(this->get_logger(), "Gripper group not initialized!");
        return;
    }

    // Clamp to valid range
    width = std::clamp(width, 0.0, 0.1);

    gripper_group_->setJointValueTarget("finger_width", width);

    moveit::planning_interface::MoveGroupInterface::Plan plan;
    bool success = (gripper_group_->plan(plan) ==
        moveit::planning_interface::MoveItErrorCode::SUCCESS);

    if (success) {
        gripper_group_->execute(plan);
    } else {
        RCLCPP_WARN(this->get_logger(), "Gripper planning failed.");
    }
}

void Motion::openGripper()
{
    RCLCPP_INFO(this->get_logger(), "Opening gripper");
    moveGripper(0.1);   // 100mm = fully open
}

void Motion::closeGripper()
{
    RCLCPP_INFO(this->get_logger(), "Closing gripper");
    moveGripper(0.0);   // 0mm = fully closed
}


double Motion::normalizeAngle(double angle) {
    while (angle > M_PI)  angle -= 2.0 * M_PI;
    while (angle < -M_PI) angle += 2.0 * M_PI;
    return angle;
}


// This function will simulate the joint positions updating as the user moves 
// the EE in unity
void Motion::demoMovement(){
    // // joint positions
    // double shoulder_pan_joint = 0;
    // double shoulder_lift_joint = -1.57;
    // double elbow_joint = 0; 
    // double wrist_1_joint = -1.57;
    // double wrist_2_joint = 0;
    // double wrist_3_joint = 0.0;

    //  // Increment each step
    // double increment = 0.05;
    // int steps = 100;

    // for (int i = 0; i < steps; i++)
    // {
    //     // Slightly change each joint each iteration
    //     shoulder_pan_joint += increment;
    //     shoulder_lift_joint += increment * 0.5;
    //     elbow_joint -= increment * 0.3;
    //     wrist_1_joint += increment * 0.2;
    //     wrist_2_joint -= increment * 0.1;
    //     wrist_3_joint += increment * 0.4;

    //     sensor_msgs::msg::JointState msg;
    //     msg.header.stamp = this->get_clock()->now();
    //     msg.name = {
    //         "shoulder_pan_joint",
    //         "shoulder_lift_joint",
    //         "elbow_joint",
    //         "wrist_1_joint",
    //         "wrist_2_joint",
    //         "wrist_3_joint"
    //     };
    //     msg.position = {
    //         shoulder_pan_joint,
    //         shoulder_lift_joint,
    //         elbow_joint,
    //         wrist_1_joint,
    //         wrist_2_joint,
    //         wrist_3_joint
    //     };
    //     jointstatesPub_->publish(msg);
    //     RCLCPP_INFO(this->get_logger(),
    //         "Demo step %d: publishing joints: %.2f %.2f %.2f %.2f %.2f %.2f",
    //         i, shoulder_pan_joint, shoulder_lift_joint, elbow_joint,
    //         wrist_1_joint, wrist_2_joint, wrist_3_joint);

    //     // Wait between steps so the robot has time to reach each position
    //     std::this_thread::sleep_for(std::chrono::milliseconds(500));
    // }
}




#pragma once

#include <rclcpp/rclcpp.hpp>
#include <geometry_msgs/msg/pose_stamped.hpp>
#include <sensor_msgs/msg/joint_state.hpp>
#include <vector>
#include <mutex>
#include <moveit/move_group_interface/move_group_interface.h> // For moving robot
#include "std_msgs/msg/float32.hpp" // For gripper


class Motion : public rclcpp::Node
{
    public:
        Motion();
        ~Motion();

        void initMoveIt(); // Function to intialsie MoveIt

        void targetJointStateCb(const sensor_msgs::msg::JointState::SharedPtr msg); //! Callback to read target joint states 

        double normalizeAngle(double angle);

        void demoMovement(void);    // Movement for demo

        // Gripper control
        void moveGripper(double width);  // width in metres, 0.0=closed, 0.1=open
        void openGripper();
        void closeGripper();

    private:
        rclcpp::Publisher<sensor_msgs::msg::JointState>::SharedPtr jointstatesPub_; // Publish joint states to robot

        // For gripper control
        rclcpp::Subscription<std_msgs::msg::Float32>::SharedPtr subGripperCmd_;
        void gripperCmdCb(const std_msgs::msg::Float32::SharedPtr msg);
        
        rclcpp::TimerBase::SharedPtr timer_;  //!< Timer to trigger periodic publishing of joint states

        rclcpp::Subscription<sensor_msgs::msg::JointState>::SharedPtr subTargetJointStates_; //! Subscribes to target JointStates from Unity (hopefully).

        // Last recieved target EE pose
        //geometry_msgs::msg::PoseStamped last_ee_pose_;
        // geometry_msgs::msg::PoseStamped::SharedPtr last_ee_pose_;

        std::mutex jointstate_mtx_; // guards join + exchange sequence

        std::thread motion_thread_;
        std::atomic<bool> motion_busy_{false};

        std::shared_ptr<moveit::planning_interface::MoveGroupInterface> move_group_;
        // Gripper control
        std::shared_ptr<moveit::planning_interface::MoveGroupInterface> gripper_group_;


};

cmake_minimum_required(VERSION 3.5)
project(motion)

# Default to C++17
if(NOT CMAKE_CXX_STANDARD)
  set(CMAKE_CXX_STANDARD 17)
  set(CMAKE_CXX_STANDARD_REQUIRED ON)
endif()

if(CMAKE_COMPILER_IS_GNUCXX OR CMAKE_CXX_COMPILER_ID MATCHES "Clang")
  add_compile_options(-Wall -Wextra -Wpedantic -Wunused-parameter)
else()
  add_compile_options(-Wunused-parameter)
endif()

## Find packages, remove those that are not needed
find_package(ament_cmake REQUIRED)
find_package(rclcpp REQUIRED)
find_package(std_msgs REQUIRED)
find_package(sensor_msgs REQUIRED)
find_package(geometry_msgs REQUIRED)
find_package(moveit_visual_tools REQUIRED)
find_package(moveit_ros_planning_interface REQUIRED)


# #################################################################
# # The library we are developing will be called laserprocessing
# add_library(laserprocessing src/laserprocessing.cpp)
# ament_target_dependencies(laserprocessing rclcpp std_msgs sensor_msgs nav_msgs)


# ## This is the  motion executable
add_executable(motion src/movement.cpp src/main.cpp)
ament_target_dependencies(motion 
  rclcpp 
  std_msgs 
  sensor_msgs 
  nav_msgs 
  tf2 
  tf2_ros
  tf2_geometry_msgs 
  std_srvs
  visualization_msgs
  geometry_msgs
  moveit_ros_planning_interface
  moveit_visual_tools
)
# target_link_libraries(genPointCloud laserprocessing)

# add_executable(demoMotion src/demo_main.cpp src/testmovement.cpp)
# ament_target_dependencies(demoMotion 
#   rclcpp 
#   nav_msgs
#   sensor_msgs 
#   geometry_msgs 
#   moveit_ros_planning_interface
# )

install(TARGETS
  motion
  # demoMotion
  # publishPCD
  # pathPlan
#   laserprocessing
  DESTINATION lib/${PROJECT_NAME})

install(DIRECTORY 
  launch
  DESTINATION share/${PROJECT_NAME}
)

# install(
#   DIRECTORY

#   DESTINATION share/${PROJECT_NAME}
# )

# if(BUILD_TESTING)

#   ament_add_ros_isolated_gtest(test_laserprocessing test/utest.cpp)
#   target_link_libraries(test_laserprocessing laserprocessing)
#   ament_target_dependencies(test_laserprocessing rclcpp sensor_msgs nav_msgs rosbag2_cpp)

# endif()

ament_package()
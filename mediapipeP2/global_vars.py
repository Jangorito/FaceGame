# Internally used, don't mind this.
KILL_THREADS = False

# Toggle this in order to view how your WebCam is being interpreted (reduces performance).
DEBUG = False 

# Settings do not universally apply, not all WebCams support all frame rates and resolutions
CAM_INDEX = 0
 # OpenCV2 webcam index, try changing for using another (ex: external) webcam.
USE_CUSTOM_CAM_SETTINGS = True
FPS = 60
WIDTH = 320
HEIGHT = 240

# [0, 2] Higher numbers are more precise, but also cost more performance. The demo video used 2 (good environment is more important).
MODEL_COMPLEXITY = 1

PIPE_LINE_DEBUG = False
#if true, pipeline is printed and not opened with a unity project, prints after encoding
# allows you to test on the python side without opening a pipeline to unity (c#)


#allows you to run the mediapipe view only through spout and not natively 
SPOUT_ONLY = False


#turn off spouting = false for no spout
SPOUT_ON = True
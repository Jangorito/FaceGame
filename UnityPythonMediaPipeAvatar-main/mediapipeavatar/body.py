import struct
import cv2
import mediapipe as mp
import threading
import time
import global_vars  #  global_vars module is defined in global_vars.py
import struct

# Define a thread for capturing video frames
class CaptureThread(threading.Thread):
    cap = None
    ret = None
    frame = None
    isRunning = False
    counter = 0
    timer = 0.0

    def run(self):
        # Open a video capture using OpenCV with specified camera index
        self.cap = cv2.VideoCapture(global_vars.CAM_INDEX) 
        #the camera you usually use is index 0 but you can play around with in mediapipeavatar\global_vars.py if you want to use an external cam

        # Apply custom camera settings if specified in global_vars
        if global_vars.USE_CUSTOM_CAM_SETTINGS:
            self.cap.set(cv2.CAP_PROP_FPS, global_vars.FPS)
            self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, global_vars.WIDTH)
            self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, global_vars.HEIGHT)

        # Wait for a short duration to allow the camera to initialize
        time.sleep(1)

        # Print the frames per second of the capture
        print("Opened Capture @ %s fps" % str(self.cap.get(cv2.CAP_PROP_FPS)))

        # Continuously capture frames while the program is running
        while not global_vars.KILL_THREADS:
            self.ret, self.frame = self.cap.read()
            self.isRunning = True

            # Print debug information if debugging is enabled (it is at the moment as were in development)
            if global_vars.DEBUG:
                self.counter = self.counter + 1
                if time.time() - self.timer >= 3:
                    print("Capture FPS: ", self.counter / (time.time() - self.timer))
                    self.counter = 0
                    self.timer = time.time()

# Define a thread for the processing of landmarks
class BodyThread(threading.Thread):
    data = ""
    pipe = None
    timeSinceCheckedConnection = 0
    def run(self):
        # Import necessary modules from the Mediapipe library
        mp_drawing = mp.solutions.drawing_utils
        mp_drawing_styles = mp.solutions.drawing_styles
        mp_holistic = mp.solutions.holistic

        # Start the CaptureThread to capture video frames
        capture = CaptureThread()
        capture.start()

        # Create a Mediapipe Holistic instance for processing body landmarks
        with mp_holistic.Holistic(min_detection_confidence=0.8, min_tracking_confidence=0.5) as holistic:
            # Wait until the camera is running before starting body landmark processing
            while not global_vars.KILL_THREADS and capture.isRunning == False:
                print("Waiting for camera and capture thread.")
                time.sleep(0.5)

            print("Beginning capture")
            print(capture.cap.isOpened())

            # Process body landmarks while the camera is open
            while not global_vars.KILL_THREADS and capture.cap.isOpened():
                image = capture.frame

                # Flip the captured frame horizontally for better visualization
                image = cv2.flip(image, 1)
                image.flags.writeable = False

                # Process body landmarks using MediaPipe Holistic
                results = holistic.process(image)

                image.flags.writeable = True
                image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

                # Draw landmarks on the image
                mp_drawing.draw_landmarks(
                    image,
                    results.face_landmarks,
                    mp_holistic.FACEMESH_CONTOURS,
                    landmark_drawing_spec=None,
                    connection_drawing_spec=mp_drawing_styles.get_default_face_mesh_contours_style())
                mp_drawing.draw_landmarks(
                    image,
                    results.face_landmarks,
                    mp_holistic.FACEMESH_TESSELATION,
                    landmark_drawing_spec=None,
                    connection_drawing_spec=mp_drawing_styles.get_default_face_mesh_tesselation_style())
                mp_drawing.draw_landmarks(
                    image,
                    results.pose_landmarks,
                    mp_holistic.POSE_CONNECTIONS,
                    landmark_drawing_spec=mp_drawing_styles.get_default_pose_landmarks_style())

                # Draw hand landmarks and connections for the left hand
                mp_drawing.draw_landmarks(
                    image,
                    results.left_hand_landmarks,
                    mp_holistic.HAND_CONNECTIONS,
                    landmark_drawing_spec=mp_drawing_styles.get_default_hand_landmarks_style(),
                    connection_drawing_spec=mp_drawing_styles.get_default_hand_connections_style())

                # Draw hand landmarks and connections for the right hand
                mp_drawing.draw_landmarks(
                    image,
                    results.right_hand_landmarks,
                    mp_holistic.HAND_CONNECTIONS,
                    landmark_drawing_spec=mp_drawing_styles.get_default_hand_landmarks_style(),
                    connection_drawing_spec=mp_drawing_styles.get_default_hand_connections_style())


                # Display the annotated image
                cv2.imshow('MediaPipe Holistic', image)

                # Break the loop if the 'Esc' key is pressed
                if cv2.waitKey(5) & 0xFF == 27:
                    break
                # Debugging and communication with Unity project
                print(time.time()- self.timeSinceCheckedConnection)
                if self.pipe == None and time.time() - self.timeSinceCheckedConnection >= 1:
                    try:
                        # Attempt to open a named pipe for communication with Unity
                        self.pipe = open(r'\\.\pipe\UnityMediaPipeBody', 'r+b', 0)
                        print("entered")
                    except FileNotFoundError:
                        print("Waiting for Unity project to run...")
                        self.pipe = None
                    self.timeSinceCheckedConnection = time.time()

                if self.pipe != None:
                    # Set up data for piping
                    self.data = ""
                    i = 0
                    if results.pose_world_landmarks:
                        hand_world_landmarks = results.pose_world_landmarks
                        for i in range(0, 33):
                           print("{}|{}|{}|{}\n".format(i, hand_world_landmarks.landmark[i].x,
                                                                    hand_world_landmarks.landmark[i].y,
                                                                    hand_world_landmarks.landmark[i].z))
                    if  results.face_landmarks:
                        face_landmarks = results.face_landmarks
                        for i in range(0, 468):
                            print("{}|{}|{}|{}".format(i, face_landmarks.landmark[i].x, face_landmarks.landmark[i].y, face_landmarks.landmark[i].z))

                    if results.left_hand_landmarks:
                        left_hand_landmarks = results.left_hand_landmarks
                        for i in range(0, 21):
                            print("Left Hand Landmark {}: x={}, y={}, z={}".format(
                                i, left_hand_landmarks.landmark[i].x,
                                left_hand_landmarks.landmark[i].y,
                                left_hand_landmarks.landmark[i].z))

                    if results.right_hand_landmarks:
                        right_hand_landmarks = results.right_hand_landmarks
                        for i in range(0, 21):
                            print("Right Hand Landmark {}: x={}, y={}, z={}".format(
                                i, right_hand_landmarks.landmark[i].x,
                                right_hand_landmarks.landmark[i].y,
                                right_hand_landmarks.landmark[i].z))


                    # Encode the data and write it to the named pipe
                    s = self.data.encode('utf-8')
                    try:
                        self.pipe.write(struct.pack('I', len(s)) + s)
                        self.pipe.seek(0)
                    except Exception as ex:
                        print("Failed to write to pipe. Is the unity project open?")
                        self.pipe = None

            # Close the named pipe and destroy OpenCV windows
        self.pipe.close()
        # Release the video capture when done
        capture.cap.release()
        cv2.destroyAllWindows()

# End of the code
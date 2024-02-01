import struct
import cv2
import mediapipe as mp
import threading
import time
import global_vars  #  global_vars module is defined in global_vars.py
import struct
from mediapipe.python.solutions.pose import PoseLandmark
from mediapipe.python.solutions.drawing_utils import DrawingSpec

custom_style =""

POSE_CONNECTIONS = frozenset([(0, 1), (1, 2), (2, 3), (3, 7), (0, 4), (4, 5),
                              (5, 6), (6, 8), (9, 10), (11, 12), (11, 13),
                              (13, 15), (12, 14), (14, 16), (11, 23), (12, 24), (23, 24), (23, 25),
                              (24, 26), (25, 27), (26, 28), (27, 29), (28, 30),
                              (29, 31), (30, 32), (27, 31), (28, 32)])

FACE_CONNECTIONS = frozenset([(10, 338), (338, 297), (297, 332), (332, 284),
                                (284, 251), (251, 389), (389, 356), (356, 454),
                                (454, 323), (323, 361), (361, 288), (288, 397),
                                (397, 365), (365, 379), (379, 378), (378, 400),
                                (400, 377), (377, 152), (152, 148), (148, 176),
                                (176, 149), (149, 150), (150, 136), (136, 172),
                                (172, 58), (58, 132), (132, 93), (93, 234),
                                (234, 127), (127, 162), (162, 21), (21, 54),
                                (54, 103), (103, 67), (67, 109), (109, 10)])

custom_connections = list(POSE_CONNECTIONS)
custom_face_connections = list(FACE_CONNECTIONS)

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
        self.ApplyCameraSettings()

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

    def ApplyCameraSettings(self):
        if global_vars.USE_CUSTOM_CAM_SETTINGS:
            self.cap.set(cv2.CAP_PROP_FPS, global_vars.FPS)
            self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, global_vars.WIDTH)
            self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, global_vars.HEIGHT)

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
        self.CustomLandmarksAndConnections(mp_drawing_styles)

        # Start the CaptureThread to capture video frames
        capture = CaptureThread()
        capture.start()

        # Create a Mediapipe Holistic instance for processing body landmarks
        with mp_holistic.Holistic(min_detection_confidence=0.8, min_tracking_confidence=0.5) as holistic:
            # Wait until the camera is running before starting body landmark processing
            self.WaitForCamera(capture)

            print("Beginning capture")
            print(capture.cap.isOpened())

            # Process body landmarks while the camera is open
            while not global_vars.KILL_THREADS and capture.cap.isOpened():
                image = capture.frame

                # Flip the captured frame horizontally for better visualization
                image = cv2.flip(image, 1)
                image.flags.writeable = False

                results = holistic.process(image)
                self.RemoveHandLandmarks(mp_holistic, results)
                self.RemovePoseFaceLandmarks(mp_holistic, results)
                image.flags.writeable = True
                image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

                # Draw landmarks on the image
                self.DrawFaceLandmarks(mp_drawing, mp_drawing_styles, mp_holistic, image, results)
                #self.DrawFaceTesselation(mp_drawing, mp_drawing_styles, mp_holistic, image, results)
                self.DrawPoseLandmarks(mp_drawing, mp_drawing_styles, image, results)
                self.DrawCustomPoseLandmarks(mp_drawing, mp_drawing_styles, mp_holistic, image, results)
                self.DrawLeftHandLandmarksAndConnections(mp_drawing, mp_drawing_styles, mp_holistic, image, results)
                self.DrawRightHandLandmarksAndConnections(mp_drawing, mp_drawing_styles, mp_holistic, image, results)


                # Display the annotated image
                cv2.imshow('MediaPipe Holistic', image)

                # Break the loop if the 'Esc' key is pressed
                if cv2.waitKey(5) & 0xFF == 27:
                    break
                # Debugging and communication with Unity project
                print(time.time()- self.timeSinceCheckedConnection)
                if self.pipe == None and time.time() - self.timeSinceCheckedConnection >= 1:
                    self.OpenNamedPipe()

                if self.pipe != None:
                    # Set up data for piping
                    self.data = ""
                    i = 0
                    self.CollatePoseLandmarks(results)
                    self.CollateFaceLandmarks(results)
                    self.CollateLeftHandLandmarks(results)
                    self.CollateRightHandLandmarks(results)
                    # Encode the data and write it to the named pipe
                    s = self.data.encode('utf-8')
                    self.SendDataOverPipe(s)

            # Close the named pipe and destroy OpenCV windows
        self.pipe.close()
        # Release the video capture when done
        capture.cap.release()
        cv2.destroyAllWindows()

    def RemoveHandLandmarks(self, mp_holistic, results):
        if results.pose_landmarks:
            #results.pose_landmarks.landmark[mp_holistic.PoseLandmark.LEFT_WRIST].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.LEFT_PINKY].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.LEFT_THUMB].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.LEFT_INDEX].visibility = 0.0
            #results.pose_landmarks.landmark[mp_holistic.PoseLandmark.RIGHT_WRIST].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.RIGHT_PINKY].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.RIGHT_THUMB].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.RIGHT_INDEX].visibility = 0.0

    def RemovePoseFaceLandmarks(self, mp_holistic, results):
        if results.pose_landmarks:
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.NOSE].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.LEFT_EYE_INNER].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.LEFT_EYE].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.LEFT_EYE_OUTER].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.RIGHT_EYE_INNER].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.RIGHT_EYE].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.RIGHT_EYE_OUTER].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.LEFT_EAR].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.RIGHT_EAR].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.MOUTH_LEFT].visibility = 0.0
            results.pose_landmarks.landmark[mp_holistic.PoseLandmark.MOUTH_RIGHT].visibility = 0.0

            # results.pose_landmarks.landmark[mp_holistic.PoseLandmark.LEFT_WRIST].ClearField("x")
            # results.pose_landmarks.landmark[mp_holistic.PoseLandmark.LEFT_WRIST].ClearField("y")
            # results.pose_landmarks.landmark[mp_holistic.PoseLandmark.LEFT_WRIST].ClearField("z")
            
    def CustomLandmarksAndConnections(self, mp_drawing_styles):
        hand_landmarks = mp_drawing_styles.get_default_hand_landmarks_style()
        custom_style = mp_drawing_styles.get_default_pose_landmarks_style()

        # Get the hand landmark indices to be removed
        hand_landmark_indices = [landmark.value for landmark in hand_landmarks]

        # Remove hand landmarks from custom style
        custom_style = {landmark: style for landmark, style in custom_style.items() if landmark not in hand_landmark_indices}

    def WaitForCamera(self, capture):
        while not global_vars.KILL_THREADS and capture.isRunning == False:
            print("Waiting for camera and capture thread.")
            time.sleep(0.5)

    def SendDataOverPipe(self, s):
        try:
            self.pipe.write(struct.pack('I', len(s)) + s)
            self.pipe.seek(0)
        except Exception as ex:
            print("Failed to write to pipe. Is the unity project open?")
            self.pipe = None

    def OpenNamedPipe(self):
        try:
                        # Attempt to open a named pipe for communication with Unity
            self.pipe = open(r'\\.\pipe\UnityMediaPipeBody', 'r+b', 0)
            print("entered")
        except FileNotFoundError:
            print("Waiting for Unity project to run...")
            self.pipe = None
        self.timeSinceCheckedConnection = time.time()

    def CollateRightHandLandmarks(self, results):
        if results.right_hand_landmarks:
            right_hand_landmarks = results.right_hand_landmarks
            for i in range(0, 21):
                 self.data +=("{}|{}|{}|{}".format(
                                i, right_hand_landmarks.landmark[i].x,
                                right_hand_landmarks.landmark[i].y,
                                right_hand_landmarks.landmark[i].z))

    def CollateLeftHandLandmarks(self, results):
        if results.left_hand_landmarks:
            left_hand_landmarks = results.left_hand_landmarks
            for i in range(0, 21):
                self.data +=("{}|{}|{}|{}".format(
                                i, left_hand_landmarks.landmark[i].x,
                                left_hand_landmarks.landmark[i].y,
                                left_hand_landmarks.landmark[i].z))

    def CollateFaceLandmarks(self, results):
        if  results.face_landmarks:
            face_landmarks = results.face_landmarks
            for i in range(0, 468):
                 self.data +=("{}|{}|{}|{}".format(i, face_landmarks.landmark[i].x, face_landmarks.landmark[i].y, face_landmarks.landmark[i].z))

    def CollatePoseLandmarks(self, results):
        if results.pose_world_landmarks:
            hand_world_landmarks = results.pose_world_landmarks
            for i in range(0, 33):
               if i in (list(range(0, 10)) + list(range(17, 22))): ## Removes pose face landmarks and hands apart from wrist landmark
                   continue
               self.data +=("{}|{}|{}|{}\n".format(i, hand_world_landmarks.landmark[i].x,
                                                                    hand_world_landmarks.landmark[i].y,
                                                                    hand_world_landmarks.landmark[i].z))

    def DrawRightHandLandmarksAndConnections(self, mp_drawing, mp_drawing_styles, mp_holistic, image, results):
        mp_drawing.draw_landmarks(
                    image,
                    results.right_hand_landmarks,
                    mp_holistic.HAND_CONNECTIONS,
                    landmark_drawing_spec=mp_drawing_styles.get_default_hand_landmarks_style(),
                    connection_drawing_spec=mp_drawing_styles.get_default_hand_connections_style())

    def DrawLeftHandLandmarksAndConnections(self, mp_drawing, mp_drawing_styles, mp_holistic, image, results):
        mp_drawing.draw_landmarks(
                    image,
                    results.left_hand_landmarks,
                    mp_holistic.HAND_CONNECTIONS,
                    landmark_drawing_spec=mp_drawing_styles.get_default_hand_landmarks_style(),
                    connection_drawing_spec=mp_drawing_styles.get_default_hand_connections_style())

    def DrawPoseLandmarks(self, mp_drawing, mp_drawing_styles, image, results):
        mp_drawing.draw_landmarks(
                    image,
                    results.pose_landmarks,
                    #mp_holistic.POSE_CONNECTIONS,
                    #landmark_drawing_spec=mp_drawing_styles.get_default_pose_landmarks_style())
                    connections = custom_connections,
                    #landmark_drawing_spec=custom_style)
                    landmark_drawing_spec=mp_drawing_styles.get_default_pose_landmarks_style())
        
    def DrawCustomPoseLandmarks(self, mp_drawing, mp_drawing_styles, mp_holistic, image, results): #doesnt work atm
        custom_connections2 = [
        # Custom connection from left elbow to left wrist
        (mp_holistic.PoseLandmark.LEFT_ELBOW, mp_holistic.HandLandmark.WRIST),
        # Custom connection from right elbow to right wrist
        (mp_holistic.PoseLandmark.RIGHT_ELBOW, mp_holistic.HandLandmark.WRIST)
        ]
        mp_drawing.draw_landmarks(
                        image,
                        results.pose_landmarks,
                        connections = custom_connections2,
                        #landmark_drawing_spec=mp_drawing_styles.get_default_pose_landmarks_style(),
                        connection_drawing_spec=mp_drawing_styles.get_default_hand_connections_style())

    def DrawFaceTesselation(self, mp_drawing, mp_drawing_styles, mp_holistic, image, results):
        mp_drawing.draw_landmarks(
                    image,
                    results.face_landmarks,
                    mp_holistic.FACEMESH_TESSELATION,
                    landmark_drawing_spec=None,
                    connection_drawing_spec=mp_drawing_styles.get_default_face_mesh_tesselation_style())

    def DrawFaceLandmarks(self, mp_drawing, mp_drawing_styles, mp_holistic, image, results):
        drawing_spec = mp_drawing.DrawingSpec(color = (0, 0, 255), thickness = 1, circle_radius = 1)
        mp_drawing.draw_landmarks(
                    image,
                    results.face_landmarks,
                    #mp_holistic.FACEMESH_CONTOURS,
                    connections = custom_face_connections,
                    landmark_drawing_spec=drawing_spec)
                    #connection_drawing_spec=mp_drawing_styles.get_default_face_mesh_contours_style())

# End of the code
import cv2 #image facilities
import mediapipe as mp #mediapipe for tracking
import threading #camera thread
import time #for fps ect
import global_vars  #  global_vars module is defined in global_vars.py
from pythonosc.udp_client import SimpleUDPClient #to set up and use udp clients for sending OSC data
import socket #to connect to an IP address

ipAddress = ""

def get_ip_address():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.connect(("8.8.8.8", 80))
    return s.getsockname()[0]


print(get_ip_address())

custom_style =""
TARGET_FPS = 1
SEND_WIDTH = global_vars.WIDTH
SEND_HEIGHT = global_vars.HEIGHT
SENDER_NAME = "SpoutGL-test"
POSE_CONNECTIONS = frozenset([(0, 1), (1, 2), (2, 3), (3, 7), (0, 4), (4, 5),
                              (5, 6), (6, 8), (9, 10), (11, 12), (11, 13),
                              (13, 15), (12, 14), (14, 16), (11, 23), (12, 24), (23, 24), (23, 25),
                              (24, 26), (25, 27), (26, 28), (27, 29), (28, 30),
                              (29, 31), (30, 32), (27, 31), (28, 32)])

FACEMESH_LIPS = frozenset([(0, 39), (39, 61), (0, 269), (269, 291),
                            (13, 81), (81, 78), (13, 311), (311, 308),
                            (14, 178), (178, 78), (14, 402), (402, 308),
                            (17, 181), (181, 61), (17, 405), (405, 291)])
# 40 -> 16
#outer lip clockwise from the middle
#0 = LIPS_OUTER_1
#269 = LIPS_OUTER_2
#291 = LIPS_OUTER_3
#405 = LIPS_OUTER_4
#17 = LIPS_OUTER_5
#181 = LIPS_OUTER_6
#61 = LIPS_OUTER_7
#39 = LIPS_OUTER_8


#inner lip clockwise from the middle
#13 = LIPS_INNER_1
#311 = LIPS_INNER_2
#308 = LIPS_INNER_3
#402 = LIPS_INNER_4
#14 = LIPS_INNER_5
#178 = LIPS_INNER_6
#78 = LIPS_INNER_7
#81 = LIPS_INNER_8


FACEMESH_LEFT_EYE = frozenset([(263, 390), (390, 374), (374, 381), (381, 362),
                                (263, 388), (388, 386), (386, 384), (384, 362)])
# # 16 -> 8

FACEMESH_LEFT_EYEBROW = frozenset([(276, 282), (282, 285), (336, 334), (334, 300)])
# 8 -> 4

# 276 = LEFT_LOWER_EYEBROW_1
# 282 = LEFT_LOWER_EYEBROW_2
# 285 = LEFT_LOWER_EYEBROW_3
# 336 = LEFT_UPPER_EYEBROW_1
# 334 = LEFT_UPPER_EYEBROW_2
# 300 = LEFT_UPPER_EYEBROW_3

FACEMESH_RIGHT_EYE = frozenset([(33, 163), (163, 145), (145, 154), (154, 133),
                                (33, 161), (161, 159), (159, 157), (157, 133)])
# # 16 -> 8

FACEMESH_RIGHT_EYEBROW = frozenset([(46, 52), (52, 55), (70, 105), (105, 107)])
# 8 -> 4

# 46 = RIGHT_LOWER_EYEBROW_1
# 52 = RIGHT_LOWER_EYEBROW_2
# 55 = RIGHT_LOWER_EYEBROW_3
# 70 = RIGHT_UPPER_EYEBROW_1
# 105 = RIGHT_UPPER_EYEBROW_2
# 107 = RIGHT_UPPER_EYEBROW_3

FACEMESH_FACE_OVAL = frozenset([(10, 297), (297, 284), (284, 389), (389, 454),
                                (454, 361), (361, 397), (397, 379), (379, 400),
                                (400, 152), (152, 176), (176, 150), (150, 172),
                                (172, 132), (132, 234),  (234, 162), (162, 54), 
                                (54, 67), (67, 10)])
#face oval goes round clockwise from the top

# 10 FACE_OVAL_1

# 297 FACE_OVAL_2

# 284 FACE_OVAL_3

# 389 FACE_OVAL_4

# 454 FACE_OVAL_5

# 361 FACE_OVAL_6

# 397 FACE_OVAL_7

# 379 FACE_OVAL_8

# 400 FACE_OVAL_9

# 152 FACE_OVAL_10

# 176 FACE_OVAL_11

# 150 FACE_OVAL_12

# 172 FACE_OVAL_13

# 132 FACE_OVAL_14

# 234 FACE_OVAL_15

# 162 FACE_OVAL_16

# 54 FACE_OVAL_17

# 67 FACE_OVAL_18

FACE_CONNECTIONS = frozenset().union(*[
    FACEMESH_LIPS, FACEMESH_LEFT_EYE, FACEMESH_LEFT_EYEBROW, FACEMESH_RIGHT_EYE,
    FACEMESH_RIGHT_EYEBROW, FACEMESH_FACE_OVAL
])
# 17 + 8 + 4 + 8 + 4 +16 = 57 landmarks for the face

custom_connections = list(POSE_CONNECTIONS)
custom_face_connections = list(FACE_CONNECTIONS)

custom_face_landmarks = []

for tuple in custom_face_connections:
    int1, int2 = tuple
    custom_face_landmarks.extend([int1, int2])

custom_face_landmarks = list(set(custom_face_landmarks))



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
        #self.sendWebCam()
        # Wait for a short duration to allow the camera to initialize
        time.sleep(1)

        # Print the frames per second of the capture
        if global_vars.DEBUG:
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
    client = None
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
        with mp_holistic.Holistic(min_detection_confidence=0.8, min_tracking_confidence=0.5, refine_face_landmarks = False) as holistic:
            # Wait until the camera is running before starting body landmark processing
            self.WaitForCamera(capture)
            if global_vars.DEBUG:
                print("Beginning capture")

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
                # Draw landmarks on the image
                self.DrawFaceLandmarks(mp_drawing, mp_drawing_styles, mp_holistic, image, results)
                #self.DrawFaceTesselation(mp_drawing, mp_drawing_styles, mp_holistic, image, results)
                self.DrawPoseLandmarks(mp_drawing, mp_drawing_styles, image, results)
                self.DrawCustomPoseLandmarks(mp_drawing, mp_drawing_styles, mp_holistic, image, results)
                self.DrawLeftHandLandmarksAndConnections(mp_drawing, mp_drawing_styles, mp_holistic, image, results)
                self.DrawRightHandLandmarksAndConnections(mp_drawing, mp_drawing_styles, mp_holistic, image, results)
                # Display the annotated image
                if global_vars.SPOUT_ON == True:
                    self.send(image)
                if global_vars.SPOUT_ONLY == False:
                    cv2.imshow('MediaPipe Holistic', image)
                # Break the loop if the 'Esc' key is pressed
                if cv2.waitKey(5) & 0xFF == 27:
                    break

                # Debugging and communication with Unity project
                if global_vars.DEBUG:
                    print(time.time() - self.timeSinceCheckedConnection)

                if self.client is None and time.time() - self.timeSinceCheckedConnection >= 1 and not global_vars.PIPE_LINE_DEBUG:
                    port = 5012
                    self.client = SimpleUDPClient(ipAddress, port)  # Create client

                if self.client is not None or global_vars.PIPE_LINE_DEBUG:
                    # Set up data for OSC messaging
                    self.data = ""
                    self.CollatePoseLandmarks(results)
                    self.CollateFaceLandmarks(results)
                    self.CollateLeftHandLandmarks(results)
                    self.CollateRightHandLandmarks(results)
                    if self.data:
                        #s = self.data.encode('utf-8')
                        s = self.data.encode('utf-8')
                        self.client.send_message("/PythonData", s)   # Send OSC message
                        #print(s)
                    if global_vars.DEBUG:
                        print("Data is empty. Skipping sending OSC message.")
                    # s = self.data.encode('utf-8')
                    # self.client.send_message("/PythonData", s)   # Send OSC message

            # Close the named pipe and destroy OpenCV windows
        #self.pipe.close()
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

    # def SendDataOverPipe(self, s):
    #     try:
    #         self.pipe.write(struct.pack('I', len(s)) + s)
    #         self.pipe.seek(0)
    #     except Exception as ex:
    #         print("Failed to write to pipe. Is the unity project open?")
    #         self.pipe = None

    # def OpenNamedPipe(self):
    #     try:
    #                     # Attempt to open a named pipe for communication with Unity
    #         self.pipe = open(r'\\.\pipe\UnityMediaPipeBody', 'r+b', 0)
    #         print("entered")
    #     except FileNotFoundError:
    #         print("Waiting for Unity project to run...")
    #         self.pipe = None
    #     self.timeSinceCheckedConnection = time.time()

    def CollateRightHandLandmarks(self, results):
        if results.right_hand_landmarks:
            right_hand_landmarks = results.right_hand_landmarks
            for i in range(0, 21):
                 self.data +=("{}|{}|{}|{}|{}\n".format(
                                "RH", i, right_hand_landmarks.landmark[i].x,
                                right_hand_landmarks.landmark[i].y,
                                right_hand_landmarks.landmark[i].z))
                                    #RH
    def CollateLeftHandLandmarks(self, results):
        if results.left_hand_landmarks:
            left_hand_landmarks = results.left_hand_landmarks
            for i in range(0, 21):
                self.data +=("{}|{}|{}|{}|{}\n".format(
                                "LH", i, left_hand_landmarks.landmark[i].x,
                                left_hand_landmarks.landmark[i].y,
                                left_hand_landmarks.landmark[i].z))
                                    # LH = Left Hand
    def CollateFaceLandmarks(self, results):
        if  results.face_landmarks:
            face_landmarks = results.face_landmarks
            for i in range(0, 468):
                 if i not in custom_face_landmarks:
                    continue
                 self.data +=("{}|{}|{}|{}|{}\n".format("FL", i, face_landmarks.landmark[i].x, face_landmarks.landmark[i].y, face_landmarks.landmark[i].z))
                                    # FL = Face Landmarks
    def CollatePoseLandmarks(self, results):
        if results.pose_world_landmarks:
            hand_world_landmarks = results.pose_world_landmarks
            for i in range(0, 33):
               if i in (list(range(0, 11)) + list(range(17, 23))): ## Removes pose face landmarks and hands apart from wrist landmark
                   continue
               self.data +=("{}|{}|{}|{}|{}\n".format("PL", i, hand_world_landmarks.landmark[i].x, # PL = Pose Landmarks
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

        if  results.face_landmarks:
            face_landmarks = results.face_landmarks

            for i in range(0, 468):
                if i in custom_face_landmarks:
                    continue
                face_landmarks.landmark[i].x = 0
                face_landmarks.landmark[i].y = 0
                face_landmarks.landmark[i].z = 0

            mp_drawing.draw_landmarks(
                    image,
                    results.face_landmarks,
                    connections = custom_face_connections,
                    landmark_drawing_spec=drawing_spec)
    



    def send(self, image):
        # Set up OSC client
        client = SimpleUDPClient(ipAddress, 5010)  # OSC server address and port

        # Convert the image to bytes
        retval, buffer = cv2.imencode('.jpg', image)

        # Check if image encoding was successful
        if not retval:
            print("Error: Failed to encode the image.")
            return

        # Convert the image buffer to bytes
        data = buffer.tobytes()

        try:
            # Send the image bytes via OSC
            client.send_message("/video", data)
            print("Image sent successfully via OSC.")
        except Exception as e:
            print("Error: Failed to send image via OSC:", str(e))
    # def send(self, image):
    #     # Set up OSC client
    #     #client = SimpleUDPClcient("192.168.0.115", 12345)  # OSC server address and port


    #     # Convert the image to bytes
    #     retval, buffer = cv2.imencode('.jpg', image)

    #     # Check if image encoding was successful
    #     if not retval:
    #         print("Error: Failed to encode the image.")
    #         return

    #     # Convert the image buffer to bytes
    #     data = buffer.tobytes()

    #     try:
    #         # Send the image bytes via OSC
    #         print("Image sent successfully via OSC.")



    #         sender_socket.send(data)
    #     except Exception as e:
    #         print("Error: Failed to send image via OSC:", str(e))









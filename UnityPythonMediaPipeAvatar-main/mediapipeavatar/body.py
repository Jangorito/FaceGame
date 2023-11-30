import cv2
import mediapipe as mp
import threading
import time
import global_vars
import struct
 
class CaptureThread(threading.Thread):
    cap = None
    ret = None
    frame = None
    isRunning = False
    counter = 0
    timer = 0.0
    def run(self): #run this thread
        self.cap = cv2.VideoCapture(global_vars.CAM_INDEX) # sometimes it can take a while for certain video captures
        if global_vars.USE_CUSTOM_CAM_SETTINGS:
            self.cap.set(cv2.CAP_PROP_FPS, global_vars.FPS)
            self.cap.set(cv2.CAP_PROP_FRAME_WIDTH,global_vars.WIDTH)
            self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT,global_vars.HEIGHT)
 
        time.sleep(1)
       
        print("Opened Capture @ %s fps"%str(self.cap.get(cv2.CAP_PROP_FPS)))
        while not global_vars.KILL_THREADS:
            self.ret, self.frame = self.cap.read()
            self.isRunning = True
            if global_vars.DEBUG:
                self.counter = self.counter+1
                if time.time()-self.timer>=3:
                    print("Capture FPS: ",self.counter/(time.time()-self.timer))
                    self.counter = 0
                    self.timer = time.time()
 
# For webcam input:
class BodyThread(threading.Thread):
 
  data = ""
  dirty = True
  pipe = None
  timeSinceCheckedConnection = 0
  timeSincePostStatistics = 0
  def run(self):
    mp_drawing = mp.solutions.drawing_utils
    mp_drawing_styles = mp.solutions.drawing_styles
    mp_holistic = mp.solutions.holistic
 
    capture = CaptureThread()
    capture.start()
 
    with mp_holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
        while not global_vars.KILL_THREADS and capture.isRunning==False:
                print("Waiting for camera and capture thread.")
                time.sleep(0.5)
        print("Beginning capture")
        print(capture.cap.isOpened())
        while not global_vars.KILL_THREADS and capture.cap.isOpened():
            image = capture.frame
           
            print("test")
 
            # To improve performance, optionally mark the image as not writeable to
            # pass by reference.
            image = cv2.flip(image, 1)
            image.flags.writeable = False
            #image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
 
            results = holistic.process(image)
 
            # Draw landmark annotation on the image.
            image.flags.writeable = True
            image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
            print("test1")
 
            mp_drawing.draw_landmarks(
                image,
                results.face_landmarks,
                mp_holistic.FACEMESH_CONTOURS,
                landmark_drawing_spec=None,
                connection_drawing_spec=mp_drawing_styles
                .get_default_face_mesh_contours_style())
            mp_drawing.draw_landmarks(
                image,
                results.pose_landmarks,
                mp_holistic.POSE_CONNECTIONS,
                landmark_drawing_spec=mp_drawing_styles
                .get_default_pose_landmarks_style())
            mp_drawing.draw_landmarks(
                image,
                results.left_hand_landmarks,
                mp_holistic.HAND_CONNECTIONS,
                landmark_drawing_spec=mp_drawing_styles
                .get_default_pose_landmarks_style())
            mp_drawing.draw_landmarks(
                image,
                results.right_hand_landmarks,
                mp_holistic.HAND_CONNECTIONS,
                landmark_drawing_spec=mp_drawing_styles
                .get_default_pose_landmarks_style())
            # Flip the image horizontally for a selfie-view display.
            print("test2")
            cv2.imshow('MediaPipe Holistic', image)
            if cv2.waitKey(5) & 0xFF == 27:
                break
    #this part needs debugging
    if self.pipe==None and time.time()-self.timeSinceCheckedConnection>=1:
        try:
            self.pipe = open(r'\\.\pipe\UnityMediaPipeBody', 'r+b', 0)
        except FileNotFoundError:
            print("Waiting for Unity project to run...")
            self.pipe = None
        self.timeSinceCheckedConnection = time.time()
       
    if self.pipe != None:
        # Set up data for piping
        self.dataHand = ""
        i = 0
        if results.pose_world_landmarks:
            hand_world_landmarks = results.pose_world_landmarks
            face_landmarks = results.face_landmarks
            left_landmarks = results.left_hand_landmarks
            right_landmarks = results.left_hand_landmarks
            for i in range(0,33):
                self.dataHand += "{}|{}|{}|{}\n".format(i,hand_world_landmarks.landmark[i].x,hand_world_landmarks.landmark[i].y,hand_world_landmarks.landmark[i].z)
            for i in range (0, 468):
                self.data += "{}|{}|{}|{}\n".format(i,face_landmarks.landmark[i].x,face_landmarks.landmark[i].y,face_landmarks.landmark[i].z)
            for i in range (0, 21):
                self.data += "{}|{}|{}|{}\n".format(i,left_landmarks.landmark[i].x,left_landmarks.landmark[i].y,left_landmarks.landmark[i].z)
            for i in range (0, 21):
                self.data += "{}|{}|{}|{}\n".format(i,right_landmarks.landmark[i].x,right_landmarks.landmark[i].y,right_landmarks.landmark[i].z)
 
        s = self.data.encode('utf-8')
        try:    
            self.pipe.write(struct.pack('I', len(s)) + s)  
            self.pipe.seek(0)    
        except Exception as ex:  
            print("Failed to write to pipe. Is the unity project open?")
            self.pipe= None
           
                           
                    #time.sleep(1/20)
                           
            self.pipe.close()
            cv2.destroyAllWindows()
 
    capture.cap.release()
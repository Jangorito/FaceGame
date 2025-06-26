import cv2
import mediapipe as mp
import time
import numpy as np
from threading import Lock
from mediapipe import solutions
from mediapipe.framework.formats import landmark_pb2
from pythonosc import udp_client

# --- Part 1: OSC and MediaPipe Setup ---

print("Starting MediaPipe FaceLandmarker with OSC...")
# Lock and a global variable to store the latest result
lock = Lock()
latest_result = None

# OSC setup
OSC_IP = "127.0.0.1"
OSC_PORT = 9000
OSC_ADDRESS = "/FaceData"
OSC_BLEND_ADDRESS = "/FaceBlendshapes"
client = udp_client.SimpleUDPClient(OSC_IP, OSC_PORT)

# "callback" function that stores and handles results from MediaPipe
def process_result(result, output_image: mp.Image, timestamp_ms: int):

    # store the result in a global variable that is thread-safe due to the lock
    global latest_result
    with lock:
        latest_result = result


    # If face landmarks are found, send them via OSC
    if result.face_landmarks:
        # Send landmarks
        landmarks = result.face_landmarks[0]
        landmark_strs = [f"{i},{lm.x:.5f},{lm.y:.5f},{lm.z:.5f}" for i, lm in enumerate(landmarks)]
        data = "|".join(landmark_strs)
        client.send_message(OSC_ADDRESS, data)

        # Send blendshapes
        if result.face_blendshapes:
            blendshapes = result.face_blendshapes[0]
            blendshape_strs = [f"{b.category_name},{b.score:.5f}" for b in blendshapes]
            blend_data = "|".join(blendshape_strs)
            client.send_message(OSC_BLEND_ADDRESS, blend_data)

# Draw landmarks on the output image
def draw_landmarks_on_frame(frame, detection_result):
    """function that takes a cv2 frame and a detection result, and draws the landmarks on the frame."""
    
    if detection_result is None or not detection_result.face_landmarks:
        return frame

    # Create a copy of the frame to annotate
    annotated_image = frame.copy()
    # Convert the frame to RGB for MediaPipe processing
    face_landmarks_list = detection_result.face_landmarks
    
    # Loop through the detected faces to visualize.
    for face_landmarks in face_landmarks_list:

        # Convert the landmarks to a list of NormalizedLandmark objects which drawing_utils can use
        face_landmarks_proto = landmark_pb2.NormalizedLandmarkList()
        face_landmarks_proto.landmark.extend([
            landmark_pb2.NormalizedLandmark(x=landmark.x, y=landmark.y, z=landmark.z) for landmark in face_landmarks
        ]) 

        # Draw the face mesh tesselation
        solutions.drawing_utils.draw_landmarks(
            image=annotated_image,
            landmark_list=face_landmarks_proto,
            connections=mp.solutions.face_mesh.FACEMESH_TESSELATION,
            landmark_drawing_spec=None,
            connection_drawing_spec=mp.solutions.drawing_styles.get_default_face_mesh_tesselation_style())

        # Draw the face contours
        solutions.drawing_utils.draw_landmarks(
            image=annotated_image,
            landmark_list=face_landmarks_proto,
            connections=mp.solutions.face_mesh.FACEMESH_CONTOURS,
            landmark_drawing_spec=None,
            connection_drawing_spec=mp.solutions.drawing_styles.get_default_face_mesh_contours_style())
        
        # Draw the iris landmarks
        solutions.drawing_utils.draw_landmarks(
            image=annotated_image,
            landmark_list=face_landmarks_proto,
            connections=mp.solutions.face_mesh.FACEMESH_IRISES,
            landmark_drawing_spec=None,
            connection_drawing_spec=mp.solutions.drawing_styles.get_default_face_mesh_iris_connections_style())

    return annotated_image

def wait_for_camera(cap, timeout=10):
    print("Waiting for camera to open...")
    """Waits until the camera is opened or until timeout (in seconds) is reached."""
    start_time = time.time()
    while not cap.isOpened():
        print("Waiting for camera to open...")
        time.sleep(0.5)
        if time.time() - start_time > timeout:
            raise RuntimeError("Camera failed to open within timeout.")
    print("Camera is open and ready.")

# MediaPipe FaceLandmarker setup

print("Setting up MediaPipe FaceLandmarker...")

model_path = "face_landmarker.task"

print (f"Using model at: {model_path}, if blank model_path is broken")

try:
    BaseOptions = mp.tasks.BaseOptions
    FaceLandmarker = mp.tasks.vision.FaceLandmarker
    FaceLandmarkerOptions = mp.tasks.vision.FaceLandmarkerOptions
    VisionRunningMode = mp.tasks.vision.RunningMode
except Exception as e:
    print("FAILED DURING IMPORT:", e)
    import sys
    sys.exit(1)

options = FaceLandmarkerOptions(
    base_options=BaseOptions(model_asset_path=model_path),
    output_face_blendshapes=True,
    output_facial_transformation_matrixes=True,
    num_faces=1,
    running_mode=VisionRunningMode.LIVE_STREAM, # CRITICAL: Use LIVE_STREAM for video
    result_callback=process_result # Tell MediaPipe to call our function with the results
)

# --- Part 2: Main Loop ---
print("Before creating landmarker")
try:
    with FaceLandmarker.create_from_options(options) as landmarker:
        print("FaceLandmarker initialized successfully.")
        cap = cv2.VideoCapture(0)
        wait_for_camera(cap)
        frame_timestamp_ms = 0

        try:
            while cap.isOpened():
                ret, frame = cap.read()
                if not ret:
                    break

                # Flip the frame horizontally for a mirror effect
                frame = cv2.flip(frame, 1)

                # Convert the frame to a MediaPipe Image object.
                mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
                
                # Calculate the timestamp for the current frame
                frame_timestamp_ms = int(time.time() * 1000)

                # Call detect_async to process the frame. The result will be sent to our 'process_result' function.
                landmarker.detect_async(mp_image, frame_timestamp_ms)

                # Create a local copy of the frame to draw on
                annotated_frame = frame.copy()
                
                # Get the latest result from the callback
                with lock:
                    if latest_result is not None:
                        # Draw the landmarks on the frame
                        annotated_frame = draw_landmarks_on_frame(annotated_frame, latest_result)

                # Display the frame for debugging
                cv2.imshow("FaceLandmarker", annotated_frame)
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    break

        finally:
            cap.release()
            cv2.destroyAllWindows()
except Exception as e:
    print("Failed to initialize FaceLandmarker:", e)
import cv2
import mediapipe as mp
import time
import numpy as np
from threading import Lock
from mediapipe import solutions
from mediapipe.framework.formats import landmark_pb2
from pythonosc import udp_client
import os

# --- OSC and MediaPipe Setup ---

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

# Flags to control printing of landmarks and blendshapes
printed_landmarks = False
printed_blendshapes = False

# "callback" function that stores and handles results from MediaPipe
def process_result(result, output_image: mp.Image, timestamp_ms: int):
    # store the result in a global variable that is thread-safe due to the lock
    global latest_result, printed_landmarks, printed_blendshapes

    with lock:
        latest_result = result

    # Map MediaPipe blendshapes to avatar blendshapes
    avatar_blendshapes = map_mediapipe_to_avatar(result)

    # If avatar blendshapes are found, send them via OSC
    if avatar_blendshapes:
        blendshape_strs = [f"{name},{value:.2f}" for name, value in avatar_blendshapes.items()]
        blend_data = "|".join(blendshape_strs)
        client.send_message(OSC_BLEND_ADDRESS, blend_data)

    # # If face landmarks are found, send them via OSC
    # if result.face_landmarks:
    #     # Send landmarks
    #     landmarks = result.face_landmarks[0]
    #     landmark_strs = [f"{i},{lm.x:.5f},{lm.y:.5f},{lm.z:.5f}" for i, lm in enumerate(landmarks)]
    #     data = "|".join(landmark_strs)
    #     client.send_message(OSC_ADDRESS, data)

    #     # Send blendshapes
    #     if result.face_blendshapes:
    #         blendshapes = result.face_blendshapes[0]
    #         blendshape_strs = [f"{b.category_name},{b.score:.5f}" for b in blendshapes]
    #         blend_data = "|".join(blendshape_strs)
    #         client.send_message(OSC_BLEND_ADDRESS, blend_data)

# Function to draw landmarks on the output image
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

# Function to wait for the camera to open
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

# Function to map MediaPipe blendshapes to avatar blendshapes
def map_mediapipe_to_avatar(mediapipe_result, scale_factor=100.0):
    """
    Takes a MediaPipe FaceLandmarker result and maps it to the avatar's blendshape values.
    
    Args:
        mediapipe_result: The result object from the MediaPipe FaceLandmarker.
        scale_factor: The value to multiply the 0-1 scores by to get the desired range for your avatar's blendshapes.

    Returns:
        A dictionary where keys are your avatar's blendshape names and values are the mapped scores.
        Returns None if no blendshapes are found in the result.
    """
    # Check if the result contains face blendshapes
    if not mediapipe_result or not mediapipe_result.face_blendshapes:
        print("No face blendshapes found in the MediaPipe result.")
        return None

    # Create a dictionary from the MediaPipe blendshape results for easy lookup
    mp_scores = {shape.category_name: shape.score for shape in mediapipe_result.face_blendshapes[0]}
    """ E.G. mp_scores = 
    {
        'mouthOpen': 0.73,
        'eyeBlinkLeft': 0.02,
        }
    """    

    # Initialize a dictionary for our avatar's blendshape values, all at 0 until set
    avatar_scores = {}

    # Loop through mapping dictionary
    for mp_name, avatar_names in MEDIAPIPE_TO_AVATAR_MAPPING.items():

        # Get the score from MediaPipe, defaulting to 0 if not found
        score = mp_scores.get(mp_name, 0)
        
        # Apply this score to all corresponding avatar blendshapes
        for avatar_name in avatar_names:
            avatar_scores[avatar_name] = score * scale_factor
            
    return avatar_scores


# MediaPipe FaceLandmarker setup

print("Setting up MediaPipe FaceLandmarker...")

# Get the directory where face.py is located
script_dir = os.path.dirname(os.path.abspath(__file__))

# Build the absolute path to the model file
model_path = os.path.join(script_dir, "face_landmarker.task")
print(f"Using model at: {model_path}")

# Import MediaPipe tasks
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
    running_mode=VisionRunningMode.LIVE_STREAM, 
    result_callback=process_result # Tell MediaPipe to call our function with the results
)

# --- Blendshape Mapping ---
# Keys are MediaPipe blendshape names.
# Values are a list of corresponding Avatar blendshape names.
MEDIAPIPE_TO_AVATAR_MAPPING = {
    'browInnerUp': ['Brow_Raise_Inner_L', 'Brow_Raise_Inner_R'],
    'browDownLeft': ['Brow_Drop_L'],
    'browDownRight': ['Brow_Drop_R'],
    'browOuterUpLeft': ['Brow_Raise_Outer_L'],
    'browOuterUpRight': ['Brow_Raise_Outer_R'],
    'eyeBlinkLeft': ['Eye_Blink_L'],
    'eyeBlinkRight': ['Eye_Blink_R'],
    'eyeSquintLeft': ['Eye_Squint_L'],
    'eyeSquintRight': ['Eye_Squint_R'],
    'eyeWideLeft': ['Eye_Wide_L'],
    'eyeWideRight': ['Eye_Wide_R'],
    'eyeLookOutLeft': ['Eye_L_Look_L'],
    'eyeLookInLeft': ['Eye_L_Look_R'],
    'eyeLookOutRight': ['Eye_R_Look_R'],
    'eyeLookInRight': ['Eye_R_Look_L'],
    'eyeLookUpLeft': ['Eye_L_Look_Up'],
    'eyeLookUpRight': ['Eye_R_Look_Up'],
    'eyeLookDownLeft': ['Eye_L_Look_Down'],
    'eyeLookDownRight': ['Eye_R_Look_Down'],
    'cheekPuff': ['Cheek_Puff_L', 'Cheek_Puff_R'],
    'cheekSquintLeft': ['Cheek_Raise_L'],
    'cheekSquintRight': ['Cheek_Raise_R'],
    'noseSneerLeft': ['Nose_Sneer_L', 'Nose_Nostril_Raise_L'],
    'noseSneerRight': ['Nose_Sneer_R', 'Nose_Nostril_Raise_R'],
    'jawOpen': ['Jaw_Open'],
    'jawForward': ['Jaw_Forward'],
    'jawLeft': ['Jaw_L'],
    'jawRight': ['Jaw_R'],
    'mouthSmileLeft': ['Mouth_Smile_L'],
    'mouthSmileRight': ['Mouth_Smile_R'],
    'mouthFrownLeft': ['Mouth_Frown_L'],
    'mouthFrownRight': ['Mouth_Frown_R'],
    'mouthDimpleLeft': ['Mouth_Dimple_L'],
    'mouthDimpleRight': ['Mouth_Dimple_R'],
    'mouthStretchLeft': ['Mouth_Stretch_L'],
    'mouthStretchRight': ['Mouth_Stretch_R'],
    'mouthPucker': ['Mouth_Pucker_Up_L', 'Mouth_Pucker_Up_R', 'Mouth_Pucker_Down_L', 'Mouth_Pucker_Down_R'],
    'mouthFunnel': ['Mouth_Funnel_Up_L', 'Mouth_Funnel_Up_R', 'Mouth_Funnel_Down_L', 'Mouth_Funnel_Down_R'],
    'mouthRollUpper': ['Mouth_Roll_In_Upper_L', 'Mouth_Roll_In_Upper_R'],
    'mouthRollLower': ['Mouth_Roll_In_Lower_L', 'Mouth_Roll_In_Lower_R'],
    'mouthShrugUpper': ['Mouth_Shrug_Upper'],
    'mouthShrugLower': ['Mouth_Shrug_Lower'],
    'mouthClose': ['Mouth_Close'],
    'mouthUpperUpLeft': ['Mouth_Up_Upper_L'],
    'mouthUpperUpRight': ['Mouth_Up_Upper_R'],
    'mouthLowerDownLeft': ['Mouth_Down_Lower_L'],
    'mouthLowerDownRight': ['Mouth_Down_Lower_R'],
}


# --- Main Loop ---
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
            print("Released camera and destroyed all windows.")
            cap.release()
            cv2.destroyAllWindows()
            
except Exception as e:
    print("Failed to initialize FaceLandmarker:", e)
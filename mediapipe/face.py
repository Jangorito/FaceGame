import cv2
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from pythonosc import udp_client
import time

# --- Part 1: OSC and MediaPipe Setup ---

# OSC setup
OSC_IP = "127.0.0.1"
OSC_PORT = 9000
OSC_ADDRESS = "/FaceData"
OSC_BLEND_ADDRESS = "/FaceBlendshapes"
client = udp_client.SimpleUDPClient(OSC_IP, OSC_PORT)

# This function will be the "callback" that handles results from MediaPipe
def process_result(result, output_image: mp.Image, timestamp_ms: int):
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

# MediaPipe FaceLandmarker setup
model_path = "face_landmarker.task"
BaseOptions = mp.tasks.python.BaseOptions
FaceLandmarker = mp.tasks.vision.FaceLandmarker
FaceLandmarkerOptions = mp.tasks.vision.FaceLandmarkerOptions
VisionRunningMode = mp.tasks.vision.RunningMode

options = FaceLandmarkerOptions(
    base_options=BaseOptions(model_asset_path=model_path),
    output_face_blendshapes=True,
    output_facial_transformation_matrixes=True,
    num_faces=1,
    running_mode=VisionRunningMode.LIVE_STREAM, # CRITICAL: Use LIVE_STREAM for video
    result_callback=process_result # Tell MediaPipe to call our function with the results
)

# --- Part 2: Main Loop ---

# The landmarker is created within a 'with' block to ensure resources are managed
with FaceLandmarker.create_from_options(options) as landmarker:
    cap = cv2.VideoCapture(0)
    frame_timestamp_ms = 0

    try:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            # Convert the frame to a MediaPipe Image object.
            mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
            
            # Calculate the timestamp for the current frame
            frame_timestamp_ms = int(time.time() * 1000)

            # Call detect_async to process the frame. The result will be sent to our 'process_result' function.
            landmarker.detect_async(mp_image, frame_timestamp_ms)


            # Display the frame for debugging
            cv2.imshow("FaceLandmarker", frame)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
    finally:
        cap.release()
        cv2.destroyAllWindows()
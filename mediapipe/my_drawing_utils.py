
import math
from typing import Optional, Tuple

import cv2
import matplotlib.pyplot as plt
import numpy as np
import mediapipe

from mediapipe.framework.formats import landmark_pb2

_PRESENCE_THRESHOLD = 0.5
_VISIBILITY_THRESHOLD = 0.5
_BGR_CHANNELS = 3

def _normalized_to_pixel_coordinates(normalized_x: float, normalized_y: float, image_width: int, image_height: int) -> Optional[Tuple[int, int]]:
  """Converts normalized coordinates to pixel coordinates."""
  if 0 <= normalized_x <= 1 and 0 <= normalized_y <= 1:
    x_px = min(math.floor(normalized_x * image_width), image_width - 1)
    y_px = min(math.floor(normalized_y * image_height), image_height - 1)
    return (x_px, y_px)
  return None

nose_line = [168, 6, 197, 195, 5, 1, 19] # the line along the nose that we don't want to draw


def draw_my_landmarks(
    image: np.ndarray,
    landmark_list: landmark_pb2.NormalizedLandmarkList,
    printer: bool,
    ):
  """Draws EMMANUEL'S CHOICE OF landmarks on the image.

  Args:
    image: A three channel BGR image represented as numpy ndarray.
    landmark_list: A normalized landmark list proto message to be annotated on
      the image.

  Raises:
    ValueError: If one of the followings:
      a) If the input image is not three channel BGR.
      b) If any connetions contain invalid landmark index.
  """
  if not landmark_list:
    return
  if image.shape[2] != _BGR_CHANNELS:
    raise ValueError('Input image must contain three channel bgr data.')
  image_rows, image_cols, _ = image.shape
  idx_to_coordinates = {} # A map from landmark index to pixel coordinates.
  font = cv2.FONT_HERSHEY_SIMPLEX
  font_scale = 0.4
  thickness = 1

  nose_landmarks = []
  final_nose_landmarks = []
  if printer:
    print("outside of for loop but function knows printer is True")

  for nose in mediapipe.solutions.face_mesh.FACEMESH_NOSE:
    nose_landmarks.append(nose[0])
    nose_landmarks.append(nose[1])

  for idx, landmark in enumerate(landmark_list.landmark): 
    if ((landmark.HasField('visibility') and
         landmark.visibility < _VISIBILITY_THRESHOLD) or
        (landmark.HasField('presence') and
         landmark.presence < _PRESENCE_THRESHOLD)):# Skip landmarks that are not visible or present.
      continue
    # if (idx not in nose_x and idx not in nose_y):
    #   continue
    
    # print (f'landmark {idx} x: {landmark.x}, y: {landmark.y}, z: {landmark.z}')
    landmark_px = _normalized_to_pixel_coordinates(landmark.x, landmark.y,
                                                   image_cols, image_rows) # Convert normalized coordinates to pixel coordinates.
    if landmark_px: # Only draw the landmark if it is within the image bounds.
      idx_to_coordinates[idx] = landmark_px # Store the pixel coordinates for each landmark index.

      # Define the text color and font
      if idx in nose_landmarks and idx not in nose_line:
        final_nose_landmarks.append(idx)
        text_color = (255, 0, 0) # Blue color for nose landmarks
        cv2.putText(image, str(idx), (landmark_px[0], landmark_px[1]),
            font, font_scale, text_color, thickness)  

        if printer:
          print(f"Nose landmark {idx} at coordinates: {landmark_px}")

      else:
        text_color = (0, 0, 255) # Red color for numbers

      # Draw the landmark number
      # cv2.putText(image, str(idx), (landmark_px[0], landmark_px[1]),
      #             font, font_scale, text_color, thickness)  
    # if printer:
    #   print(f"idx_to_coordinates = {idx_to_coordinates}")
    #   continue
    # print(f"final_nose_landmarks = {final_nose_landmarks}")

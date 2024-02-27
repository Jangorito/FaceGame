
import mediapipe as mp #  global_vars module is defined in global_vars.py
import struct
from mediapipe.python.solutions.pose import PoseLandmark
from mediapipe.python.solutions.drawing_utils import DrawingSpec
from mediapipe.framework.formats import landmark_pb2


custom_style =""

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

print(custom_face_landmarks)
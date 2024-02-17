

class Program{

    //static List<enum>[] Enums = {FaceEnum, PoseEnum, RightHand, LeftHand };

    enum FaceEnum
    {
        //Outer lips clockwise from the middle
        LIPS_OUTER_1 = 0,
        LIPS_OUTER_2 = 269,
        LIPS_OUTER_3 = 291,
        LIPS_OUTER_4 = 405,
        LIPS_OUTER_5 = 17,
        LIPS_OUTER_6 = 181,
        LIPS_OUTER_7 = 61,
        LIPS_OUTER_8 = 39,

        //Inner lip clockwise from the mddle
        LIPS_INNER_1 = 13,
        LIPS_INNER_2 = 311,
        LIPS_INNER_3 = 308,
        LIPS_INNER_4 = 402,
        LIPS_INNER_5 = 14,
        LIPS_INNER_6 = 178,
        LIPS_INNER_7 = 78,
        LIPS_INNER_8 = 81,

        //Left Eyebrow
        LEFT_LOWER_EYEBROW_1 = 276,
        LEFT_LOWER_EYEBROW_2 = 282,
        LEFT_LOWER_EYEBROW_3 = 285,
        LEFT_UPPER_EYEBROW_1 = 336,
        LEFT_UPPER_EYEBROW_2 = 334,
        LEFT_UPPER_EYEBROW_3 = 300,

        //Right Eyebrow
        RIGHT_LOWER_EYEBROW_1 = 46,
        RIGHT_LOWER_EYEBROW_2 = 52,
        RIGHT_LOWER_EYEBROW_3 = 55,
        RIGHT_UPPER_EYEBROW_1 = 70,
        RIGHT_UPPER_EYEBROW_2 = 105,
        RIGHT_UPPER_EYEBROW_3 = 107,

        //Face Oval
        FACE_OVAL_1 = 10,
        FACE_OVAL_2 = 297,
        FACE_OVAL_3 = 284,
        FACE_OVAL_4 = 389,
        FACE_OVAL_5 = 454,
        FACE_OVAL_6 = 361,
        FACE_OVAL_7 = 397,
        FACE_OVAL_8 = 379,
        FACE_OVAL_9 = 400,
        FACE_OVAL_10 = 152,
        FACE_OVAL_11 = 176,
        FACE_OVAL_12 = 150,
        FACE_OVAL_13 = 172,
        FACE_OVAL_14 = 132,
        FACE_OVAL_15 = 234,
        FACE_OVAL_16 = 162,
        FACE_OVAL_17 = 54,
        FACE_OVAL_18 = 67

    }

    enum PoseEnum
    {
        RIGHT_SHOULDER = 11,
        LEFT_SHOULDER = 12,
        RIGHT_ELBOW = 13,
        LEFT_ELBOW = 14,
        RIGHT_WRIST = 15,
        LEFT_WRIST = 16,
        RIGHT_HIP = 23,
        LEFT_HIP = 24,
        RIGHT_KNEE = 25,
        LEFT_KNEE = 26,
        RIGHT_ANKLE = 27,
        LEFT_ANKLE = 28,
        RIGHT_HEEL = 29,
        LEFT_HEEL = 30,
        RIGHT_TOE = 31,
        LEFT_TOE = 32
    }

    enum RightHand
    {
        RIGHT_WRIST = 0,
        RIGHT_THUMB_CMC = 1,
        RIGHT_THUMB_MCP = 2,
        RIGHT_THUMB_IP = 3,
        RIGHT_THUMB_TIP = 4,
        RIGHT_INDEX_FINGER_MCP = 5,
        RIGHT_INDEX_FINGER_PIP = 6,
        RIGHT_INDEX_FINGER_DIP = 7,
        RIGHT_INDEX_FINGER_TIP = 8,
        RIGHT_MIDDLE_FINGER_MCP = 9,
        RIGHT_MIDDLE_FINGER_PIP = 10,
        RIGHT_MIDDLE_FINGER_DIP = 11,
        RIGHT_MIDDLE_FINGER_TIP = 12,
        RIGHT_RING_FINGER_MCP = 13,
        RIGHT_RING_FINGER_PIP = 14,
        RIGHT_RING_FINGER_DIP = 15,
        RIGHT_RING_FINGER_TIP = 16,
        RIGHT_PINKY_MCP = 17,
        RIGHT_PINKY_PIP = 18,
        RIGHT_PINKY_DIP = 19,
        RIGHT_PINKY_TIP = 20
        
        }

        enum LeftHand
    {
        LEFT_WRIST = 0,
        LEFT_THUMB_CMC = 1,
        LEFT_THUMB_MCP = 2,
        LEFT_THUMB_IP = 3,
        LEFT_THUMB_TIP = 4,
        LEFT_INDEX_FINGER_MCP = 5,
        LEFT_INDEX_FINGER_PIP = 6,
        LEFT_INDEX_FINGER_DIP = 7,
        LEFT_INDEX_FINGER_TIP = 8,
        LEFT_MIDDLE_FINGER_MCP = 9,
        LEFT_MIDDLE_FINGER_PIP = 10,
        LEFT_MIDDLE_FINGER_DIP = 11,
        LEFT_MIDDLE_FINGER_TIP = 12,
        LEFT_RING_FINGER_MCP = 13,
        LEFT_RING_FINGER_PIP = 14,
        LEFT_RING_FINGER_DIP = 15,
        LEFT_RING_FINGER_TIP = 16,
        LEFT_PINKY_MCP = 17,
        LEFT_PINKY_PIP = 18,
        LEFT_PINKY_DIP = 19,
        LEFT_PINKY_TIP = 20
        
        }

}
We might have to exclude some landmarks because we might have issues with certain ones like left ear or right ear.
We can exclude landmarks by using this code:

while (deck1 == 7 || deck1 == 8 || deck2 == 7 || deck2 == 8)
        {
            deck1 = random.Next(-2, 32);
            deck2 = random.Next(-2, 32);
        }

Wasn't entirely sure about the difficulty levels (and we can change them) but as it stands:
Easy: The chosen body part moves the same body part by x degrees.
Medium: The chosen body part moves another body part by x degrees.
Hard: The chosen body part moves another body part by x degrees. (Larger range)

Feel free to let me know if you think this should be different.


Landmark values:

    NOSE = 0,
    LEFT_EYE_INNER = 1,
    LEFT_EYE = 2,
    LEFT_EYE_OUTER = 3,
    RIGHT_EYE_INNER = 4,
    RIGHT_EYE = 5,
    RIGHT_EYE_OUTER = 6,
    LEFT_EAR = 7,
    RIGHT_EAR = 8,
    MOUTH_LEFT = 9,
    MOUTH_RIGHT = 10,
    LEFT_SHOULDER = 11,
    RIGHT_SHOULDER = 12,
    LEFT_ELBOW = 13,
    RIGHT_ELBOW = 14,
    LEFT_WRIST = 15,
    RIGHT_WRIST = 16,
    LEFT_PINKY = 17,
    RIGHT_PINKY = 18,
    LEFT_INDEX = 19,
    RIGHT_INDEX = 20,
    LEFT_THUMB = 21,
    RIGHT_THUMB = 22,
    LEFT_HIP = 23,
    RIGHT_HIP = 24,
    LEFT_KNEE = 25,
    RIGHT_KNEE = 26,
    LEFT_ANKLE = 27,
    RIGHT_ANKLE = 28,
    LEFT_HEEL = 29,
    RIGHT_HEEL = 30,
    LEFT_FOOT_INDEX = 31,
    RIGHT_FOOT_INDEX = 32,
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class DisplayText : MonoBehaviour
{
    TextMeshProUGUI mText;

    string calibrating = "Calibrating Avatars: Please make T-pose";
    string player1wins = "Player 1 wins; Congratulations";
    string player2wins = "Player 2 wins; Congratulations";
    string message1 = "What's taking so long; You forget how to move?";

    string[] insults = {
        "What's taking so long; You forget how to move?",
        "You look pretty funny right now",
        "Come on, I'm not that hard to match!",
        "No, that's not how you move an arm!",
        "Look at you dangling by your waist",
        "Congradulations, if you were trying to disappoint me"
        };

    int timeToChange = 10000;
    int timeTracker =0;

    bool m_bIsCalibrated = false;

    private void Awake()
    {
        mText = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        mText.text = calibrating;
    }

    // Update is called once per frame
    void Update()
    {
        timeTracker++;

        if (timeTracker > timeToChange)
        {
            int index = UnityEngine .Random.Range(0, insults.Length);
            mText.text = insults[index];
            timeTracker = 0;
        }

    }

    public void setIsCalibrated(bool val)
    {
        m_bIsCalibrated = val;
    }

    public void setText(string text)
    {
        mText.text = text;
    }
}

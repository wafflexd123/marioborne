using System;
using UnityEngine;

public class WatchScript : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating("UpdateClock", 0f, 1f);
    }

    void UpdateClock()
    {
        // current time
        DateTime time = DateTime.Now;

        // count angle
        float hourAngle = (time.Hour % 12) * 30f + time.Minute * 0.5f;
        float minuteAngle = time.Minute * 6f;
        float secondAngle = time.Second * 6f;

        // angle for rotation
        Transform hourHand = transform.Find("HourHand");
        Transform minuteHand = transform.Find("MinuteHand");
        Transform secondHand = transform.Find("SecondHand");
        hourHand.localRotation = Quaternion.Euler(0f, hourAngle, 0f);
        minuteHand.localRotation = Quaternion.Euler(0f, minuteAngle, 0f);
        secondHand.localRotation = Quaternion.Euler(0f, secondAngle, 0f);
    }

}

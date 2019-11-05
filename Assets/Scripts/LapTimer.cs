using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LapTimer : MonoBehaviour
{
    public static float countdownStart = 5;
    public Text countdownStartText, actualLapTimeText, bestLapTimeText;
    public List<GameObject> checkpointTriggers;
    int actualCheckpoint;
    float actualLapTimeMilliseconds, actualLapTimeSeconds, actualLapTimeMinutes, actualLapTime, bestLapTimeMilliseconds, bestLapTimeSeconds, bestLapTimeMinutes, bestLapTime;
    bool firstLap = true;

    private void Update()
    {
        countdownStart -= Time.deltaTime;
        countdownStart = Mathf.Round(countdownStart * 100);
        countdownStart /= 100;
        countdownStartText.text = countdownStart + "s";
        while (countdownStart > 0)
        {
            return;
        }
        countdownStartText.gameObject.SetActive(false);

        actualLapTime += Time.deltaTime;
        actualLapTimeMilliseconds += Time.deltaTime * 100;
        actualLapTimeMilliseconds = Mathf.Round(actualLapTimeMilliseconds);
        if (actualLapTimeMilliseconds > 100)
        {
            actualLapTimeMilliseconds -= 100;
            actualLapTimeSeconds++;
            if (actualLapTimeSeconds > 60)
            {
                actualLapTimeSeconds -= 60;
                actualLapTimeMinutes++;
            }
        }

        actualLapTimeText.text = "Current lap time: " + actualLapTimeMinutes + ":" + actualLapTimeSeconds + "." + actualLapTimeMilliseconds;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (checkpointTriggers.Count > 0)
        {
            checkpointTriggers[actualCheckpoint].SetActive(false);
            actualCheckpoint++;
            if (actualCheckpoint == checkpointTriggers.Count)
            {
                if (firstLap)
                {
                    bestLapTimeMilliseconds = actualLapTimeMilliseconds;
                    bestLapTimeSeconds = actualLapTimeSeconds;
                    bestLapTimeMinutes = actualLapTimeMinutes;
                    bestLapTime = actualLapTime;
                    bestLapTimeText.text = "Best Lap Time: " + actualLapTimeMinutes + ":" + actualLapTimeSeconds + "." + actualLapTimeMilliseconds;
                    firstLap = false;
                }

                if (actualLapTime < bestLapTime)
                {
                    bestLapTimeMilliseconds = actualLapTimeMilliseconds;
                    bestLapTimeSeconds = actualLapTimeSeconds;
                    bestLapTimeMinutes = actualLapTimeMinutes;
                    bestLapTime = actualLapTime;
                    bestLapTimeText.text = "Best Lap Time: " + actualLapTimeMinutes + ":" + actualLapTimeSeconds + "." + actualLapTimeMilliseconds;
                }
                actualLapTimeMilliseconds = 0;
                actualLapTimeSeconds = 0;
                actualLapTimeMinutes = 0;
                actualLapTime = 0;

                actualCheckpoint = 0;
            }
            checkpointTriggers[actualCheckpoint].SetActive(true);
        }
        else
        {
            return;
        }

        if (actualCheckpoint == checkpointTriggers.Count)
        {

        }
    }

}

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

        actualLapTimeText.text = "Volta atual: " + FormatToLapTime(actualLapTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Checkpoint")
        {
            if (checkpointTriggers.Count > 0)
            {
                checkpointTriggers[actualCheckpoint].SetActive(false);
                actualCheckpoint++;
                if (actualCheckpoint == checkpointTriggers.Count)
                {
                    if (firstLap)
                    {
                        SetBestLapTime();
                        firstLap = false;
                    }

                    if (actualLapTime < bestLapTime)
                    {
                        SetBestLapTime();
                    }

                    actualLapTime = 0;

                    actualCheckpoint = 0;
                }
                checkpointTriggers[actualCheckpoint].SetActive(true);
            }
            else
            {
                return;
            }
        }
    }

    void SetBestLapTime()
    {
        bestLapTime = actualLapTime;

        bestLapTimeText.text = "Melhor volta: " + FormatToLapTime(bestLapTime);

        HighscoreTable.Instance.AddHighscoreEntry(bestLapTime);
    }

    string FormatToLapTime(float time)
    {
        float lapTimeMiliseconds = time - Mathf.Floor(time);
        float lapTimeSeconds = (time - lapTimeMiliseconds) % 60;
        float lapTimeMinutes = ((time - lapTimeMiliseconds) - lapTimeSeconds) / 60;
        lapTimeMiliseconds = Mathf.Round(lapTimeMiliseconds * 1000);

        string milisecondsText = lapTimeMiliseconds.ToString("000");
        string secondsText = lapTimeSeconds.ToString("00");
        string minutesText = lapTimeMinutes.ToString("00");

        return (minutesText + ":" + secondsText + "." + milisecondsText);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HighscoreTable : MonoBehaviour
{
    public Transform entryContainer, entryTemplate;
    List<Transform> highscoreEntryTransformList;

    private void Awake()
    {
        //PlayerPrefs.DeleteKey("highscoreTable");

        entryTemplate.gameObject.SetActive(false);

        AddHighscoreEntry("Matheus", 128.985f);

        string jsonString = PlayerPrefs.GetString("highscoreTable");
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);

        highscoreEntryTransformList = new List<Transform>();
        foreach (HighscoreEntry highscoreEntry in highscores.highscoreEntryList)
        {
            CreateHighscoreEntryTransform(highscoreEntry, highscoreEntryTransformList);
        }

        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString("highscoreTable", json);
        PlayerPrefs.Save();
    }

    string FormatToLapTime(float time)
    {
        float lapTimeMiliseconds = time - Mathf.Floor(time);
        float lapTimeSeconds = (time - lapTimeMiliseconds) % 60;
        float lapTimeMinutes = ((time - lapTimeMiliseconds) - lapTimeSeconds) / 60;
        lapTimeMiliseconds = Mathf.Round(lapTimeMiliseconds * 1000);

        string milisecondsText = lapTimeMiliseconds.ToString("###");
        string secondsText = lapTimeSeconds.ToString("00");
        string minutesText = lapTimeMinutes.ToString("##");

        return (minutesText + ":" + secondsText + "." + milisecondsText);
    }

    private void CreateHighscoreEntryTransform(HighscoreEntry highscoreEntry, List<Transform> transformList)
    {
        Transform entryTransform = Instantiate(entryTemplate, entryContainer);

        entryTransform.gameObject.SetActive(true);

        int rank = transformList.Count + 1;

        entryTransform.Find("PosText").GetComponent<TMP_Text>().text = rank + "º";

        string name = highscoreEntry.name;
        entryTransform.Find("NameText").GetComponent<TMP_Text>().text = name;

        entryTransform.Find("TimeText").GetComponent<TMP_Text>().text = FormatToLapTime(highscoreEntry.lapTime);

        transformList.Add(entryTransform);
    }

    public void AddHighscoreEntry(string name, float lapTime)
    {
        HighscoreEntry highscoreEntry = new HighscoreEntry { name = name, lapTime = lapTime };

        string jsonString = PlayerPrefs.GetString("highscoreTable");
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);

        Debug.Log(highscores.highscoreEntryList.Count);

        highscores.highscoreEntryList.Add(highscoreEntry);

        for (int i = 0; i < highscores.highscoreEntryList.Count; i++)
        {
            for (int j = 0; j < highscores.highscoreEntryList.Count; j++)
            {
                if (highscores.highscoreEntryList[j].lapTime > highscores.highscoreEntryList[i].lapTime)
                {
                    HighscoreEntry tmp = highscores.highscoreEntryList[i];
                    highscores.highscoreEntryList[i] = highscores.highscoreEntryList[j];
                    highscores.highscoreEntryList[j] = tmp;
                }
            }
        }

        if (highscores.highscoreEntryList.Count > 8)
        {
            int worstLapTimeIndex = highscores.highscoreEntryList.Count - 1;
            if (highscores.highscoreEntryList[worstLapTimeIndex] == highscoreEntry)
            {
                highscores.highscoreEntryList.RemoveAt(worstLapTimeIndex);
            }
            else
            {
                highscores.highscoreEntryList.RemoveAt(worstLapTimeIndex);
                NewHighscore();
            }
        }
        else
        {
            NewHighscore();
        }

        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString("highscoreTable", json);
        PlayerPrefs.Save();
    }

    void NewHighscore()
    {
        Debug.Log("Novo recorde!");
    }

    private class Highscores
    {
        public List<HighscoreEntry> highscoreEntryList;
    }

    [System.Serializable]
    private class HighscoreEntry
    {
        public float lapTime;
        public string name;
    }
}

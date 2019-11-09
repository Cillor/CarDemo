using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HighscoreTable : MonoBehaviour
{
    public static HighscoreTable Instance { get; set; }
    string currentPlayerName;
    public TMP_InputField playerName;

    public void ChangePlayerName()
    {
        currentPlayerName = playerName.text;
        Debug.Log(playerName.text);
    }

    public Transform entryContainer, entryTemplate;
    List<Transform> highscoreEntryTransformList;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        entryTemplate.gameObject.SetActive(false);

        if (!PlayerPrefs.HasKey("highscoreTable"))
        {
            Highscores highscores1 = new Highscores();
            string json = JsonUtility.ToJson(highscores1);
            PlayerPrefs.SetString("highscoreTable", json);
            PlayerPrefs.Save();
        }

        string jsonString = PlayerPrefs.GetString("highscoreTable");
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);

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

        highscoreEntryTransformList = new List<Transform>();
        foreach (HighscoreEntry highscoreEntry in highscores.highscoreEntryList)
        {
            CreateHighscoreEntryTransform(highscoreEntry, highscoreEntryTransformList);
        }
    }

    string FormatToLapTime(float time)
    {
        float lapTimeMiliseconds = time - Mathf.Floor(time);
        float lapTimeSeconds = (time - lapTimeMiliseconds) % 60;
        float lapTimeMinutes = ((time - lapTimeMiliseconds) - lapTimeSeconds) / 60;
        lapTimeMiliseconds = Mathf.Round(lapTimeMiliseconds * 1000);

        string milisecondsText = lapTimeMiliseconds.ToString("000");
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

    public void AddHighscoreEntry(float lapTime)
    {
        HighscoreEntry newHighscoreEntry = new HighscoreEntry { name = currentPlayerName, lapTime = lapTime };
        string jsonString = PlayerPrefs.GetString("highscoreTable");
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);
        highscores.highscoreEntryList.Add(newHighscoreEntry);

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

        if (highscores.highscoreEntryList.Count > 10)
        {
            int worstLapTimeIndex = highscores.highscoreEntryList.Count - 1;
            if (highscores.highscoreEntryList[worstLapTimeIndex] == newHighscoreEntry)
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
        Debug.Log("Novo recorde de " + currentPlayerName);
    }

    [System.Serializable]
    public class Highscores
    {
        public List<HighscoreEntry> highscoreEntryList;
    }

    [System.Serializable]
    public class HighscoreEntry
    {
        public float lapTime;
        public string name;
    }
}

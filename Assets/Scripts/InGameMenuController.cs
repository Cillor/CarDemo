using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenuController : MonoBehaviour
{
    public GameObject pauseMenu, driverUI, pitStopMenu;

    [Space]
    public Slider fuelSlider;

    public static bool gamePaused;

    bool canUpdatePitStopValues = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (pitStopMenu.activeInHierarchy && canUpdatePitStopValues)
        {
            FindObjectOfType<FuelConsumption>().UpdateFuelInTankValue(fuelSlider);
            canUpdatePitStopValues = false;
        }

        if (!pitStopMenu.activeInHierarchy)
        {
            canUpdatePitStopValues = true;
        }
    }

    void TryPauseGame()
    {
        if (gamePaused)
        {
            UnpauseGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void UnpauseGame()
    {
        gamePaused = false;
        Time.timeScale = 1f;
    }

    private void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0f;
    }
}

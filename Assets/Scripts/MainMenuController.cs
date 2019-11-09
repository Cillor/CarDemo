using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    public GameObject nameScreen, mainScreen, optionsScreen;
    public GameObject jogarNameScreen, jogarMainScreen, optionsMainScreen;
    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (nameScreen.activeInHierarchy)
            {
                nameScreen.SetActive(false);
                mainScreen.SetActive(true);
                EventSystem.current.SetSelectedGameObject(jogarMainScreen);
            }

            if (optionsScreen.activeInHierarchy)
            {
                optionsScreen.SetActive(false);
                mainScreen.SetActive(true);
                EventSystem.current.SetSelectedGameObject(optionsMainScreen);
            }
        }
        if (nameScreen.activeInHierarchy)
        {
            if (Input.GetButtonDown("Submit"))
            {
                EventSystem.current.SetSelectedGameObject(jogarNameScreen);
            }
        }
    }
    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void Quit()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }

    public void SetNewSelected(GameObject firstSelected)
    {
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }
}

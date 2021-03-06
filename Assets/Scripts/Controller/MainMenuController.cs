﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private void Start()
    {
        GameObject.Find("lblTitle").GetComponent<Text>().text = Application.productName;
        GameObject.Find("lblVersion").GetComponent<Text>().text = string.Format("Version: {0}", Application.version);
    }

    public void btnPlay_Click()
    {
        if (GameController.atSchool)
        {
            SceneManager.LoadScene("Main");
            PersistentController.AddStatus("At School Mode");
            return;
        }

        PersistentController._NetworkController.Connect();
    }
    public void QuitGame()
    {
        Debug.Log("Closing game.");
        Application.Quit();
    }
}

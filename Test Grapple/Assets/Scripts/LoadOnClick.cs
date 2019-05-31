using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// UI code from Unity tutorial: https://learn.unity.com/tutorial/live-sessions-on-ui#5c7f8528edbc2a002053b4b3

public class LoadOnClick : MonoBehaviour
{
    public GameObject LoadingImage;
    public void LoadScene(int level)
    {
        LoadingImage.SetActive(true);
        SceneManager.LoadScene(level);
    }
}

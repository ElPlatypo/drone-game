using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UserInterface : MonoBehaviour
{
    public TextMeshProUGUI scrapcounter;
    public GameObject gameui;
    public GameObject starmap;

    private void Start()
    {
        GameDirector.current.actions.onScrapChanged += ChangeScrap;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (starmap.activeInHierarchy)
            {
                starmap.SetActive(false);
                gameui.SetActive(true);
                Time.timeScale = 1f;
            }
            else
            {
                starmap.SetActive(true);
                gameui.SetActive(false);
                Time.timeScale = 0f;
            }
        }
    }

    private void ChangeScrap()
    {
        scrapcounter.text = "Scrap: " + GameDirector.current.scrap;
    }
}

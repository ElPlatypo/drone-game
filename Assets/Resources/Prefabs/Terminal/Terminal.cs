using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class Terminal : MonoBehaviour
{

    public TMP_InputField inputfield;
    public GameObject history;

    private void Start()
    {
        GameDirector.current.assets.terminal = this;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && inputfield.text != "")
        {
            StartCoroutine(EnterCommand());
        }
    }

    IEnumerator EnterCommand()
    {
        string txt = inputfield.GetComponent<TMP_InputField>().text;
        Print(txt);
        ParseCommand(inputfield.text);
        inputfield.text = "";
        inputfield.ActivateInputField();
        yield return new WaitForFixedUpdate();
        history.GetComponentInChildren<ScrollRect>().normalizedPosition = new Vector2(0, 0);
        
    }

    private void ParseCommand(string text)
    {
        string[] input = text.Split(" ");

        if (input.Length > 0)
        {
            switch (input[0])
            {
                case "d":
                case "door":
                if (input.Length == 2)
                {
                    DoorToggle(int.Parse(input[1]));
                }
                else
                {
                    Print("command syntax error: (door)/(d) (doorid)");
                }
                break;

                case "t":
                case "teleport":
                if (input.Length == 3)
                {
                    Teleport(int.Parse(input[1]), int.Parse(input[2]));
                }
                else
                {
                    Print("command syntax error: (teleport)/(t) (teleid start) (teleid end)");
                }
                break;
            }
        }
    }

    public void Print(string inputtext)
    {
        GameObject input = new GameObject();
        TextMeshProUGUI text = input.AddComponent<TextMeshProUGUI>();
        input.GetComponent<RectTransform>().sizeDelta = new Vector2(380, 30);
        text.text = inputtext;
        text.fontSize = 20;
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        input.name = "line";
        GameObject panel = history.GetComponentInChildren<VerticalLayoutGroup>().gameObject;
        input.transform.SetParent(panel.transform);
    }

    private void DoorToggle(int id)
    {
        GameDirector.current.actions.onDoorToggle(id);
    }

    private void Teleport(int start, int end)
    {
        GameDirector.current.actions.onTeleportRequest(start, end);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Door : MonoBehaviour
{

    public int id;
    public bool isopen = false;
    public bool ispowered = false;
    public List<GameObject> blockingentities;
    public List<Room> connectedrooms = new List<Room>();
    public GameObject doorpanel;
    public TextMeshProUGUI text;

    private void Start()
    {
        SetId(GameDirector.current.assets.GetIndex("doors"));
        GameDirector.current.assets.doors.Add(this);
    }
    public void OpenDoor()
    {
        if (!isopen && ispowered)
        {
            doorpanel.SetActive(false);
            isopen = true;
            GameDirector.current.roomgraph.UpdateIslands();
        }
        else if (!ispowered)
        {
            GameDirector.current.assets.terminal.Print("door " + id + " is not powered");
        }
    }

    public void CloseDoor()
    {
        if (isopen && ispowered && blockingentities.Count == 0)
        {
            doorpanel.SetActive(true);
            isopen = false;
            GameDirector.current.roomgraph.UpdateIslands();
        }
        else if (!ispowered)
        {
            GameDirector.current.assets.terminal.Print("door " + id + " is not powered");
        }
        else if (blockingentities.Count != 0)
        {
            GameDirector.current.assets.terminal.Print("door " + id + " is blocked by something");
        }
    }

    public void SetId(int newid)
    {
        id = newid;
        text.text = "D" + newid;
        gameObject.name = "Door " + id;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Room>() != null && !connectedrooms.Contains(other.GetComponent<Room>()))
        {
            connectedrooms.Add((other.GetComponent<Room>()));
        }
        else if (other.GetComponent<Drone>() != null || other.GetComponent<Enemy>() != null)
        {
            if (!blockingentities.Contains(other.gameObject))
            {
                blockingentities.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Room>() != null)
        {
            connectedrooms.Remove(other.GetComponent<Room>());
        }
        else if (other.GetComponent<Drone>() != null || other.GetComponent<Enemy>() != null)
        {
            blockingentities.Remove(other.gameObject);
        }
    }
}

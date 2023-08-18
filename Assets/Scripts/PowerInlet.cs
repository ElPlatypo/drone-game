using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerInlet : MonoBehaviour
{

    public int id;
    public int room;
    public bool ispowered = false;
    void Start()
    {
        SetId(GameDirector.current.assets.GetIndex("inlets"));
        GameDirector.current.assets.inlets.Add(this);
    }


    private void SetId(int newid)
    {
        id = newid;
        gameObject.name = "Inlet " + id;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Drone>() != null)
        {
            ispowered = true;
            GameDirector.current.actions.onPowerChanged();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Drone>() != null)
        {
            ispowered = false;
            GameDirector.current.actions.onPowerChanged();
        }
    }

}

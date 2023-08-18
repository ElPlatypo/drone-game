using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Teleporter : MonoBehaviour
{
    public int id;
    public GameObject drone;
    private void Start()
    {
        SetId(GameDirector.current.assets.GetIndex("teles"));
        GameDirector.current.assets.teleporters.Add(this);
        GameDirector.current.actions.onTeleportRequest += Teleport;
    }

    private void Teleport(int start, int destination)
    {
        if (id == start && drone != null)
        {
            foreach (Teleporter tele in GameDirector.current.assets.teleporters)
            {
                if (tele.id == destination)
                {
                    NavMeshAgent agent = drone.GetComponent<NavMeshAgent>();
                    bool c = false;
                    if (agent.enabled == false)
                    {
                        agent.enabled = true;
                        c = true;
                    }
                    agent.Warp(tele.transform.position);
                    if (c)
                    {
                        agent.enabled = false;
                    }
                }
            }
        }
    }

    private void SetId(int newid)
    {
        id = newid;
        gameObject.name = "Teleporter " + id;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Drone>() != null)
        {
            drone = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == drone)
        {
            drone = null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class NavMeshManager : MonoBehaviour
{
    public NavMeshSurface navmesh;
    void Start()
    {
        navmesh = GetComponent<NavMeshSurface>();
        int? id = GetNavMeshAgentID("Drone");
        if (id != null)
        {
            navmesh.agentTypeID = id.Value;
        }
        navmesh.BuildNavMesh();
    }

    private int? GetNavMeshAgentID(string name)
    {
        for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
        {
            NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(index: i);
            if (name == NavMesh.GetSettingsNameFromID(agentTypeID: settings.agentTypeID))
            {
                return settings.agentTypeID;
            }
        }
        return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unloader : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Drone drone = other.GetComponent<Drone>();
        if (drone != null && drone.scrapinventory > 0)
        {
            GameDirector.current.ChangeScrap(drone.scrapinventory);
            drone.scrapinventory = 0;
        }
    }
}

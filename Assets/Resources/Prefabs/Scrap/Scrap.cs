using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrap : MonoBehaviour
{
    public int amount;

    private void Awake()
    {
        amount = (int) Mathf.Round(Random.Range(1, 3));
    }
    private void OnTriggerEnter(Collider other)
    {
        Drone drone = other.GetComponent<Drone>();
        if (drone != null)
        {
            drone.scrapinventory += amount;
            Destroy(this.gameObject);
        }
    }
}

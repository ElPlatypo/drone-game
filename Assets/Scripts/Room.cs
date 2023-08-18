using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    //public bool constructionmode = true;
    
    public List<Vector3[]> neighbours = new List<Vector3[]>();
    public int neighbourscount = 0;
    public int id;
    public bool constructionmode = true;
    public bool ispowered;
    public List<Door> doors;
    public List<Enemy> enemies;
    public List<Drone> drones;
    // Start is called before the first frame update
    void Start()
    {
        SetId(GameDirector.current.assets.GetIndex("rooms"));
        GameDirector.current.assets.rooms.Add(this);
        doors = new List<Door>();
        enemies = new List<Enemy>();
        drones = new List<Drone>();
        ispowered = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (constructionmode && collision.contactCount != 1)
        {
            ContactPoint[] contacts = new ContactPoint[collision.contactCount];
            Vector3[] points = new Vector3[collision.contactCount / 2];
            collision.GetContacts(contacts);
            int k = 0;
            for (int i = 0; i < contacts.Length; i++)
            {
                if (contacts[i].point.y > 0)
                {
                    points[k] = new Vector3(Mathf.Round(contacts[i].point.x), 0, Mathf.Round(contacts[i].point.z));
                    k++;
                }
            }
            neighbours.Add(points);
            neighbourscount ++;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
        Drone drone = other.GetComponent<Drone>();
        if (drone != null && !drones.Contains(drone))
        {
            drones.Add(drone);
        }
        Door door = other.GetComponent<Door>();
        if (door != null && !doors.Contains(door))
        {
            doors.Add(door);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>(); 
        if (enemy != null)
        {
            enemies.Remove(enemy);
        }
        Drone drone = other.GetComponent<Drone>();
        if (drone != null)
        {
            drones.Remove(drone);
        }
        Door door = other.GetComponent<Door>();
        if (door != null)
        {
            doors.Remove(door);
        }
    }

    public void SetId(int newid)
    {
        id = newid;
        gameObject.name = "Room " + id;
    }
}

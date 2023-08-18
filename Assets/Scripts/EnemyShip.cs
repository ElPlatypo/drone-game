using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShip : MonoBehaviour
{
    public float time = 0;
    public Vector3 newpos;
    public Vector3 oldpos;
    public Vector3 startpos;
    public float manouver = 0;

    public void Start()
    {
        newpos = transform.position;
        oldpos = transform.position;
        startpos = transform.position;
    }
    void Update()
    {
        time += Time.deltaTime;
        
        if (transform.position != newpos)
        {
            transform.position = Vector3.Lerp(oldpos, newpos, manouver);
            manouver += 0.01f;
        }
        else
        {
            manouver = 0;
        }
        if (time > 10)
        {
            time = 0;
            newpos = new Vector3(startpos.x, Random.Range(startpos.y + 10, startpos.y - 10), Random.Range(startpos.z + 10, startpos.z -10));
            oldpos = transform.position;
        }
    }
}

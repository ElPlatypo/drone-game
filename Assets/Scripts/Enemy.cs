using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    public NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(Behaviour());
    }

    IEnumerator Behaviour()
    {
        float patroltimer = 0;
        while (true)
        {
            NavMeshPath path = new NavMeshPath();
            bool c = false;
            foreach (GameObject drone in GameDirector.current.activedrones)
            {
                if (agent.CalculatePath(drone.transform.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    Chase(drone);
                    c = true;
                }
            }
            if (patroltimer > 10 && c == false)
            {
                Patrol();
                patroltimer = 0;
            }
            patroltimer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }   
    }

    private void Patrol()
    {
        foreach (Collider col in Physics.OverlapSphere(transform.position, 0.3f,  1 << LayerMask.NameToLayer("Rooms")))
        {
            Dungeon derelict = GameDirector.current.currentstructure;
            int roomid = col.GetComponent<Room>().id;
            foreach (List<Room> island in GameDirector.current.roomgraph.islands)
            {
                foreach(Room room in island)
                {
                    if (room.id == roomid)
                    {
                        roomid = island[Mathf.RoundToInt(Random.Range(0, island.Count))].id;
                    }
                }
            }
            var newpos = derelict.RandomLocarionInRoom(roomid, 10);
            if (newpos != null)
            {
                agent.SetDestination(newpos.Value);
            }
        }
    }

    private void Chase(GameObject target)
    {
        agent.SetDestination(target.transform.position);
    }

    private void Attack()
    {

    }
}

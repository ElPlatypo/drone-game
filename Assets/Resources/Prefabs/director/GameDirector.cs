using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class GameDirector : MonoBehaviour
{

    static int size = 200;
    BoxCollider[] roomcolliders = new BoxCollider[size];
    public bool move = false;
    public List<GameObject> activedrones;
    public static GameDirector current;
    public Camera[] cameras;
    public Camera currentcamera;
    public GameObject selecteddrone;
    public LoadedAssets assets; 
    public int scrap = 0;
    public RoomGraph roomgraph;
    public Planet currentplanet;
    public Destination currentdestination;
    public Dungeon currentstructure;
    public Actions actions;

    private void Awake()
    {
        current = this;
        actions = new Actions();
        assets = new LoadedAssets();
        cameras = new Camera[10];
        roomgraph = new RoomGraph();
        roomgraph.UpdateIslands();
        cameras[0] = currentcamera;
    }

    private void Start()
    {
        GenerateDungeon();
    }

    public void DoorToggleEvent(int id)
    {
        if (actions.onDoorToggle != null)
        {
            actions.onDoorToggle(id);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cameras[0].ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.GetComponent<Drone>() != null)
            {
                selecteddrone = hit.collider.gameObject;
            }
            else if (selecteddrone != null)
            {
                selecteddrone.GetComponent<NavMeshAgent>().SetDestination(hit.point);
                selecteddrone = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && cameras.Length != 0)
        {   
            for (int i = 0; i < cameras.Length; i++)
            {
                if (currentcamera == cameras[i])
                {
                    int k = i + 1;
                    while (true)
                    {
                        if (cameras[k])
                        {
                            ChangeCamera(i, k);
                            break;
                        }
                        k++;
                        if (k >= 9)
                        {
                            k = 0;
                        }
                    }
                    break;
                }
            }
        }
    }

    private void ChangeCamera(int current, int next)
    {
        cameras[current].enabled = false;
        if (cameras[current].GetComponentInParent<Drone>() != null)
        {
            cameras[current].GetComponentInParent<NavMeshAgent>().enabled = true;
            cameras[current].GetComponentInParent<CharacterController>().enabled = false;
        }

        cameras[next].enabled = true;
        if (cameras[next].GetComponentInParent<Drone>() != null)
        {
            cameras[next].GetComponentInParent<NavMeshAgent>().enabled = false;
            cameras[next].GetComponentInParent<CharacterController>().enabled = true;
        }
        currentcamera = cameras[next];
    }

    public bool ChangeScrap(int amount)
    {
        if (amount < 0 && scrap + amount < 0)
        {
            return false;
        }
        scrap += amount;
        actions.onScrapChanged();
        return true;
    }

    public void ChangeDestination(Destination newdest)
    {
        currentdestination = newdest;
        actions.onDestinationChanged();

    }

    public void GenerateDungeon()
    {
        GameObject dungeon = new GameObject("Dungeon");
        Dungeon script = dungeon.AddComponent<Dungeon>();
        currentstructure = script;
        StartCoroutine(script.GenerateRdmDivDungeon());
        
        //script.GenerateRooms(derelict, 32, 32);
    }

    public void GenerateSepSteerDungeon()
    { 

        GameObject dungeon = new GameObject("dungeon");
        
        
        for (int i = 0; i < size; i++)
        {
            GameObject newroom = new GameObject(string.Format("Room {0}", i));
            newroom.transform.parent = dungeon.transform;
            newroom.transform.position = new Vector3(Random.insideUnitSphere.x * 20, transform.position.y, Random.insideUnitSphere.z * 20);

            newroom.AddComponent<BoxCollider>();
            newroom.GetComponent<BoxCollider>().size = new Vector3(Random.Range(1,8), 1, Random.Range(1,8));
            roomcolliders[i] = newroom.GetComponent<BoxCollider>();
        }
        Debug.Log("generated rooms");
        
        move = true;

                    bool intersects = false;

                foreach (BoxCollider collider1 in roomcolliders)
                {
                    foreach (BoxCollider collider2 in roomcolliders)
                    {
                        if (collider1.bounds.Intersects(collider2.bounds) && collider1 != collider2)
                        {
                            Vector3 dir = (collider1.transform.position - collider2.transform.position).normalized * 0.55f;
                            Debug.Log(dir);
                            Vector3 newpos1 = collider1.transform.position += dir;
                            Vector3 newpos2 = collider2.transform.position -= dir;
                            collider1.transform.position = new Vector3(Mathf.Round(newpos1.x), Mathf.Round(newpos1.y), Mathf.Round(newpos1.z));
                            collider2.transform.position = new Vector3(Mathf.Round(newpos2.x), Mathf.Round(newpos2.y), Mathf.Round(newpos2.z)); 
                            if (collider1.bounds.Intersects(collider2.bounds)) 
                            {
                                intersects = true;
                            }
                            Debug.Log("moved");
                        }
                    }
                }

            if (!intersects){move = false;}
    }

    public class RoomGraph
    {
        public List<List<Room>> islands;

        public RoomGraph()
        {
            islands = new List<List<Room>>();
        }

        public void UpdateIslands()
        {
            List<List<Room>> connectedRoomLists = new List<List<Room>>();
            HashSet<Room> visitedRooms = new HashSet<Room>();

            foreach (Room room in GameDirector.current.assets.rooms)
            {
                if (!visitedRooms.Contains(room))
                {
                    List<Room> connectedRooms = new List<Room>();
                    VisitRoom(room, connectedRooms, visitedRooms);
                    connectedRoomLists.Add(connectedRooms);
                }
            }
            islands = connectedRoomLists;
        }

        private void VisitRoom(Room room, List<Room> connectedRooms, HashSet<Room> visitedRooms)
        {
            visitedRooms.Add(room);
            connectedRooms.Add(room);

            foreach (Door door in room.doors)
            {
                if (door.isopen)
                {
                    foreach (Room connectedRoom in door.connectedrooms)
                    {
                        if (!visitedRooms.Contains(connectedRoom))
                        {
                            VisitRoom(connectedRoom, connectedRooms, visitedRooms);
                        }
                    }
                }
            }
        }
    }

    public class Actions
    {
        public System.Action<int> onDoorToggle;
        public System.Action<int, int> onTeleportRequest;
        public System.Action onScrapChanged;
        public System.Action onDestinationChanged;
        public System.Action onPowerChanged;
    }

    public class LoadedAssets
    {
        public Dictionary<string, int> indexmanager;
        public List<Room> rooms;
        public List<Door> doors;
        public List<Teleporter> teleporters;
        public List<Planet> planets;
        public List<Destination> destinations;
        public List<PowerInlet> inlets;
        public Terminal terminal;

        public LoadedAssets()
        {
            indexmanager = new Dictionary<string, int> {{"doors", 0}, {"rooms", 0}, {"teles", 0}, {"planets", 0}, {"destinations", 0}, {"inlets", 0}};
            rooms = new List<Room>();
            doors = new List<Door>();
            teleporters = new List<Teleporter>();
            inlets = new List<PowerInlet>();
            planets = new List<Planet>();
            destinations = new List<Destination>();
        }
        public int GetIndex(string category)
        {
            int i = indexmanager[category];
            indexmanager[category] ++;
            return i;
        }
    }

    public class StarMap
    {
        public GameObject obj;
        public List<Planet> planets;

        public StarMap(List<Planet> newplanets)
        {
            planets = newplanets;
        }
    }
    public class Planet
    {
        public int id;
        public GameObject obj;
        public List<Destination> destinations;
        public Planet(List<Destination> newdestinations, int newid)
        {
            destinations = newdestinations;
            id = newid;
        }
    }
    public class Destination
    {
        public GameObject obj;
        public int id;
        public bool visited;

        public Destination(int newid)
        {
            id = newid;
            visited = false;
        }
    }
}

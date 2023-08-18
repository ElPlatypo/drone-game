using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
public class Dungeon : MonoBehaviour
{
    public List<Vector3[]> roomintersections = new List<Vector3[]>();
    public List<Door> doors = new List<Door>();
    public List<Room> rooms = new List<Room>();
    public List<List<Room>> sections = new List<List<Room>>();
    public int width = 32;
    public int height = 32;
    private bool c;
    public void Start()
    {
        GameDirector.current.actions.onPowerChanged += UpdatePower;
        GameDirector.current.actions.onDoorToggle += ToggleDoor;
    }

    public void ToggleDoor(int id)
    {
        foreach (Door door in doors)
        {
            if (door.id == id)
            {
                if (door.isopen)
                {
                    door.CloseDoor();
                }
                else if (!door.isopen)
                {
                    door.OpenDoor();
                }
            }
        }
        GameDirector.current.GetComponentInChildren<NavMeshManager>().navmesh.BuildNavMesh();
    }
    public IEnumerator GenerateRdmDivDungeon()
    {
        GameObject room = new GameObject("initial room");
        room.transform.parent = transform;
        BoxCollider col = room.AddComponent<BoxCollider>();
        col.size = new Vector3(32, 1, 32);
        List<BoxCollider> newrooms = new List<BoxCollider>() {col};
        float bias = 2f;
        for (int i = 0; i < 5; i++)
        {   
            foreach (BoxCollider box in newrooms)
            {
                //randomlt split along x, z axis or skip subdivision
                float value = Random.Range(0,bias);
                if (value <= 2)
                {
                    GameObject newroom1 = new GameObject();
                    GameObject newroom2 = new GameObject();
                    Rigidbody rigidbody1 = newroom1.AddComponent<Rigidbody>();
                    Rigidbody rigidbody2 = newroom2.AddComponent<Rigidbody>();
                    rigidbody1.constraints = RigidbodyConstraints.FreezeAll;
                    rigidbody1.useGravity = false;
                    rigidbody2.constraints = RigidbodyConstraints.FreezeAll;
                    rigidbody2.useGravity = false;
                    newroom1.transform.parent = box.transform.parent;
                    newroom2.transform.parent = box.transform.parent;
                    newroom1.transform.position = box.transform.position;
                    newroom2.transform.position = box.transform.position;
                    if (value <= 1)
                    {
                        float offset = box.size.x / 4;
                        newroom1.transform.position += new Vector3(offset, 0, 0);
                        newroom2.transform.position -= new Vector3(offset, 0, 0);
                        newroom1.AddComponent<BoxCollider>().size = new Vector3(offset * 2, 1, box.size.z);
                        newroom2.AddComponent<BoxCollider>().size = new Vector3(offset * 2, 1, box.size.z);
                        DestroyImmediate(box.gameObject);
                    }
                    else if (value > 1)
                    {
                        float offset = box.size.z / 4;
                        newroom1.transform.position += new Vector3(0, 0, offset);
                        newroom2.transform.position -= new Vector3(0, 0, offset);
                        newroom1.AddComponent<BoxCollider>().size = new Vector3(box.size.x, 1, offset * 2);
                        newroom2.AddComponent<BoxCollider>().size = new Vector3(box.size.x, 1, offset * 2); 
                        DestroyImmediate(box.gameObject);
                    }
                }
            }
            bias += 1f;
            newrooms.Clear();
            foreach (BoxCollider box in GetComponentsInChildren<BoxCollider>())
            {
                newrooms.Add(box);
            }
        }
        foreach (Transform roomx in transform)
        {
            roomx.gameObject.AddComponent<Room>();
            roomx.gameObject.layer = LayerMask.NameToLayer("Rooms");
        }
        yield return new WaitForFixedUpdate();
        EnumerateRoom();
        GenerateDoors();
        GenerateSections(2);
        yield return new WaitForFixedUpdate();
        GenerateGeometry();
        yield return new WaitForFixedUpdate();
        GameDirector.current.GetComponentInChildren<NavMeshManager>().navmesh.BuildNavMesh();
        GenerateLoot();
        //SpawnDrones();
        yield return new WaitForFixedUpdate();
        SpawnEnemies();
        GenerateInlets();
    }
    private void EnumerateRoom()
    {
        foreach (BoxCollider box in GetComponentsInChildren<BoxCollider>())
        {
            box.gameObject.GetComponent<BoxCollider>().isTrigger = true;
            rooms.Add(box.GetComponent<Room>());
        }
    }
    private void GenerateDoors()
    {
        List<Vector3[]> neighbours = new List<Vector3[]>();
        List<Vector3[]> finalneighbours = new List<Vector3[]>();
        foreach (Room room in GetComponentsInChildren<Room>())
        {
            neighbours.AddRange(room.neighbours);
        }
        foreach (Vector3[] neighbour1 in neighbours)
        {
            bool x = false;
            foreach (Vector3[] neighbour2 in finalneighbours)
            {
                if(neighbour1[0] == neighbour2[0] && neighbour1[1] == neighbour2[1])
                {
                    x = true;
                }
                else if (neighbour1[0] == neighbour1[1])
                {
                    x = true;
                }
            }
            if(!x)
            {
                finalneighbours.Add(neighbour1);
                Debug.DrawLine(neighbour1[0], neighbour1[1], Color.HSVToRGB(Random.value, 1, 1), 1000);
            }
        }
        roomintersections = finalneighbours;
        for(int i = 0; i < finalneighbours.Count; i++)
        {
            GameObject door = Instantiate(Resources.Load("Prefabs/Door/Door") as GameObject, transform, true);   
            if (finalneighbours[i][0].x == finalneighbours[i][1].x)
            {
                float zpos = Mathf.Round(Random.Range(Mathf.Min(finalneighbours[i][0].z, finalneighbours[i][1].z) + 1, Mathf.Max(finalneighbours[i][0].z, finalneighbours[i][1].z) - 1));
                door.transform.position = new Vector3(finalneighbours[i][0].x, 0, zpos);  
            }
            else if (finalneighbours[i][0].z == finalneighbours[i][1].z)
            {
                float xpos = Mathf.Round(Random.Range(Mathf.Min(finalneighbours[i][0].x, finalneighbours[i][1].x) + 1, Mathf.Max(finalneighbours[i][0].x, finalneighbours[i][1].x) - 1));
                door.transform.position = new Vector3(xpos, 0, finalneighbours[i][0].z);
                door.transform.Rotate(0, 90, 0);
            }
            door.gameObject.layer = LayerMask.NameToLayer("Doors");
            doors.Add(door.GetComponent<Door>());
        }
    }
    private void GenerateGeometry()
    {
        //floors
        foreach (Room room in GetComponentsInChildren<Room>())
        {
            BoxCollider box = room.GetComponent<BoxCollider>();
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "floor";
            floor.transform.position = new Vector3(box.transform.position.x, box.transform.position.y - 0.5f, box.transform.position.z);
            floor.transform.localScale = box.bounds.size * 0.1f;
            floor.GetComponent<MeshCollider>().convex = true;
            floor.transform.parent = box.transform;
        }
        //perimeter
        GameObject wall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject wall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject wall3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject wall4 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        wall1.transform.position = new Vector3(height / 2, 0, 0);
        wall2.transform.position = new Vector3(-height / 2, 0, 0);
        wall3.transform.position = new Vector3(0, 0, width / 2);
        wall4.transform.position = new Vector3(0, 0, -width / 2);

        wall1.transform.localScale = new Vector3(1, 1, width);
        wall2.transform.localScale = new Vector3(1, 1, width);
        wall3.transform.localScale = new Vector3(height, 1, 1);
        wall4.transform.localScale = new Vector3(height, 1, 1);

        wall1.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Walls") as Material;
        wall2.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Walls") as Material;
        wall3.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Walls") as Material;
        wall4.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Walls") as Material;

        wall1.transform.parent = GameDirector.current.currentstructure.transform;
        wall2.transform.parent = GameDirector.current.currentstructure.transform;
        wall3.transform.parent = GameDirector.current.currentstructure.transform;
        wall4.transform.parent = GameDirector.current.currentstructure.transform;

        //walls
        foreach (Vector3[] intersection in GetComponent<Dungeon>().roomintersections)
        {
            RaycastHit hit1;
            Vector3 dir1 = intersection[1] - intersection[0] + new Vector3(0, 0.3f, 0);
            Debug.DrawRay(intersection[0] - dir1 * 0.001f, dir1, Color.black, 10000);
            if (Physics.Raycast(intersection[0] - dir1 * 0.001f, dir1, out hit1, 100f, 1<<LayerMask.NameToLayer("Doors")) && hit1.distance > 0.4)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.position = new Vector3((intersection[0].x + hit1.point.x)/2f, 0, (intersection[0].z + hit1.point.z)/2f);
                if (Mathf.Round(intersection[0].x) == Mathf.Round(hit1.point.x))
                {
                    wall.transform.localScale = new Vector3(1, 1, hit1.distance);
                }
                else if (Mathf.Round(intersection[0].z) == Mathf.Round(hit1.point.z))
                {
                    wall.transform.localScale = new Vector3(hit1.distance, 1, 1f);
                }
                wall.transform.parent = transform;
                int? id = GetNavMeshAgentID("Drone");
                NavMeshModifier meshmod = wall.AddComponent<NavMeshModifier>();
                if (id != null)
                {
                    meshmod.AffectsAgentType(id.Value);
                }
                meshmod.overrideArea = true;
                meshmod.area = 1;
                wall.name = "wall";
                wall.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Walls") as Material;
            }
            RaycastHit hit2;
            Vector3 dir2 = intersection[0] - intersection[1];
            if (Physics.Raycast(intersection[1] - dir2 * 0.001f, dir2, out hit2, 100f, 1<<LayerMask.NameToLayer("Doors")) && hit2.distance > 0.4)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.position = new Vector3((intersection[1].x + hit2.point.x)/2f, 0, (intersection[1].z + hit2.point.z)/2f);
                if (Mathf.Round(intersection[1].x) == Mathf.Round(hit2.point.x))
                {
                    wall.transform.localScale = new Vector3(1, 1, hit2.distance);
                }
                else if (Mathf.Round(intersection[1].z) == Mathf.Round(hit2.point.z))
                {
                    wall.transform.localScale = new Vector3(hit2.distance, 1, 1f);
                }
                wall.transform.parent = transform;
                int? id = GetNavMeshAgentID("Drone");
                NavMeshModifier meshmod = wall.AddComponent<NavMeshModifier>();
                if (id != null)
                {
                    meshmod.AffectsAgentType(id.Value);
                }
                meshmod.overrideArea = true;
                meshmod.area = 1;
                wall.name = "wall";
                wall.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Walls") as Material;
            }        
        }
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
    public Vector3? RandomLocationInDungeon(int tries)
    {
        float x = GetComponent<Dungeon>().width / 2;
        float z = GetComponent<Dungeon>().height / 2;
        for(int i = 0; i < tries; i++)
        {
            Vector3 pos = new Vector3(Mathf.Round(Random.Range(-x,x)), 0, Mathf.Round(Random.Range(-z,z)));
            if(Physics.OverlapSphere(pos, 0.3f, ~ LayerMask.GetMask("Rooms")).Length == 0)
            {
                return pos;
            }
        }
        return null;
    }

    public Vector3? RandomLocarionInRoom(int roomid, int tries)
    {
        foreach (Room room in GetComponentsInChildren<Room>())
        {
            if (room.id == roomid)
            {
                float x = (room.GetComponent<BoxCollider>().size.x / 2) - 1;
                float z = (room.GetComponent<BoxCollider>().size.z / 2) - 1;
                x = Random.Range(-x, x);
                z = Random.Range(-z, z);
                Vector3 pos = new Vector3(Mathf.Round(x), 0, Mathf.Round(z));
                return room.transform.TransformPoint(pos);
            }
        }
        return null;
    }

    private void GenerateLoot()
    {
        int amount = (int) Mathf.Round(Random.Range(4, 8));

        for (int i = 0; i < amount; i++)
        {
            Vector3 pos1 = RandomLocationInDungeon(100).Value;
            GameObject scrap = Instantiate(Resources.Load("Prefabs/Scrap/Scrap") as GameObject, pos1, Quaternion.identity);
            scrap.transform.parent = GameDirector.current.currentstructure.transform;
        }
        Vector3 pos2 = RandomLocationInDungeon(100).Value;
        GameObject tele = Instantiate(Resources.Load("Prefabs/Teleporter") as GameObject, pos2 - new Vector3(0, 0.5f, 0), Quaternion.identity);
        tele.transform.parent = GameDirector.current.currentstructure.transform;
    }

    private void SpawnDrones()
    {
        int id = Random.Range(0, rooms.Count);
        var pos = RandomLocarionInRoom(id, 10);
        if (pos != null)
        {
            GameObject drone = Instantiate(Resources.Load("Prefabs/Drone/Drone"), pos.Value, Quaternion.identity) as GameObject;
            //GameDirector.current.player = drone;
        }
    }

    private void SpawnEnemies()
    {
        foreach (Room room in rooms)
        {
            float v = Random.value;
            if (room.drones.Count == 0 && v > 0.5f)
            {
                var pos = RandomLocarionInRoom(room.id, 10);
                if (pos != null)
                {
                    GameObject enemy = Instantiate(Resources.Load("Prefabs/Enemy/Enemy"), pos.Value, Quaternion.identity) as GameObject;
                    enemy.transform.parent = GameDirector.current.currentstructure.transform;
                }
            }
        }
    }

    private void GenerateSections(int amount)
    {
        List<List<Room>> islands = new List<List<Room>>();
        HashSet<Room> roomset = new HashSet<Room>(rooms);
        List<int> selectedids = new List<int>();

        for (int i = 0; i < amount; i++)
        {
            int index;
            do
            {
                index = Random.Range(0, rooms.Count);
            }
            while (selectedids.Contains(index));
            List<Room> island = new List<Room>();
            Room selroom = rooms[index];
            island.Add(selroom);
            roomset.Remove(selroom);
            selectedids.Add(index);
            islands.Add(island);
        }

        float radius = 0.5f;
        do
        {
            foreach (List<Room> island in islands)
            {
                List<Room> closerooms = new List<Room>();
                foreach (Room room in roomset)
                {
                    float distance = Vector3.Distance(room.transform.position, island[0].transform.position);
                    if (distance < radius)
                    {
                        closerooms.Add(room);
                    }
                }
                foreach (Room room in closerooms)
                {
                    roomset.Remove(room);
                    island.Add(room);
                }
            }
            radius += 0.1f;
        }
        while (roomset.Count > 0);

        int k=0;
        foreach (List<Room> island in islands)
        {
            foreach (Room room in island)
            {
                GameObject a = new GameObject("r" + k);
                a.transform.position = room.transform.position;
            }
            k++;
        }
        Debug.Log(islands.Count + "---" + islands[0].Count);
        Debug.Log(roomset.Count);
        sections = islands;
    }

    private void GenerateInlets()
    {
        foreach (List<Room> section in sections)
        {
            Room randroom = section[Random.Range(0, section.Count)];
            GameObject inlet = Instantiate(Resources.Load("Prefabs/PowerInlet"), randroom.transform) as GameObject;
            var pos = RandomLocarionInRoom(randroom.id, 100) - new Vector3(0, 0.5f, 0);
            if (pos != null)
            {
                inlet.transform.position = pos.Value;
                inlet.GetComponent<PowerInlet>().room = randroom.id;
            }
        }
    }

    public void UpdatePower()
    {
        foreach (List<Room> sect in sections)
        {
            foreach (Room room in sect)
            {
                room.ispowered = false;
            }
        }
        foreach (PowerInlet inlet in GameDirector.current.assets.inlets)
        {
            foreach (List<Room> section in sections)
            {
                bool c = false;
                foreach (Room room in section)
                {
                    if (inlet.room == room.id && inlet.ispowered)
                    {
                        c = true;
                    }
                }
                if (c)
                {
                    foreach (Room room in section)
                    {
                        room.ispowered = true;
                    }
                }
            }
        }

        foreach (Room room in rooms)
        {
            if (room.ispowered)
            {
                foreach (Door door in room.doors)
                {
                    door.ispowered = true;
                }
            }
        }
    }

    public List<AbstractRoom> GenerateRooms(GameObject derelict, int x, int z)
    {
        bool stop = false;
        List<AbstractRoom> abstractrooms = new List<AbstractRoom>();
        
        //initialize first room
        AbstractRoom initialroom = new AbstractRoom(new Vector3(x/2, 0, z/2), new Vector3(-x/2, 0, z/2), new Vector3(-x/2, 0, -z/2), new Vector3(x/2, 0, -z/2));
        abstractrooms.Add(initialroom);

        float bias = -1;
        while(!stop)
        {
            List<AbstractRoom> newrooms = new List<AbstractRoom>();
            stop = true;
            foreach (AbstractRoom room in abstractrooms)
            { 
                float r = Random.Range(0,3 + bias);
                if(r < 1 && room.x > 4 && room.cont == true)
                {
                    stop = false;
                    Vector3 new1 = new Vector3((room.e1.x + room.e2.x) / 2, 0, (room.e1.z + room.e2.z) / 2);
                    Vector3 new2 = new Vector3((room.e3.x + room.e4.x) / 2, 0, (room.e3.z + room.e4.z) / 2);
                    AbstractRoom room1 = new AbstractRoom(room.e1, new1, new2, room.e4);
                    AbstractRoom room2 = new AbstractRoom(new1, room.e2, room.e3, new2);
                    newrooms.Add(room1);
                    newrooms.Add(room2);
                }
                else if (r >= 1 && r < 2 && room.z > 4 && room.cont == true)
                {
                    stop = false;
                    Vector3 new1 = new Vector3((room.e1.x + room.e4.x) / 2, 0, (room.e1.z + room.e4.z) / 2);
                    Vector3 new2 = new Vector3((room.e2.x + room.e3.x) / 2, 0, (room.e2.z + room.e3.z) / 2);
                    AbstractRoom room1 = new AbstractRoom(room.e1, room.e2, new2, new1);
                    AbstractRoom room2 = new AbstractRoom(new1, new2, room.e3, room.e4);
                    newrooms.Add(room1);
                    newrooms.Add(room2);
                }
                else
                {
                    room.cont = false;
                    newrooms.Add(room);
                }
            }
            bias += 0.5f;
            abstractrooms = newrooms;
        }

        int i = 0;
        foreach (AbstractRoom room in abstractrooms)
        {
            GameObject newroom = new GameObject("Room " + i);
            newroom.transform.position = room.center;
            BoxCollider col = newroom.AddComponent<BoxCollider>();
            col.size = new Vector3(room.x, 1, room.z);
            col.isTrigger = true;
            newroom.transform.parent = derelict.transform;
            i++;
        }
        return abstractrooms;
    }

    public class AbstractRoom
    {
        public Vector3 e1;
        public Vector3 e2;
        public Vector3 e3;
        public Vector3 e4;
        public Vector3[] edges = new Vector3[4];
        public float x;
        public float z;
        public Vector3 center;
        public bool cont = true;

        public AbstractRoom(Vector3 a1, Vector3 a2, Vector3 a3, Vector3 a4)
        {
            e1 = a1;
            edges[0] = e1;
            e2 = a2;
            edges[1] = e2;
            e3 = a3;
            edges[2] = e3;
            e4 = a4;
            edges[3] = e4;

            x = Mathf.Abs(a1.x - a2.x);
            z = Mathf.Abs(a1.z - a4.z);
            center = new Vector3((a1.x+a2.x+a3.x+a4.x)/4, 0, (a1.z+a2.z+a3.z+a4.z)/4);
        }
    }
}

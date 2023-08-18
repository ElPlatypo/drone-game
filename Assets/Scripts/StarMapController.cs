using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StarMapController : MonoBehaviour
{
    public GameDirector.StarMap starmap;
    private List<GameObject> planetpanels;
    private GameObject mappanel;
    private GameObject shipicon;

    void Start()
    {
        planetpanels = new List<GameObject>();
        GenerateStarmap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateStarmap()
    {
        List<GameDirector.Planet> newmap = new List<GameDirector.Planet>();
        for (int i = 0; i < 6; i++)
        {
            List<GameDirector.Destination> destlist = new List<GameDirector.Destination>();
            for (int k = 0; k < 10; k++)
            {
                GameDirector.Destination newdest = new GameDirector.Destination(GameDirector.current.assets.GetIndex("destinations"));
                destlist.Add(newdest);
            }
            GameDirector.Planet newplanet = new GameDirector.Planet(destlist, GameDirector.current.assets.GetIndex("planets"));
            newmap.Add(newplanet);
        }
        starmap = new GameDirector.StarMap(newmap);
        GenerateUI(starmap);
    }

    private void GenerateUI(GameDirector.StarMap map)
    {
        mappanel = Instantiate(Resources.Load("Prefabs/UIPanel"), transform) as GameObject;
        mappanel.name = "MapPanel";
        map.obj = mappanel;
        float w = Screen.width / 2 - 50;
        float h = Screen.height / 2 - 50;
        foreach (GameDirector.Planet planet in map.planets)
        {
            Vector2 pos1 = new Vector2(Random.Range(-w, w), Random.Range(-h, h));
            GameObject panel = Instantiate(Resources.Load("Prefabs/UIPanel"), transform) as GameObject;
            panel.name = "Planet " + planet.id;
            planet.obj = panel;
            planetpanels.Add(panel);

            GameObject planetbutton = Instantiate(Resources.Load("Prefabs/UIDestination"), mappanel.transform) as GameObject;
            planetbutton.GetComponent<RectTransform>().anchoredPosition = pos1;
            planetbutton.GetComponent<Button>().onClick.AddListener(delegate {OpenPlanetPanel(panel);});
            planetbutton.GetComponentInChildren<TextMeshProUGUI>().text = panel.name;

            GameObject backbutton = Instantiate(Resources.Load("Prefabs/UIDestination"), panel.transform) as GameObject;
            backbutton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-w + 30, h);
            backbutton.GetComponent<Button>().onClick.AddListener(delegate {BackToPlanets();});
            backbutton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";

            foreach (GameDirector.Destination dest in planet.destinations)
            {
                Vector2 pos2 = new Vector2(Random.Range(-w, w), Random.Range(-h, h));
                GameObject destbutton = Instantiate(Resources.Load("Prefabs/UIDestination"), panel.transform) as GameObject;
                destbutton.GetComponent<RectTransform>().anchoredPosition = pos2;
                destbutton.name = "dest " + dest.id;
                dest.obj = destbutton;
                destbutton.GetComponentInChildren<TextMeshProUGUI>().text = destbutton.name;
                destbutton.GetComponent<Button>().onClick.AddListener(delegate {ClickDestination(dest);});
                destbutton.GetComponent<Button>().interactable = false;
            }
            panel.SetActive(false);
        }
        shipicon = Instantiate(Resources.Load("Prefabs/ShipIcon"), map.planets[0].obj.transform) as GameObject;
        shipicon.GetComponent<RectTransform>().anchoredPosition = map.planets[0].destinations[0].obj.GetComponent<RectTransform>().anchoredPosition;
        shipicon.transform.SetSiblingIndex(0);

        GameDirector.current.currentplanet = map.planets[0];
        GameDirector.current.currentdestination = map.planets[0].destinations[0];
        GameDirector.current.currentdestination.visited = true;
        ActivateInRange(map.planets[0].destinations[0].obj.GetComponent<RectTransform>().anchoredPosition);
    }

    public void ClickDestination(GameDirector.Destination dest)
    {
        if (GameDirector.current.currentplanet.destinations.Contains(dest))
        {
            if (!dest.visited && GameDirector.current.currentdestination != dest)
            {
                GameDirector.current.currentdestination = dest;
                shipicon.GetComponent<RectTransform>().anchoredPosition = dest.obj.GetComponent<RectTransform>().anchoredPosition;
                ActivateInRange(dest.obj.GetComponent<RectTransform>().anchoredPosition);
                Destroy(GameDirector.current.currentstructure.gameObject);
                GameDirector.current.GenerateDungeon();
                dest.visited = true;
            }
        }
    }

    private void ActivateInRange(Vector2 pos)
    {
        foreach (GameDirector.Destination dest in GameDirector.current.currentplanet.destinations)
        {
            Vector2 posa = dest.obj.GetComponent<RectTransform>().anchoredPosition;
            Vector2 posb = shipicon.GetComponent<RectTransform>().anchoredPosition;
            float dist = Mathf.Sqrt(Mathf.Pow((posa.x - posb.x),2) + Mathf.Pow((posa.y - posb.y),2));
            if (dist < 500)
            {
                dest.obj.GetComponent<Button>().interactable = true;
            }
        }
    }

    public void OpenPlanetPanel(GameObject panel)
    {
        mappanel.SetActive(false);
        panel.SetActive(true);
    }

    public void BackToPlanets()
    {
        foreach (GameObject panel in planetpanels)
        {
            panel.SetActive(false);
        }
        mappanel.SetActive(true);
    }

    
}

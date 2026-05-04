using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
    public GameObject map;
    // public GameObject mapPosition;
    //map position and rotation (-90 -45 -90) rotation
    private Vector3 mapPosition = new (0f, 0f, 0f);
    private Vector3 mapRotation = new (-90f, -45f, -90f);
    private Vector3 offset = new (0, 0.5f, -0.5f);
    [SerializeField] private GameObject menu;
    private GameObject mapObj;
    private readonly string mapName = "Map(Clone)";  // Name of the map object
    private bool isMapSpawned = false;
    public void SpawnMap()
    {
        if (isMapSpawned)
        {
            // map repositioning
            mapObj.transform.position = menu.transform.position + offset;
            return;
        }
        mapObj = Instantiate(map, mapPosition, map.transform.rotation);
        mapObj.name = mapName; 
        isMapSpawned = true;  
        mapObj.transform.SetPositionAndRotation(menu.transform.position + offset, Quaternion.Euler(mapRotation));
    }
    
}

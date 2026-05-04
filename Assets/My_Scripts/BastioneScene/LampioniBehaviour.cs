using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampioniBehaviour : MonoBehaviour
{
    private Light[] lampioni;
    // Start is called before the first frame update
    void Start()
    {
        lampioni = GetComponentsInChildren<Light>();
        foreach (Light l in lampioni)
        {
            l.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

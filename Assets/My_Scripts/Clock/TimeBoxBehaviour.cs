using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class TimeBoxBehaviour : MonoBehaviour
{
    private GameObject hour;
    // Start is called before the first frame update
    void Start()
    {
        hour = GameObject.Find("Hour");
        gameObject.GetComponent<InteractableUnityEventWrapper>().WhenSelect.AddListener(GetTime);
    }

    public void GetTime()
    {
        hour.GetComponent<HourBehaviour>().SetHour(int.Parse(gameObject.name));
        // I am the box 0, 1, 2, ..., 14
    }
}

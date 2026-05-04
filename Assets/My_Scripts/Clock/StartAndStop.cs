using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class StartAndStop : MonoBehaviour
{
    private HourBehaviour hourBehaviour;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<InteractableUnityEventWrapper>().WhenSelect.AddListener(Select);
        hourBehaviour = GameObject.Find("Hour").GetComponent<HourBehaviour>();
    }
    public void Select()
    {
        // I am the start or stop button
        hourBehaviour.SetInitialOrFinalHour(gameObject);
    }
}

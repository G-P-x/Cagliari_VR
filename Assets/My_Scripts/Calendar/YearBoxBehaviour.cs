using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is attached to the YearBox GameObjects (2021, 2022, ..., 2025).
/// </summary>
public class YearBoxBehaviour : MonoBehaviour
{
    private GameObject yearCanvas;
    // Start is called before the first frame update
    void Start()
    {
        yearCanvas = GameObject.Find("YearsCanvas");
    }

    public void OnToggle()
    {
        if(gameObject.GetComponent<Toggle>().isOn)
        {
            Debug.Log("[STEP] 1: OnToggle " + gameObject.name);
            yearCanvas.GetComponent<YearCanvasBehaviour>().OnToggleValueChanged(gameObject);
            
        }
    }
}

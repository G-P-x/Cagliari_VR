using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanelsManager : MonoBehaviour
{
    public GameObject DateSelection, GapSelection, TimeSelection;
    private bool dateSelected = false, gapSelected = false, timeSelected = false;
    void Start() 
    {
        TimeSelection.SetActive(timeSelected);
        GapSelection.SetActive(gapSelected);
        DateSelection.SetActive(dateSelected);
    }

    public void OpenDateSelection()
    {
        DateSelection.SetActive(true);
        GapSelection.SetActive(false);
        TimeSelection.SetActive(false);
    }

    public void OpenGapSelection()
    {
        gapSelected = !gapSelected;
        GapSelection.SetActive(gapSelected);
        DateSelection.SetActive(false);
        TimeSelection.SetActive(false);
    }

    public void OpenTimeSelection()
    {
        TimeSelection.SetActive(true);
        DateSelection.SetActive(false);
        GapSelection.SetActive(false);

    }
}

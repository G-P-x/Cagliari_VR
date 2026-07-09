using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Oculus.Interaction;

/// <summary>
/// This class is attached to the DayBox GameObjects (0, 1, ..., 41).
/// </summary>
public class DayBoxBehaviour : MonoBehaviour
{
    private TextMeshPro dayText;
    private Date date;
    private InteractableUnityEventWrapper eventWrapper;
    // Start is called before the first frame update
    void Start()
    {
        dayText = GetComponentInChildren<TextMeshPro>();
        date = GameObject.Find("Data").GetComponent<Date>();
        eventWrapper = GetComponent<InteractableUnityEventWrapper>();
        eventWrapper.WhenSelect.AddListener(GetDate);
    }

    public void GetDate()
    {
        // I am the box 0, 1, 2, ..., 41
        // date.OnDayClick(int.Parse(gameObject.name));
        date.SetSelectedDate(int.Parse(gameObject.name));
    }
}

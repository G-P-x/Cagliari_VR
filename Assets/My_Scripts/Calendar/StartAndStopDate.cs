using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// This class is attached to the Start and Stop Date GameObjects.
/// </summary>
public class StartAndStopDate : MonoBehaviour
{
    private Date date;
    // Start is called before the first frame update
    void Start()
    {
        date = GameObject.Find("Data").GetComponent<Date>();
        gameObject.GetComponent<InteractableUnityEventWrapper>().WhenSelect.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        date.SetInitialOrFinalDate(gameObject);
    }
}

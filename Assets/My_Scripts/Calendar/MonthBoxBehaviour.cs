using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Oculus.Interaction;
using UnityEngine.Assertions;

/// <summary>
/// This class is attached to the MonthBox GameObjects (Gennaio, Febbraio,..., Dicembre).
/// </summary>
public class MonthBoxBehaviour : MonoBehaviour
{
    private TextMeshPro monthText;
    private InteractableUnityEventWrapper monthInteractable;
    
    private GameObject monthPanel;
    private InteractableColorVisual monthColorVisual;
    // Start is called before the first frame update
    void Start()
    {
        monthPanel = GameObject.Find("MonthPanel");
        monthText = monthPanel.GetComponentInChildren<TextMeshPro>();
        monthInteractable = GetComponent<InteractableUnityEventWrapper>();
        monthInteractable.WhenUnselect.AddListener(MonthBoxClicked);
        gameObject.GetComponentInChildren<InteractableColorVisual>().enabled = false;
        /*
        * Disabilito il componente InteractableColorVisual perché essendo questo attaccato ad un oggetto che viene
        * disattivato e riattivato, il componente InteractableColorVisual genera un errore di esecuzione 
        * della Coroutine "ChangeColor" che non riesce a trovare il componente Renderer dell'oggetto.
        * Questo errore non è bloccante, ma preferisco evitarlo.
        * L'effetto visivo non c'è più, ma l'errore non viene più generato.
        */
    }

    public void MonthBoxClicked()
    {
        string month = gameObject.name;
        // update month selected
        monthText.text = month;
        // open day panel
        monthPanel.GetComponent<DatePanelsBehaviour>().OpenDays(month, null);        
    }
}

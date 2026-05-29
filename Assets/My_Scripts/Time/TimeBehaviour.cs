using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeBehaviour : MonoBehaviour
{
    
    [Tooltip("Altri campi orari che devono essere deselezionati quando questo viene selezionato questo campo")]
    public GameObject[] othersTimeFields;

    /// <summary>
    /// Indica se l'oggetto a cui è attaccato lo script è stato selezionato
    /// </summary>
    private bool isSelected = false;

    /// <summary>
    /// Immagine di sfondo del campo testo
    /// </summary>
    private UnityEngine.UI.Image myImage;   

    private UnityEngine.UI.Image[] othersImage;
    private TimeBehaviour[] othersBehaviour;
    private Color selectedColor, deselectedColor;

    private string selectedTime;
    private TextMeshProUGUI textTime;

    // PROPRIETA'
    /// <summary>
    /// ottieni l'orario inserito nel campo Time
    /// </summary>
    public string SelectedTime
    {
        get {return selectedTime; }
    }

    public bool IsSelected
    {
        get { return isSelected; }
        set { isSelected = value; }
    }
    // Start is called before the first frame update
    void Start()
    {
        myImage = gameObject.GetComponentInChildren<UnityEngine.UI.Image>();
        selectedColor = new Color(0f, 0.05f, 0.46f, 1);
        deselectedColor = myImage.color;

        int i = 0;
        othersImage = new UnityEngine.UI.Image[othersTimeFields.Length];
        othersBehaviour = new TimeBehaviour[othersTimeFields.Length];
        foreach (GameObject obj in othersTimeFields)
        {
            othersBehaviour[i] = obj.GetComponent<TimeBehaviour>();
            othersImage[i] = obj.GetComponentInChildren<UnityEngine.UI.Image>();
            i++;
        }
        textTime = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        textTime.text = "";
    }
    // GESTIONE SELEZIONE
    public void Select()
    {
        myImage.color = selectedColor;
        DeselectOthers();
        isSelected = true;
    }
    private void DeselectOthers()
    {
        int i = 0;
        foreach (GameObject obj in othersTimeFields)
        {
            othersBehaviour[i].IsSelected = false;
            othersImage[i].color = deselectedColor;
            i++;
        }
    }

    // GESTIONE INSERIMENTO DATA
    public void SetTime(string input)
    {
        if (isSelected)
        {
            selectedTime = input;
            ShowInputString();
        }
    }

    // VISUALIZZAZIONE DATA INSERITA
    private void ShowInputString()
    {
        textTime.text = selectedTime;
    }
}

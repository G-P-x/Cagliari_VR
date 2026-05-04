using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UserInput;


/// <summary>
/// Dispatcher per la gestione dell'inserimento della data
/// Invia il numero inserito dall'utente a tutti i campi di data
/// </summary>
public class DateBehaviour : MonoBehaviour
{    
    [Tooltip("Altri campi di data che devono essere deselezionati quando questo viene selezionato questo campo")]
    public GameObject[] othersDateFields;

    /// <summary>
    /// Indica se l'oggetto a cui è attaccato lo script è stato selezionato
    /// </summary>
    private bool isSelected = false;
    private InputFormat inputFormat = new();

    /// <summary>
    /// Immagine di sfondo del campo testo
    /// </summary>
    private UnityEngine.UI.Image myImage;   

    private UnityEngine.UI.Image[] othersImage;
    private DateBehaviour[] othersBehaviour;
    private Color selectedColor, deselectedColor;

    /// <summary>
    /// data inserita dall'utente
    /// </summary>
    private List<string> date = new List<string>(); 
    private TextMeshProUGUI textDate;

    // PROPRIETA'
    public bool IsSelected
    {
        get { return isSelected; }
        set { isSelected = value; }
    }

    /// <summary>
    /// ottieni data inserita nello specifico campo Date
    /// </summary>
    public string SelectedDate
    {
        get
        {
            string _data = "";
            foreach (string s in date)
            {
                _data += s;
            }
            return _data;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        myImage = gameObject.GetComponentInChildren<UnityEngine.UI.Image>();
        selectedColor = new Color(0f, 0.05f, 0.46f, 1);
        deselectedColor = myImage.color;

        int i = 0;
        othersImage = new UnityEngine.UI.Image[othersDateFields.Length];
        othersBehaviour = new DateBehaviour[othersDateFields.Length];
        foreach (GameObject obj in othersDateFields)
        {
            othersBehaviour[i] = obj.GetComponent<DateBehaviour>();
            othersImage[i] = obj.GetComponentInChildren<UnityEngine.UI.Image>();
            i++;
        }
        textDate = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        textDate.text = "";
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
        foreach (GameObject obj in othersDateFields)
        {
            othersBehaviour[i].IsSelected = false;
            othersImage[i].color = deselectedColor;
            i++;
        }
    }

    private void DeselectMe()
    {
        myImage.color = deselectedColor;
        isSelected = false;
    }

    // GESTIONE INSERIMENTO DATA
    public void SetNumber(string input)
    {
        if (isSelected)
        {
            if (date.Count >= 10)
            {
                DeselectMe();
                return;
            }
            date.Add(input);     
            if (date.Count == 3 || date.Count == 6)
            {
                date.Insert(date.Count - 1, inputFormat.dateSeparator);
            }
            ShowInputString();
        }
    }

    public void DeleteNumber()
    {
        if (isSelected)
        {
            if (date.Count > 0)
            {
                date.RemoveAt(date.Count - 1);
                ShowInputString();
            }
        }
    }

    public void ClearInput()
    {
        if (isSelected)
        {
            date.Clear();
            ShowInputString();
        }
    }

    // VISUALIZZAZIONE DATA INSERITA
    private void ShowInputString()
    {
        textDate.text = "";
        foreach (string input in date)
        {
            textDate.text += input;
        }
    }

    
}

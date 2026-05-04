using UnityEngine;
using TMPro;


/// <summary>
/// Dispatcher per la gestione dell'inserimento della data
/// Invia il numero inserito dall'utente a tutti i campi di data
/// </summary>
public class YearBehaviour : MonoBehaviour
{    
    /// <summary>
    /// Indica se l'oggetto a cui è attaccato lo script è stato selezionato
    /// </summary>
    private bool isSelected = false;

    /// <summary>
    /// Immagine di sfondo del campo testo
    /// </summary>
    private UnityEngine.UI.Image myImage;   
    private Color selectedColor, deselectedColor;
    private string year = "";
    private TextMeshProUGUI textYear;

    // PROPRIETA'
    public bool IsSelected
    {
        get { return isSelected; }
        set { isSelected = value; }
    }

    public string Year
    {
        get { return year; }
    }

    // Start is called before the first frame update
    void Start()
    {
        myImage = gameObject.GetComponentInChildren<UnityEngine.UI.Image>();
        selectedColor = new Color(0f, 0.05f, 0.46f, 1);
        deselectedColor = myImage.color;
        textYear = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        textYear.text = "";
    }
        
    // GESTIONE SELEZIONE
    public void Select()
    {
        myImage.color = selectedColor;
        isSelected = true;
    }

    /// <summary>
    /// Deseleziona il campo di day ogni volta che viene inserito un numero
    /// </summary>
    private void DeselectMe()
    {
        myImage.color = deselectedColor;
        isSelected = false;
    }

    // GESTIONE INSERIMENTO DATA
    public void SetYear(string input)
    {        
        if (isSelected)
        {
            textYear.text = input;
            year = input;
            DeselectMe();
        }
    }
}

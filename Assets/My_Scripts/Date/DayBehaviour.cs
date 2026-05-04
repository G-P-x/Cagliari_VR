using UnityEngine;
using TMPro;
using UserInput;


/// <summary>
/// Dispatcher per la gestione dell'inserimento della data
/// Invia il numero inserito dall'utente a tutti i campi di data
/// </summary>
public class DayBehaviour : MonoBehaviour
{    
    /// <summary>
    /// Indica se l'oggetto a cui è attaccato lo script è stato selezionato
    /// </summary>
    private bool isSelected = false;
    private InputFormat inputFormat = new();

    /// <summary>
    /// Immagine di sfondo del campo testo
    /// </summary>
    private UnityEngine.UI.Image myImage;   
    private Color selectedColor, deselectedColor;

    /// <summary>
    /// data inserita dall'utente
    /// </summary>

    private string day = "";
    private TextMeshProUGUI textDay;

    // PROPRIETA'
    public bool IsSelected
    {
        get { return isSelected; }
        set { isSelected = value; }
    }

    public string Day
    {
        get { return day; }
    }

    // Start is called before the first frame update
    void Start()
    {
        myImage = gameObject.GetComponentInChildren<UnityEngine.UI.Image>();
        selectedColor = new Color(0f, 0.05f, 0.46f, 1);
        deselectedColor = myImage.color;
        textDay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        textDay.text = "";
    }
        
    // GESTIONE SELEZIONE
    public void Select()
    {
        myImage.color = selectedColor;
        // DeselectOthers();
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
    public void SetDay(string input)
    {
        
        if (isSelected)
        {
            textDay.text = input;
            day = input;
            DeselectMe();
        }
    }
}

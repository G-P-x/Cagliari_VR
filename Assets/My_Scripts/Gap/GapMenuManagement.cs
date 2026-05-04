using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GapMenuManagement : MonoBehaviour
{
    private string gapName = "----";
    /// <summary>
    /// Testo che mostra il nome del varco selezionato
    /// </summary>
    public TextMeshProUGUI gapNameText;

    // PROPRIETA'
    public string SelectedGap
    {
        get { return gapName; }
    }
    public void SetGap(string input)
    {
        gapName = input;        
    }
    public void GetGap()
    {
        gapNameText.text = gapName;
    }
}

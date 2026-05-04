using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor_test : MonoBehaviour
{
    public Material materialToChange;

    public void ChangeColor()
    {
        if (materialToChange != null)
        {
            // Change the color of the material to a random color
            Color newColor = new Color(Random.value, Random.value, Random.value);
            materialToChange.color = newColor;
        }
        else
        {
            Debug.LogWarning("Material to change is not assigned.");
        }
    }
}

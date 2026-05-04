using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public void OnClick()
    {
        if (gameObject.GetComponent<Renderer>().material.color == Color.red)
            gameObject.GetComponent<Renderer>().material.color = Color.white;
        else
            gameObject.GetComponent<Renderer>().material.color = Color.red;
    }
}

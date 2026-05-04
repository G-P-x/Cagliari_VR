using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundMapPixelUpdate : MonoBehaviour
{
    Renderer objRend;
    private GameObject frame;
    private readonly string raggioProperty = "_Raggio";
    private readonly string referenceProperty = "_ReferencePosition";
    private const string frameTag = "Frame"; // Tag for the frame object
    // Start is called before the first frame update
    void Start()
    {
        objRend = GetComponent<Renderer>();
        frame = GameObject.FindGameObjectWithTag(frameTag);
        objRend.material.SetVector(referenceProperty, frame.transform.position);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        objRend.material.SetVector(referenceProperty, frame.transform.position);
        float size = Mathf.Max(frame.transform.localScale.x, frame.transform.localScale.y, frame.transform.localScale.z);
        objRend.material.SetFloat(raggioProperty, size / 2f);   
        // set the radius of the circle to the half of the size of the frame     
    }
}

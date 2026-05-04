using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelsReferenceUpdate : MonoBehaviour
{
    // maybe this script is not used anymore!!!
    [SerializeField] Renderer objRend;
    private GameObject frame;
    private const string frameTag = "Frame"; // Tag for the frame object
    private GameObject city;
    private const string cityTag = "City"; // Tag for the city object
    private readonly string up = "_UpperBound";
    private readonly string down = "_LowerBound";
    private readonly string referenceProperty = "_ReferencePosition";
    private Vector3 upper;
    private Vector3 lower;
    private Vector2 yz;
    private float max_scale;
    private float cityThickness;
    

    // Start is called before the first frame update
    void Start()
    {
        frame = GameObject.FindGameObjectWithTag(frameTag);
        city = GameObject.FindGameObjectWithTag(cityTag);
        max_scale = Mathf.Max(frame.transform.localScale.x, frame.transform.localScale.y, frame.transform.localScale.z);
        cityThickness = 0.4f;
        //float up_x = referenceObj.transform.localScale.x / 2f;
        //float up_y = referenceObj.transform.localScale.y / 2f;
        //float up_z = floor.GetComponent<MeshCollider>().bounds.size.x / 2f;

        // L'oggetto usato come reference, seppure la sua scala
        // e' (0.1,1,0.1), le dimensioni effettive sono 0.1x2x0.1. Per cui posso prendere come riferimento la scala y ( max scale)
        // ma non devo dividere per 2

        // float up_x = referenceObj.transform.localScale.x;
        // float up_y = referenceObj.transform.localScale.y;
        // float up_z = floor.GetComponent<MeshCollider>().bounds.size.x;
        upper = new Vector3(max_scale, max_scale, max_scale);

        // poich� l'oggetto usato come riferimento � stato importato da Blender, usa un sistema destrorso,
        // mentre la posizione dei pixel si riferisce ad un sistema sinistrorso,
        // l'unico asse in comune � l'asse x e dato che la finestra � quadrata ho deciso di utilizzare
        // direttamente l'asse x dell'oggetto come riferimento sia per la posizione x che y dei pixel.

        lower = -upper;

        objRend.material.SetVector(up, upper);
        objRend.material.SetVector(down, lower);
    }

    // Update is called once per frame
    void LateUpdate()
    {        
        (float x, float z) = RotationY();
        upper = new Vector3(x, max_scale, z);
        lower = -upper;
        objRend.material.SetVector(up, upper);
        objRend.material.SetVector(down, lower);
        objRend.material.SetVector(referenceProperty, frame.transform.position);
    }

    private (float x, float z) RotationY()
    {
        max_scale = Mathf.Max(frame.transform.localScale.x, frame.transform.localScale.y, frame.transform.localScale.z);
        
        float y_radiant = frame.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;      
        float x = Mathf.Abs(max_scale * Mathf.Cos(y_radiant));
        float z = Mathf.Abs(max_scale * Mathf.Sin(y_radiant));
        if (x < cityThickness)
            x = cityThickness;
        if (z < cityThickness)
            z = cityThickness;
        return (x, z);
    }
}

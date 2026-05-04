using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HumanBehaviour : MonoBehaviour
{
    void LateUpdate()
    {
        // Vector3 human = gameObject.transform.localPosition;
        gameObject.transform.Translate(-Vector3.right * Time.deltaTime);
        // gameObject.transform.localPosition += new Vector3((-1) * speed * Time.deltaTime, 0, 0);    
    }
}

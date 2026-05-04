using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillerBehaviour : MonoBehaviour
{
    private void OnCollisionEnter(Collision other) 
    {
        if(other.gameObject.CompareTag("Human"))
        {
            Destroy(other.gameObject);
        }        
    }
}

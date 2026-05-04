using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowBehaviour : MonoBehaviour
{
    public GameObject target;
    public GameObject generalMenu;
    // Start is called before the first frame update
    void Start()
    {
        // transform.SetParent(generalMenu.transform);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = generalMenu.transform.localPosition + new Vector3(0, 0.5f, 0);
        transform.LookAt(target.transform.position, worldUp: Vector3.forward);
    }

    // disattiva l'oggetto se la camera vede il target
    private void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }
}

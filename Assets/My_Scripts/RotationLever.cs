using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RotationLever : MonoBehaviour
{
    public GameObject applyRotationTo;
    private Vector3 applyRotationToOffset;
    private Vector3 rotationOffset;
    // Start is called before the first frame update
    void Start()
    {
        rotationOffset = transform.rotation.eulerAngles;
        applyRotationToOffset = applyRotationTo.transform.rotation.eulerAngles;
        

    }

    // Update is called once per frame
    void Update()
    {
        applyRotationTo.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles - rotationOffset + applyRotationToOffset);
    }
}

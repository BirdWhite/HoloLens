using Microsoft.MixedReality.Toolkit.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PovController : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isFixed = false;
    public Vector3 originPos;
    public Quaternion originQuat;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FixPov();
    }

    public void Togglefix()
    {
        isFixed = !isFixed;
        originPos = transform.position;
        originQuat = transform.rotation;
    }
    public void FixPov()
    {
        if (isFixed)
        {
            transform.position = originPos;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, originQuat.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
    }
}

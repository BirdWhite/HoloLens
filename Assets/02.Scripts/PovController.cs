using Microsoft.MixedReality.Toolkit.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PovController : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isfixed = false;
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
        isfixed = !isfixed;
    }
    public void FixPov()
    {
        if (isfixed)
        {

        }
    }

}

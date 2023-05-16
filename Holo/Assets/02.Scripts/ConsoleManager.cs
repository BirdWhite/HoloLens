using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleManager : MonoBehaviour
{
    [SerializeField]
    public TextMesh tM;
    string logText = "";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        tM.text = logText;
    }

    public void UnityLog(string msg)
    {
        logText += msg + "\n";
    }
}

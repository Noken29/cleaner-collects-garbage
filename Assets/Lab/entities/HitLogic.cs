using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitLogic : MonoBehaviour
{
    private Color defaultColor;
    public bool needChange;

    void Start()
    {
        defaultColor = GetComponent<Renderer>().material.color;
        needChange = false;
    }

    void Update()
    {
        if (needChange)
        {
            GetComponent<Renderer>().material.color = Color.red;
            needChange = false;
        } else
        {
            GetComponent<Renderer>().material.color = defaultColor;
        }
    }
}

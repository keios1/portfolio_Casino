using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float settedTime;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, settedTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

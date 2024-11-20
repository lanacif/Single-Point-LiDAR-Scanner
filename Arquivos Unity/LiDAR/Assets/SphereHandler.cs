using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereHandler : MonoBehaviour
{
    public SphereData sphereData;

    void Start()
    {
        sphereData.position = transform.position;
        SaveData.current.spheres.Add(sphereData);
    }

}

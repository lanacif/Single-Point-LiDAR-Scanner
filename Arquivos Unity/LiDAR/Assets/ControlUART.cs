using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class ControlUART : MonoBehaviour
{
    public GameObject SpherePrefab;

    SerialPort sp = new SerialPort("COM3", 115200);

    void Start()
    {
        sp.Open();
        sp.ReadTimeout = 1;
    }

    void OnDestroy()
    {
        sp.Close();
    }

    void Update()
    {
        if(sp.BytesToRead >= 4){
            Spawn(sp.ReadLine());
        }   
    }


    void Spawn(string teste)
    {
        if(teste == "0")
        {
            Instantiate(SpherePrefab, new Vector3(Random.Range(-12, 12), Random.Range(-12, 12), Random.Range(-12, 12)), Quaternion.identity);
        }
        else
            //Instantiate(Sphere, new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), Random.Range(-2, 2)), Quaternion.identity);
            print(teste);
            //teste2
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{

    public static SaveData current;
    
    /*
    private static SaveData _current;

    public static SaveData current
    {
        get
        {
            if (_current == null)
            {
                _current = new SaveData();
            }

            return _current;
        }
    }
    */
    //public List<SphereData> spheres;
    public List<SphereData> spheres = new List<SphereData>();
    
}

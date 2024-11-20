using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Awake()
    {
        // Inicialize SaveData.current quando o jogo começa
        SaveData.current = new SaveData();
        Debug.Log(Application.persistentDataPath);
    }
}
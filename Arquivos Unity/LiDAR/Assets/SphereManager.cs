using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Adicione esta linha

public class SphereManager : MonoBehaviour
{
    public Material PointMaterial; // Atribua um material que utilize um shader adequado para desenhar pontos

    /*
    private void Start()
    {
        OnLoad();
    }
    */

    public void OnSave()
    {
        SerializationManager.Save("cena", SaveData.current);
        Debug.Log("Salvo");
    }

    public void OnLoad()
    {
        Debug.Log(Application.persistentDataPath);
        SaveData.current = (SaveData)SerializationManager.Load(Application.persistentDataPath + "/saves/Entrada.save");

        // Crie uma nova malha e uma lista para armazenar os vértices
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        List<Vector3> vertices = new List<Vector3>();

        for(int i = 0; i < SaveData.current.spheres.Count; i++){
            SphereData currentSphere = SaveData.current.spheres[i];

           // float x = currentSphere.position.x;
           // float z = currentSphere.position.z;

           // if ((x > -120) && (x < 37) && (z > -211) && (z < -156))
           vertices.Add(currentSphere.position); // Adicione a posição ao invés de instanciar um GameObject
        }

        // Aplique os vértices à malha e defina o material
        mesh.SetVertices(vertices);
        mesh.SetIndices(System.Linq.Enumerable.Range(0, vertices.Count).ToArray(), MeshTopology.Points, 0);
        mesh.RecalculateBounds();

        // Crie um GameObject para manter a malha
        GameObject meshHolder = new GameObject("MeshHolder");
        meshHolder.transform.SetParent(transform);
        MeshFilter meshFilter = meshHolder.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = meshHolder.AddComponent<MeshRenderer>();
        meshRenderer.material = PointMaterial;

        Debug.Log($"Cena carregada - Entrada - {vertices.Count} pontos");


        //Cena 2

        SaveData.current = (SaveData)SerializationManager.Load(Application.persistentDataPath + "/saves/Quarto1.save");

        // Crie uma nova malha e uma lista para armazenar os vértices
        Mesh mesh2 = new Mesh();
        mesh2.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        List<Vector3> vertices2 = new List<Vector3>();

        for(int i = 0; i < SaveData.current.spheres.Count; i++){
            SphereData currentSphere = SaveData.current.spheres[i];

           // float x = currentSphere.position.x;
           // float z = currentSphere.position.z;

           // if ((x > -120) && (x < 37) && (z > -211) && (z < -156))
           vertices2.Add(currentSphere.position + new Vector3(1500f, 0f, 0f)); // Adicione a posição ao invés de instanciar um GameObject
        }

        // Aplique os vértices à malha e defina o material
        mesh2.SetVertices(vertices2);
        mesh2.SetIndices(System.Linq.Enumerable.Range(0, vertices2.Count).ToArray(), MeshTopology.Points, 0);
        mesh2.RecalculateBounds();

        // Crie um GameObject para manter a malha
        GameObject meshHolder2 = new GameObject("MeshHolder");
        meshHolder2.transform.SetParent(transform);
        MeshFilter meshFilter2 = meshHolder2.AddComponent<MeshFilter>();
        meshFilter2.mesh = mesh2;

        MeshRenderer meshRenderer2 = meshHolder2.AddComponent<MeshRenderer>();
        meshRenderer2.material = PointMaterial;

        Debug.Log($"Cena carregada - Quarto1 - {vertices2.Count} pontos");



        //Cena 3

        SaveData.current = (SaveData)SerializationManager.Load(Application.persistentDataPath + "/saves/Piano.save");

        // Crie uma nova malha e uma lista para armazenar os vértices
        Mesh mesh3 = new Mesh();
        mesh3.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        List<Vector3> vertices3 = new List<Vector3>();

        for(int i = 0; i < SaveData.current.spheres.Count; i++){
            SphereData currentSphere = SaveData.current.spheres[i];

           // float x = currentSphere.position.x;
           // float z = currentSphere.position.z;

           // if ((x > -120) && (x < 37) && (z > -211) && (z < -156))
           vertices3.Add(currentSphere.position + new Vector3(-1500f, 0f, 0f)); // Adicione a posição ao invés de instanciar um GameObject
        }

        // Aplique os vértices à malha e defina o material
        mesh3.SetVertices(vertices3);
        mesh3.SetIndices(System.Linq.Enumerable.Range(0, vertices3.Count).ToArray(), MeshTopology.Points, 0);
        mesh3.RecalculateBounds();

        // Crie um GameObject para manter a malha
        GameObject meshHolder3 = new GameObject("MeshHolder");
        meshHolder3.transform.SetParent(transform);
        MeshFilter meshFilter3 = meshHolder3.AddComponent<MeshFilter>();
        meshFilter3.mesh = mesh3;

        MeshRenderer meshRenderer3 = meshHolder3.AddComponent<MeshRenderer>();
        meshRenderer3.material = PointMaterial;

        Debug.Log($"Cena carregada - Piano - {vertices3.Count} pontos");



        //Cena 4

        SaveData.current = (SaveData)SerializationManager.Load(Application.persistentDataPath + "/saves/Suite.save");

        // Crie uma nova malha e uma lista para armazenar os vértices
        Mesh mesh4 = new Mesh();
        mesh4.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        List<Vector3> vertices4 = new List<Vector3>();

        for(int i = 0; i < SaveData.current.spheres.Count; i++){
            SphereData currentSphere = SaveData.current.spheres[i];

           // float x = currentSphere.position.x;
           // float z = currentSphere.position.z;

           // if ((x > -120) && (x < 37) && (z > -211) && (z < -156))
           vertices4.Add(currentSphere.position + new Vector3(0f, 0f, 1500f)); // Adicione a posição ao invés de instanciar um GameObject
        }

        // Aplique os vértices à malha e defina o material
        mesh4.SetVertices(vertices4);
        mesh4.SetIndices(System.Linq.Enumerable.Range(0, vertices4.Count).ToArray(), MeshTopology.Points, 0);
        mesh4.RecalculateBounds();

        // Crie um GameObject para manter a malha
        GameObject meshHolder4 = new GameObject("MeshHolder");
        meshHolder4.transform.SetParent(transform);
        MeshFilter meshFilter4 = meshHolder4.AddComponent<MeshFilter>();
        meshFilter4.mesh = mesh4;

        MeshRenderer meshRenderer4 = meshHolder4.AddComponent<MeshRenderer>();
        meshRenderer4.material = PointMaterial;

        Debug.Log($"Cena carregada - Suite - {vertices4.Count} pontos");


        //Cena 5

        SaveData.current = (SaveData)SerializationManager.Load(Application.persistentDataPath + "/saves/Sala.save");

        // Crie uma nova malha e uma lista para armazenar os vértices
        Mesh mesh5 = new Mesh();
        mesh5.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        List<Vector3> vertices5 = new List<Vector3>();

        for(int i = 0; i < SaveData.current.spheres.Count; i++){
            SphereData currentSphere = SaveData.current.spheres[i];

           // float x = currentSphere.position.x;
           // float z = currentSphere.position.z;

           // if ((x > -120) && (x < 37) && (z > -211) && (z < -156))
           vertices5.Add(currentSphere.position + new Vector3(0f, 0f, -1500f)); // Adicione a posição ao invés de instanciar um GameObject
        }

        // Aplique os vértices à malha e defina o material
        mesh5.SetVertices(vertices5);
        mesh5.SetIndices(System.Linq.Enumerable.Range(0, vertices5.Count).ToArray(), MeshTopology.Points, 0);
        mesh5.RecalculateBounds();

        // Crie um GameObject para manter a malha
        GameObject meshHolder5 = new GameObject("MeshHolder");
        meshHolder5.transform.SetParent(transform);
        MeshFilter meshFilter5 = meshHolder5.AddComponent<MeshFilter>();
        meshFilter5.mesh = mesh5;

        MeshRenderer meshRenderer5 = meshHolder5.AddComponent<MeshRenderer>();
        meshRenderer5.material = PointMaterial;

        Debug.Log($"Cena carregada - Sala - {vertices5.Count} pontos");

    }
}

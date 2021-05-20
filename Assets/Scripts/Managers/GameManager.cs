using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlyAgent.Navigation;

public class GameManager : MonoBehaviour
{

    public static GameManager gameManager;

    [Header("Threat")]
    public GameObject ThreatPrefab;
    public float SpawnFrequency;
    public int SpawnCount;

    MapGenerator mapGen;

    List<ManageObject> manageObjects;

    private void Awake()
    {
        mapGen = GetComponent<MapGenerator>();
        StartCoroutine(ThreatSpawner());
    }

    IEnumerator ThreatSpawner() 
    {
        GameObject ThreatRoot = Instantiate(new GameObject("Threat Root"));
        yield return new WaitForFixedUpdate();
        while (true) 
        {
            Vector3 spawnPos = GetRandomPointInSpace();
            GameObject lead = Instantiate(ThreatPrefab, spawnPos, new Quaternion(), ThreatRoot.transform);
            lead.GetComponent<Threat>().rad = mapGen.CubeSize * mapGen.GridSize / 2f;
            lead.GetComponent<Threat>().StartThreat();
            for (int x = 0; x < SpawnCount; x++) 
            {
                GameObject go = Instantiate(ThreatPrefab, spawnPos + (Random.insideUnitSphere * 50f), new Quaternion(), ThreatRoot.transform);
                go.GetComponent<Threat>().rad = mapGen.CubeSize * mapGen.GridSize / 2f;
                go.GetComponent<Threat>().StartThreat(lead.GetComponent<Threat>());
            }
            yield return new WaitForSeconds(SpawnFrequency);
        }
    }

    public Vector3 GetRandomPointInSpace() 
    {
        return Random.insideUnitSphere * mapGen.CubeSize * mapGen.GridSize / 2f;
    }

    public bool AddObject(ManageObject manageObject)
    {
        if (manageObjects.Contains(manageObject))
        {
            return false;
        }
        manageObjects.Add(manageObject);
        return true;
    }

    public bool RemoveObject(ManageObject manageObject)
    {
        if (!manageObjects.Contains(manageObject))
        {
            return false;
        }
        manageObjects.Remove(manageObject);
        return true;
    }

    public List<ManageObject> GetObjects(ManageObject.ObjectType type)
    {
        return manageObjects.FindAll(
            delegate (ManageObject manageObject)
            {
                return manageObject.type == type;
            });
    }



}

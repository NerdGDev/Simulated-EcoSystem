using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlyAgent.Navigation;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    public GameObject BakerObject;

    List<ManageObject> manageObjects;    

    Dictionary<MapBaker, Vector3> OctreeSection = new Dictionary<MapBaker, Vector3>(); 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BuildMapBakers() 
    {
        foreach (var item in OctreeSection) 
        {
            Destroy(item.Key.gameObject);
        }
        OctreeSection.Clear();

        float octreeScale = 500f;
        for (int x = -2; x < 3; x++) 
        {
            for (int y = -2; y < 3; y++)
            {
                for (int z = -2; z < 3; z++)
                {
                    GameObject go = Instantiate(BakerObject, new Vector3((float)x * octreeScale, (float)y * octreeScale, (float)z * octreeScale), new Quaternion(),transform);
                    go.GetComponent<MapBaker>().BakeStatic();
                }
            }
        }
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

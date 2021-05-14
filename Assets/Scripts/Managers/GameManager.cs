using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    List<ManageObject> manageObjects;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUIHandle : MonoBehaviour
{
    public TMP_Text InfoData;
    public TMP_Text QueueData;

    public GameObject SnapCanvas;

    Visualise visual;

    private void Awake()
    {

    }

    public void SetVisual(Visualise v) 
    {
        visual = v;
    }

    private void Update()
    {
        if (visual != null) 
        {
            SnapCanvas.transform.position = visual.transform.position;
            SnapCanvas.transform.rotation = Camera.main.transform.rotation;
            SetData();
        }
        else 
        {
            InfoData.text = "No Target";
            QueueData.text = "";
        }
    }

    public void SetData() 
    {
        Dictionary<string, string> dataFields;
        Dictionary<string, string> shortData;
        visual.GetDataFields(out dataFields, out shortData);
        InfoData.text = FormatListData(dataFields);
        QueueData.text = FormatListData(shortData);
        
    }

    string FormatListData(Dictionary<string, string> dataDict) 
    {
        string data = "";
        foreach (var item in dataDict) 
        {
            data += item.Key;
            data += " : ";
            data += item.Value;
            data += "\n";
        }
        return data;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUIHandle : MonoBehaviour
{
    public TMP_Text InfoData;
    public TMP_Text QueueData;
    public LineRenderer drawLine;

    public GameObject SnapCanvas;

    Visualise visual;

    PlayerExplorer explorer;

    private void Awake()
    {
        explorer = FindObjectOfType<PlayerExplorer>();
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
            drawLine.SetPosition(0, explorer.transform.position);
            drawLine.SetPosition(1, visual.transform.position);
            SetData();
        }
        else 
        {
            InfoData.text = "No Target";
            QueueData.text = "";
        }
        transform.position = SnapCanvas.transform.position;
    }

    public void SetData() 
    {
        Dictionary<string, string> dataFields;
        List<string> shortData;
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
    string FormatListData(List<string> dataDict)
    {
        string data = "";
        foreach (var item in dataDict)
        {
            data += item;
            data += "\n";
        }
        return data;
    }
}

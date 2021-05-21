using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualise : MonoBehaviour
{
    Dictionary<string, string> dataFields = new Dictionary<string, string>();
    Dictionary<string, string> shortData = new Dictionary<string, string>();

    public void AddDataField(string field, string content) 
    {
        if (!dataFields.ContainsKey(field))
        {
            dataFields.Add(field, content);
        }
        else
        {
            dataFields[field] = content;
        }
    }

    public void AddShortData(string field, string content) 
    {
        if (shortData.ContainsKey(field)) 
        {
            shortData.Remove(field);
        }
        StartCoroutine(ShortData(field, content));
    }

    IEnumerator ShortData(string field, string content) 
    {
        shortData.Add(field, content);
        yield return new WaitForSeconds(3f);
        if (shortData.ContainsKey(field))
        {
            shortData.Remove(field);
        }
    }


    public void GetDataFields(out Dictionary<string, string> infoData, out Dictionary<string, string> timedData) 
    {
        infoData = dataFields;
        timedData = shortData;
    }
}

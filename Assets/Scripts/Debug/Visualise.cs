using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualise : MonoBehaviour
{
    Dictionary<string, string> dataFields = new Dictionary<string, string>();
    List<string> shortData = new List<string>();

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
        string built = field + " : " + content;
        if (shortData.Contains(built)) 
        {
            shortData.Remove(built);
        }
        StartCoroutine(ShortData(built));
    }

    IEnumerator ShortData(string build) 
    {
        shortData.Add(build);
        yield return new WaitForSeconds(12f);
        if (shortData.Contains(build))
        {
            shortData.Remove(build);
        }
    }


    public void GetDataFields(out Dictionary<string, string> infoData, out List<string> timedData) 
    {
        infoData = dataFields;
        timedData = new List<string>(shortData);
        timedData.Reverse();
    }
}

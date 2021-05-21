using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Kit;

[RequireComponent(typeof(Visualise))]
public class Debuggable : MonoBehaviour
{
    private StringBuilder debugReport = new StringBuilder(1000);

    Dictionary<string, string> dataFields = new Dictionary<string, string>();
    List<string> shortData = new List<string>();

    Visualise visualise;

    private void Awake()
    {
        visualise = GetComponent<Visualise>();        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        visualise.GetDataFields(out dataFields, out shortData);
    }

    private void OnDrawGizmosSelected()
    {
        debugReport.Remove(0, debugReport.Length);
        foreach (var item in dataFields) 
        {
            debugReport.AppendLine(item.Key + " : " + item.Value);
        }
        debugReport.AppendLine("\n--Recent Events--");
        foreach (var item in shortData)
        {
            debugReport.AppendLine(item);
        }
        GizmosExtend.DrawLabel(transform.position, debugReport.ToString());
    }
}

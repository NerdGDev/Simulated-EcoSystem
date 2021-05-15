using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resourcing;

public class HomeManager : MonoBehaviour
{
    [Header("Home HangarBays")]
    [SerializeField]
    public GameObject[] HangarBay;

    [Header("Attention Ranges")]
    public float ExtractorRange;
    public float StorageRange;
    public float HomeRange;

    [Header("Home Base Costs")]
    public float ResourceConsumptionRate;

    [Header("Civilian Ships")]
    public GameObject[] CivilianPrefabs;
    public int CivilianMax;
    public float CivilianRange;

    [Header("Carrier Ships")]
    public GameObject[] CarrierPrefabs;
    public int CarrierMax;
    public float CarrierRange;
    private List<Carrier> Carriers;

    [Header("Miner Ships")]
    public GameObject[] MinerPrefabs;
    public int MinerMax;
    public float MiningRange;

    [Header("Fighter Ships")]
    public GameObject[] FighterPrefabs;
    public int FighterMax;
    public float FighterRange;

    #region Mission Assignment Dictionaries
    private Dictionary<Container, List<Carrier>> ContainerHandlers = new Dictionary<Container, List<Carrier>>();
    #endregion

    private void Awake()
    {
        FindContainers();

        StartCoroutine(Run());        
    }

    private void FindContainers() 
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, StorageRange);
        foreach (Collider col in hitColliders) 
        {
            Container current = col.transform.GetComponent<Container>();

            Debug.Log("Has Hit");
            Debug.Log(current);

            if (current != null) 
            {
                if (!ContainerHandlers.ContainsKey(current)) 
                {
                    Debug.Log("Add Hit");
                    ContainerHandlers.Add(current, new List<Carrier>());
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Run() 
    {
        while (true) 
        {
            UpdateCarrierMissions();
            yield return new WaitForSeconds(15f);
        }
    }

    public void AskForNewOrder(UnitBase unitBase) 
    {
        
    }

    void UpdateCarrierMissions() 
    {
        Debug.Log("Update Carrier");
        foreach (var item in ContainerHandlers) 
        {
            Debug.Log("Checking Container Assignement");
            if (item.Value.Count >= 0 && item.Value.Count < 3) 
            {
                Debug.Log("Launching Carrier");
                LaunchNewCarrier(item.Key);
            }
        }
    }

    GameObject LaunchNewCarrier(Container target) 
    {
        GameObject hangar = HangarBay[Random.Range(0, HangarBay.Length)];
        Debug.Log("Launching New Carrier");
        GameObject go = Instantiate(
            CarrierPrefabs[Random.Range(0,CarrierPrefabs.Length)],
            hangar.transform.position,
            hangar.transform.rotation);
        go.GetComponent<UnitBase>().home = this;
        ContainerHandlers[target].Add(go.GetComponent<Carrier>());
        StartCoroutine(LaunchCarrierMission(go, target));

        return go;
    }

    IEnumerator LaunchCarrierMission(GameObject go, Container target) 
    {
        Debug.Log("Sending Carrier Mission");
        yield return new WaitForSeconds(1f);
        go.GetComponent<Carrier>().DeliverResource(
            target.GetComponent<Resource>(),
            GetComponent<Resource>());
    }

    public GameObject GetHangar() 
    {
        return HangarBay[Random.Range(0, HangarBay.Length)];
    }

    public void Dock(UnitBase unit) 
    {
        if (unit.type == ManageObject.ObjectType.CARRIER) 
        {
            foreach (var item in ContainerHandlers)
            {
                if (item.Value.Contains(unit.GetComponent<Carrier>())) 
                {
                    item.Value.Remove(unit.GetComponent<Carrier>());
                }
            }
        }
        
    }
}

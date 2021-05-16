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
    private Dictionary<HomeManager, List<Civilian>> HomeManagerHandlers = new Dictionary<HomeManager, List<Civilian>>();
    #endregion

    public delegate void LaunchDelegate();
    protected Queue<LaunchDelegate> LaunchQueue = new Queue<LaunchDelegate>();

    private void Awake()
    {
        FindContainers();
        FindHomemanagers();

        StartCoroutine(HangarLaunch());

        StartCoroutine(CivilianUpdater());
        StartCoroutine(CarrierUpdater());
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

    private void FindHomemanagers()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, HomeRange);
        foreach (Collider col in hitColliders)
        {
            HomeManager current = col.transform.GetComponent<HomeManager>();

            if (current != null && current != this)
            {
                if (!HomeManagerHandlers.ContainsKey(current))
                {
                    Debug.LogError("Add HomeManager");
                    HomeManagerHandlers.Add(current, new List<Civilian>());
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

    IEnumerator HangarLaunch()
    {
        while (true)
        {
            if (LaunchQueue.Count > 0)
            {
                LaunchQueue.Dequeue().Invoke();
            }

            yield return new WaitForSeconds(1f);
        }

    }

    IEnumerator CivilianUpdater()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            UpdateCivilianMission();
            yield return new WaitForSeconds(4f);
        }
    }

    IEnumerator CarrierUpdater()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            UpdateCarrierMissions();
            yield return new WaitForSeconds(15f);
        }
    }

    public void AskForNewOrder(UnitBase unitBase)
    {

    }

    void UpdateCivilianMission()
    {
        foreach (var item in HomeManagerHandlers)
        {
            LaunchQueue.Enqueue(() =>
            {
                LaunchNewCivilian(item.Key);
            });
        }
        

    }

    void LaunchNewCivilian(HomeManager destination) 
    {
        GameObject hangar = HangarBay[Random.Range(0, HangarBay.Length)];
        Debug.Log("Launching New Civilian");
        GameObject go = Instantiate(
            CivilianPrefabs[Random.Range(0, CarrierPrefabs.Length)],
            hangar.transform.position,
            hangar.transform.rotation);
        go.GetComponent<UnitBase>().home = this;
        StartCoroutine(LaunchCivilianMission(go, destination));
    }

    IEnumerator LaunchCivilianMission(GameObject go, HomeManager destination) 
    {
        Debug.Log("Sending Carrier Mission");
        yield return new WaitForSeconds(1f);
        go.GetComponent<Civilian>().TravelTo(destination);
    }

    void UpdateCarrierMissions() 
    {
        Debug.Log("Update Carrier");
        foreach (var item in ContainerHandlers) 
        {
            Debug.Log("Checking Container Assignement");
            if (item.Value.Count >= 0 && item.Value.Count < 3) 
            {
                Debug.Log(item);
                Debug.Log(item.Key);
                Debug.Log(item.Value);
                Debug.Log(item.Value.Count);
                Debug.LogError(item.Key.resource);
                float pool;
                pool = item.Key.GetPool();
                if (pool - (300 * item.Value.Count) > 0) 
                {
                    Debug.Log("Sending Carrier to Queue");
                    LaunchQueue.Enqueue(() =>
                    {
                        LaunchNewCarrier(item.Key);
                    });
                }
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

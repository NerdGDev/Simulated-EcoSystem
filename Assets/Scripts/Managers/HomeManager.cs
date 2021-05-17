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
    private int CurrentCivilians;
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
    private List<HomeManager> HomeManagerHandlers = new List<HomeManager>();
    #endregion

    public delegate void LaunchDelegate(GameObject go);
    protected Queue<LaunchDelegate> LaunchQueue = new Queue<LaunchDelegate>();

    private void Awake()
    {
        CurrentCivilians = CivilianMax;


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
                if (!HomeManagerHandlers.Contains(current))
                {
                    Debug.LogError("Add HomeManager");
                    HomeManagerHandlers.Add(current);
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
            foreach(GameObject hangar in HangarBay) 
            {
                if (LaunchQueue.Count > 0)
                {
                    LaunchQueue.Dequeue().Invoke(hangar);
                }
            }
            

            yield return new WaitForSeconds(5f);
        }

    }

    IEnumerator CivilianUpdater()
    {
        yield return new WaitForSeconds(20f);
        while (true)
        {
            UpdateCivilianMission();
            yield return new WaitForSeconds(Random.Range(5f, 10f));
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
            if (!(CurrentCivilians <= 0)) 
            {
                LaunchQueue.Enqueue((hangar) =>
                {
                    CurrentCivilians--;
                    LaunchNewCivilian(hangar, item);
                });
            }
            
        }
        

    }

    void LaunchNewCivilian(GameObject hangar, HomeManager destination) 
    {
        Debug.Log("Launching New Civilian");

        GameObject go = Instantiate(
            CivilianPrefabs[Random.Range(0, CivilianPrefabs.Length)],
            hangar.transform.position,
            Quaternion.LookRotation(hangar.transform.localPosition));

        go.GetComponent<UnitBase>().home = this;

        //go.GetComponent<Rigidbody>().AddForce(hangar.transform.localPosition.normalized * 300f);

        StartCoroutine(LaunchCivilianMission(go, destination));
    }

    IEnumerator LaunchCivilianMission(GameObject go, HomeManager destination) 
    {
        Debug.Log("Sending Carrier Mission");
        yield return new WaitForFixedUpdate();
        go.GetComponent<Civilian>().TravelTo(destination);
    }

    void UpdateCarrierMissions() 
    {
        Debug.Log("Update Carrier");
        foreach (var item in ContainerHandlers) 
        {
            Debug.Log("Checking Container Assignement");
            if (item.Value.Count >= 0 && item.Value.Count < CarrierMax) 
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
                    LaunchQueue.Enqueue((hangar) =>
                    {
                        LaunchNewCarrier(hangar, item.Key);
                    });
                }
            }
        }
    }

    GameObject LaunchNewCarrier(GameObject hangar, Container target) 
    {
        Debug.Log("Launching New Carrier");

        GameObject go = Instantiate(
            CarrierPrefabs[Random.Range(0,CarrierPrefabs.Length)],
            hangar.transform.position,
            hangar.transform.rotation);

        go.GetComponent<UnitBase>().home = this;

        ContainerHandlers[target].Add(go.GetComponent<Carrier>());

        //go.GetComponent<Rigidbody>().AddForce(hangar.transform.localPosition.normalized * 300f);

        StartCoroutine(LaunchCarrierMission(go, target));

        return go;
    }

    IEnumerator LaunchCarrierMission(GameObject go, Container target) 
    {
        Debug.Log("Sending Carrier Mission");
        yield return new WaitForFixedUpdate();
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
        else if (unit.type == ManageObject.ObjectType.CIVILIAN) 
        {
            CurrentCivilians++;
        }
        
    }
}

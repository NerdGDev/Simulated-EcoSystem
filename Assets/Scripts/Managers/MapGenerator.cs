using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using FlyAgent.Navigation;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Baker")]

    [SerializeField]
    public GameObject BakerRoot;

    [SerializeField]
    public GameObject BakerObject;

    public float CubeSize;
    public int GridSize;

    List<MapBaker> OctreeSection = new List<MapBaker>();

    [Header("Structure")]
    public GameObject StructureRoot;
    List<GameObject> StructureObjects = new List<GameObject>();

    public float StructureScale;

    public GameObject HomeBase;
    public int HomeBaseCount;

    public GameObject Extractor;
    public int ExtractorCount;

    public GameObject Container;
    public int ContainerCount;

    [Header("Environment")]
    public GameObject EnvironmentRoot;
    List<GameObject> EnvironmentObjects = new List<GameObject>();

    public GameObject[] RockAssets;
    [Range(0,1)]
    public float Density;

    public GameObject MineralNode;
    [Range(0, 1)]
    public float MineralDensity;

#if UNITY_EDITOR
    bool EditorDebug;
    Vector3 DebugColliderPos = new Vector3();
    float DebugSize = 0;
#endif

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BuildStructures() 
    {
        ClearStructure();
        EditorCoroutineUtility.StartCoroutine(BuildStructuresRoutine(), this);
    }

    public void ClearStructure() 
    {
        foreach (GameObject item in StructureObjects)
        {
            if (item != null)
            {
                DestroyImmediate(item.gameObject);
            }
        }

        StructureObjects.Clear();
    }

    IEnumerator BuildStructuresRoutine() 
    {
        StructureRoot.transform.localScale = new Vector3(StructureScale, StructureScale, StructureScale);
        EditorDebug = true;
        for (int x = 0; x < HomeBaseCount; x++) 
        {
            bool validSpot = false;
            Vector3 pos = new Vector3();
            while (!validSpot) 
            {
                validSpot = true;
                pos = Random.insideUnitSphere * CubeSize * GridSize / 2;
                Collider[] cols = Physics.OverlapSphere(pos, HomeBase.GetComponent<CapsuleCollider>().radius);

                DebugColliderPos = pos;
                DebugSize = HomeBase.GetComponent<CapsuleCollider>().radius * 20;

                yield return new EditorWaitForSeconds(0.01f);
                foreach (Collider col in cols) 
                {
                    validSpot = false;
                }
                yield return new EditorWaitForSeconds(0.01f);
            }

            GameObject go = Instantiate(HomeBase, pos, Quaternion.LookRotation(Random.insideUnitSphere,Vector3.up), StructureRoot.transform);
            StructureObjects.Add(go);
            
        }

        for (int x = 0; x < ExtractorCount; x++)
        {
            bool validSpot = false;
            Vector3 pos = new Vector3();
            while (!validSpot)
            {
                validSpot = true;
                pos = Random.insideUnitSphere * CubeSize * GridSize / 2;
                Collider[] cols = Physics.OverlapSphere(pos, HomeBase.GetComponent<CapsuleCollider>().radius);

                DebugColliderPos = pos;
                DebugSize = Extractor.GetComponent<CapsuleCollider>().radius;

                yield return new EditorWaitForSeconds(0.01f);
                foreach (Collider col in cols)
                {
                    validSpot = false;
                }
                yield return new EditorWaitForSeconds(0.01f);
            }

            GameObject go = Instantiate(Extractor, pos, Quaternion.LookRotation(Random.insideUnitSphere, Vector3.up), StructureRoot.transform);
            StructureObjects.Add(go);

        }

        for (int x = 0; x < ContainerCount; x++)
        {
            bool validSpot = false;
            Vector3 pos = new Vector3();
            while (!validSpot)
            {
                validSpot = true;
                pos = Random.insideUnitSphere * CubeSize * GridSize / 2;
                Collider[] cols = Physics.OverlapSphere(pos, HomeBase.GetComponent<CapsuleCollider>().radius);

                DebugColliderPos = pos;
                DebugSize = Container.GetComponent<CapsuleCollider>().radius;

                yield return new EditorWaitForSeconds(0.01f);
                foreach (Collider col in cols)
                {
                    validSpot = false;
                }
                yield return new EditorWaitForSeconds(0.01f);
            }

            GameObject go = Instantiate(Container, pos, Quaternion.LookRotation(Random.insideUnitSphere, Vector3.up), StructureRoot.transform);
            StructureObjects.Add(go);

        }

        yield return new EditorWaitForSeconds(0.01f);
        EditorDebug = false;
    }

    public void BuildEnvironment() 
    {
    
    }

    public void ClearEnvironment() 
    {
    
    }

    IEnumerator BuildEnvironmentRoutine()
    {        
        EditorDebug = true;
        for (int x = 0; x < HomeBaseCount; x++)
        {
            bool validSpot = false;
            Vector3 pos = new Vector3();
            while (!validSpot)
            {
                validSpot = true;
                pos = Random.insideUnitSphere * CubeSize * GridSize / 2;
                Collider[] cols = Physics.OverlapSphere(pos, HomeBase.GetComponent<CapsuleCollider>().radius);

                DebugColliderPos = pos;
                DebugSize = HomeBase.GetComponent<CapsuleCollider>().radius * 20;

                yield return new EditorWaitForSeconds(0.01f);
                foreach (Collider col in cols)
                {
                    validSpot = false;
                }
                yield return new EditorWaitForSeconds(0.01f);
            }

            GameObject go = Instantiate(HomeBase, pos, Quaternion.LookRotation(Random.insideUnitSphere, Vector3.up), StructureRoot.transform);
            StructureObjects.Add(go);

        }

        for (int x = 0; x < ExtractorCount; x++)
        {
            bool validSpot = false;
            Vector3 pos = new Vector3();
            while (!validSpot)
            {
                validSpot = true;
                pos = Random.insideUnitSphere * CubeSize * GridSize / 2;
                Collider[] cols = Physics.OverlapSphere(pos, HomeBase.GetComponent<CapsuleCollider>().radius);

                DebugColliderPos = pos;
                DebugSize = Extractor.GetComponent<CapsuleCollider>().radius;

                yield return new EditorWaitForSeconds(0.01f);
                foreach (Collider col in cols)
                {
                    validSpot = false;
                }
                yield return new EditorWaitForSeconds(0.01f);
            }

            GameObject go = Instantiate(Extractor, pos, Quaternion.LookRotation(Random.insideUnitSphere, Vector3.up), StructureRoot.transform);
            StructureObjects.Add(go);

        }

        yield return new EditorWaitForSeconds(0.01f);
        EditorDebug = false;
    }

    public void BuildMapBakers()
    {
        ClearMapBakers();
        EditorCoroutineUtility.StartCoroutine(BuildMapBakersRoutine(), this);
    }

    public void ClearMapBakers() 
    {
        foreach (MapBaker item in OctreeSection)
        {
            if (item != null)
            {
                DestroyImmediate(item.gameObject);
            }
        }

        OctreeSection.Clear();
    }

    IEnumerator BuildMapBakersRoutine() 
    {
        int lower = (int)Mathf.Ceil(-GridSize / 2f);
        int heigher = (int)Mathf.Ceil(GridSize / 2f);
        for (int x = lower; x < heigher; x++)
        {
            for (int y = lower; y < heigher; y++)
            {
                for (int z = lower; z < heigher; z++)
                {
                    GameObject go = Instantiate(BakerObject, new Vector3((float)x * CubeSize, (float)y * CubeSize, (float)z * CubeSize), new Quaternion(), BakerRoot.transform);
                    OctreeSection.Add(go.GetComponent<MapBaker>());
                    go.GetComponent<MapBaker>().m_MinWorldSize = CubeSize;
                    go.GetComponent<MapBaker>().BakeStatic();
                    yield return new EditorWaitForSeconds(0.01f);
                }
            }
        }      

    }

    private void OnDrawGizmos()
    {
        if (EditorDebug) 
        {
            Gizmos.DrawWireSphere(DebugColliderPos, DebugSize);
        }
    }


}

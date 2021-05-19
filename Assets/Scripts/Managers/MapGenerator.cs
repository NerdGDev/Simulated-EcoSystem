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
    float rockRad = 0.45f;
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
        if (StructureRoot == null) 
        {
            StructureRoot = Instantiate(new GameObject("Structure Root"));
        }
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

        foreach (Transform item in StructureRoot.GetComponentsInChildren<Transform>())
        {
            if (item != null && item != StructureRoot.transform)
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
        int FailMax = 40;
        int fail = 0;

        for (int x = 0; x < HomeBaseCount; x++)
        {
            fail = 0;
            bool validSpot = false;
            Vector3 pos = new Vector3();
            while (!validSpot)
            {
                if (fail >= FailMax)
                {
                    break;
                }
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
            fail = 0;
            bool validSpot = false;
            Vector3 pos = new Vector3();
            while (!validSpot)
            {
                if (fail >= FailMax)
                {
                    break;
                }
                validSpot = true;
                pos = Random.insideUnitSphere * CubeSize * GridSize / 2;
                Collider[] cols = Physics.OverlapSphere(pos, Extractor.GetComponent<CapsuleCollider>().radius);

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
            fail = 0;
            bool validSpot = false;
            Vector3 pos = new Vector3();
            while (!validSpot)
            {
                if (fail >= FailMax)
                {
                    break;
                }
                validSpot = true;
                pos = Random.insideUnitSphere * CubeSize * GridSize / 2;
                Collider[] cols = Physics.OverlapSphere(pos, Container.GetComponent<CapsuleCollider>().radius);

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
        if (EnvironmentRoot == null)
        {
            EnvironmentRoot = Instantiate(new GameObject("Environment Root"));
        }
        ClearEnvironment();
        EditorCoroutineUtility.StartCoroutine(BuildEnvironmentRoutine(), this);
    }

    public void ClearEnvironment() 
    {
        foreach (GameObject item in EnvironmentObjects)
        {
            if (item != null)
            {
                DestroyImmediate(item.gameObject);
            }
        }

        foreach (Transform item in EnvironmentRoot.GetComponentsInChildren<Transform>())
        {
            if (item != null && item != EnvironmentRoot.transform)
            {
                DestroyImmediate(item.gameObject);
            }
        }

        EnvironmentObjects.Clear();
    }

    IEnumerator BuildEnvironmentRoutine()
    {        
        EditorDebug = true;
        int FailMax = 40;
        int fail = 0;
        float densityVal = (Density * CubeSize * GridSize / 2) * rockRad;
        float randScale = 0f;
        for (int x = 0; x < densityVal; x++)
        {
            fail = 0;
            bool validSpot = false;
            Vector3 pos = new Vector3();
            while (!validSpot)
            {
                if (fail >= FailMax) 
                {
                    break;
                }
                randScale = Random.Range(30f, 100f);
                validSpot = true;
                pos = Random.insideUnitSphere * CubeSize * GridSize / 2;
                Collider[] cols = Physics.OverlapSphere(pos, rockRad * randScale);

                DebugColliderPos = pos;
                DebugSize = randScale;

                yield return new EditorWaitForSeconds(0.002f);
                foreach (Collider col in cols)
                {
                    validSpot = false;
                }
                yield return new EditorWaitForSeconds(0.002f);
            }

            GameObject go = Instantiate(RockAssets[Random.Range(0, RockAssets.Length)], pos, Quaternion.LookRotation(Random.insideUnitSphere, Vector3.up), EnvironmentRoot.transform);
            go.AddComponent<SphereCollider>().radius = rockRad;
            go.transform.localScale = new Vector3(randScale, randScale, randScale);
            go.isStatic = true;
            EnvironmentObjects.Add(go);

        }

        float mineralDensityVal = (Density * CubeSize * GridSize / 2) / 10;
        for (int x = 0; x < mineralDensityVal; x++)
        {
            fail = 0;
            bool validSpot = false;
            Vector3 pos = new Vector3();
            while (!validSpot)
            {
                if (fail >= FailMax)
                {
                    break;
                }
                validSpot = true;
                pos = Random.insideUnitSphere * CubeSize * GridSize / 2;
                Collider[] cols = Physics.OverlapSphere(pos, MineralNode.GetComponent<SphereCollider>().radius * 5f);

                DebugColliderPos = pos;
                DebugSize = MineralNode.GetComponent<SphereCollider>().radius * 5f;

                yield return new EditorWaitForSeconds(0.01f);
                foreach (Collider col in cols)
                {
                    validSpot = false;
                }
                yield return new EditorWaitForSeconds(0.01f);
            }

            GameObject go = Instantiate(MineralNode, pos, Quaternion.LookRotation(Random.insideUnitSphere, Vector3.up), EnvironmentRoot.transform);
            EnvironmentObjects.Add(go);
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

        foreach (Transform item in BakerRoot.GetComponentsInChildren<Transform>())
        {
            if (item != null && item != BakerRoot.transform)
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

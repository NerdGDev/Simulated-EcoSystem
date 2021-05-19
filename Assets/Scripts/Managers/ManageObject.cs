using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageObject : MonoBehaviour
{
    [Header("Game Configuration")]
    public float UpdateFrequency = 1f;

    // All Units Types Needed Here
    public enum ObjectType
    {
        BASE,
        CIVILIAN,
        FIGHTER,
        CARRIER,
        MINER,
        THREAT
    }
    public ObjectType type{ get; protected set; }

    GameManager gameManager;


    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Civilian : UnitBase
{

    void Awake()
    {
        base.Awake();
        type = ObjectType.CARRIER;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TravelTo(HomeManager home) 
    {
        AddOrder((go) => { go.StartCoroutine(Goto(home.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(Land(home)); });
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TreasureHunterInventory : MonoBehaviour
{
    [Serializable]
    public class CollecibleAmountMap : SerializableDictionary<CollectibleTreasure, int> {}
    public CollecibleAmountMap numberOfEachThingICollected;
}

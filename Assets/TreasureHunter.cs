using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System;
using System.Linq;

public class TreasureHunter : MonoBehaviour
{
    public TreasureHunterInventory inventory;
    public OVRCameraRig oVRCameraRig;
    public OVRManager oVRManager;
    public OVRHeadsetEmulator oVRHeadsetEmulator;
    public Camera viewpointCamera;
    public TextMesh scoreText;
    int currentTotalScore;

    // Start is called before the first frame update
    void Start(){
        oVRCameraRig = this.gameObject.GetComponent<OVRCameraRig>();
        oVRManager = this.gameObject.GetComponent<OVRManager>();
        oVRHeadsetEmulator = this.gameObject.GetComponent<OVRHeadsetEmulator>();
        viewpointCamera = this.gameObject.GetComponent<Camera>();
        inventory = this.gameObject.GetComponent<TreasureHunterInventory>();
    }
    // Update is called once per frame
    void Update(){
       int score = calculateScore();
       if(Input.GetKeyDown("space")){
           Debug.Log("Space");
            // got from nick code
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 100.0f)){
                name = hit.transform.GetComponent<CollectibleTreasure>().prefab;
                CollectibleTreasure prefab = AssetDatabase.LoadAssetAtPath("Assets/" + name + ".prefab", typeof(CollectibleTreasure)) as CollectibleTreasure;
                if(!inventory.numberOfEachThingICollected.ContainsKey(prefab)) inventory.numberOfEachThingICollected.Add(prefab, 1);
                else inventory.numberOfEachThingICollected[prefab] += 1;
                Destroy(hit.transform.gameObject);
            }
       }
       
       if(Input.GetKeyDown("4")){
            Debug.Log("4");
            int count = inventory.numberOfEachThingICollected.Sum(amountCollected => amountCollected.Value);
            scoreText.text = "Meris Beganovic\n" + 
                             "Score: " + score + "\n" + 
                             "Count: " + count;
        }
    }
    int calculateScore(){
        int totalScore = 0;
        foreach(CollectibleTreasure treasure in inventory.numberOfEachThingICollected.Keys) {
                totalScore += inventory.numberOfEachThingICollected[treasure] * treasure.treasureValue;
        }
        return totalScore;
    }
}

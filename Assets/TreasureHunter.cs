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
    public Camera viewpointCamera;
    public TextMesh scoreText;
    int currentTotalScore;
    int audioCollected = 0;

    // Start is called before the first frame update
    void Start(){
        oVRCameraRig = this.gameObject.GetComponent<OVRCameraRig>();
        viewpointCamera = this.gameObject.GetComponent<Camera>();
        inventory = this.gameObject.GetComponent<TreasureHunterInventory>();
    }
    // Update is called once per frame
    void Update(){
       int score = calculateScore();
       if(Input.GetKeyDown("c")){
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 1)){
                audioCollected++;
                name = hit.transform.GetComponent<CollectibleTreasure>().prefab;
                // CollectibleTreasure prefab = AssetDatabase.LoadAssetAtPath("Assets/" + name + ".prefab", typeof(CollectibleTreasure)) as CollectibleTreasure;
                // if(!inventory.numberOfEachThingICollected.ContainsKey(prefab)) inventory.numberOfEachThingICollected.Add(prefab, 1);
                // else inventory.numberOfEachThingICollected[prefab] += 1;
                scoreText.text = audioCollected + "";
                Destroy(hit.transform.gameObject);
            }
       }
       
    //    if(Input.GetKeyDown("4")){
    //         Debug.Log("4");
    //         int count = inventory.numberOfEachThingICollected.Sum(amountCollected => amountCollected.Value);
    //         scoreText.text = "Meris Beganovic\n" + 
    //                          "Score: " + score + "\n" + 
    //                          "Count: " + count;
    //     }
    }
    int calculateScore(){
        int totalScore = 0;
        foreach(CollectibleTreasure treasure in inventory.numberOfEachThingICollected.Keys) {
                totalScore += inventory.numberOfEachThingICollected[treasure] * treasure.treasureValue;
        }
        return totalScore;
    }
}
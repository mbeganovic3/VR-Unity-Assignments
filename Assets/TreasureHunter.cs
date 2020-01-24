using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureHunter : MonoBehaviour
{
    public List<CollectibleTreasure> collectiblesInScene; 
    public TreasureHunterInventory inventory;
    public OVRCameraRig oVRCameraRig;
    public OVRManager oVRManager;
    public OVRHeadsetEmulator oVRHeadsetEmulator;
    public Camera viewpointCamera;
    public TextMesh wonText;
    public TextMesh scoreText;
    // public PostProcessLayer postProcessLayer;
    // public LocomotionHandler locomotionHandler;

    int currentTotalScore;
    // Start is called before the first frame update
    void Start(){
        oVRCameraRig = this.gameObject.GetComponent<OVRCameraRig>();
        oVRManager = this.gameObject.GetComponent<OVRManager>();
        oVRHeadsetEmulator = this.gameObject.GetComponent<OVRHeadsetEmulator>();
        viewpointCamera = this.gameObject.GetComponent<Camera>();
        inventory = this.gameObject.GetComponent<TreasureHunterInventory>();
        // postProcessLayer = this.gameObject.GetComponent<PostProcessLayer>();
        // locomotionHandler = this.gameObject.GetComponent<LocomotionHandler>();
    }
    // Update is called once per frame
    void Update(){
        int score = calculateScore();
        CollectibleTreasure coin = collectiblesInScene[0];
        CollectibleTreasure treasureChest = collectiblesInScene[1];
        CollectibleTreasure diamond = collectiblesInScene[2];

        if(Input.GetKeyDown("1")){
            Debug.Log("1");
            if(!inventory.inventoryItems.Contains(coin)) inventory.inventoryItems.Add(coin);
        }else if(Input.GetKeyDown("2")){
            Debug.Log("2");
            if(!inventory.inventoryItems.Contains(treasureChest)) inventory.inventoryItems.Add(treasureChest);
        }else if(Input.GetKeyDown("3")){
            Debug.Log("3");
            if(!inventory.inventoryItems.Contains(diamond)) inventory.inventoryItems.Add(diamond);
        }else if(Input.GetKeyDown("4")){
            Debug.Log("4");
            int count = inventory.inventoryItems.Count;
            scoreText.text = "Score: " + score + "\n" + 
                             "Count: " + count;
        }else if(score == 60){
            wonText.text = "You Win!";
        }
    }

    int calculateScore(){
        int totalScore = 0;
        foreach(CollectibleTreasure treasure in inventory.inventoryItems) totalScore += treasure.treasureValue;
        currentTotalScore = totalScore;
        return totalScore;
    }
}

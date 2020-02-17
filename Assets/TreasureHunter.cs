using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public enum AttachmentRule { KeepRelative, KeepWorld, SnapToTarget }

public class TreasureHunter : MonoBehaviour {
    public TreasureHunterInventory inventory;
    public OVRCameraRig oVRCameraRig;
    public TextMesh scoreText;
    public TextMesh myName;
    public Camera camera;

    int currentTotalScore;

    // camera.transform.location.y

    /////////////////////////////////////////////// NICK CODE
    public GameObject leftPointerObject;
    public GameObject rightPointerObject;
    public LayerMask collectiblesMask;
    GameObject thingOnGun;
    CollectibleTreasure thingIGrabbed;

    public TextMesh otherText;
    Vector3 previousPointerPos; //using this for velocity since Unity's broken physics engine won't give it to me otherwise
    ///////////////////////////////////////////////////////

    void Start () {
        //camera = this.gameObject.GetComponent<Camera> ();
        oVRCameraRig = this.gameObject.GetComponent<OVRCameraRig> ();
        inventory = this.gameObject.GetComponent<TreasureHunterInventory> ();
        myName.text = "Meris Beganovic";
    }

    void Update () {
        int score = calculateScore ();
        int count = inventory.numberOfEachThingICollected.Sum (amountCollected => amountCollected.Value);
        scoreText.text = "Item | Value | Count\n";
        foreach (CollectibleTreasure treasure in inventory.numberOfEachThingICollected.Keys) {
            scoreText.text += treasure + " | " + treasure.treasureValue + " | " + inventory.numberOfEachThingICollected[treasure] + "\n";
        }
        scoreText.text += "Total Score = " + score + "\n" + "Total Items = " + count;

        if (Input.GetKeyDown ("1")) {
            // got from nick code
            RaycastHit hit;
            if (Physics.Raycast (transform.position, transform.forward, out hit, 100.0f)) {
                name = hit.transform.GetComponent<CollectibleTreasure> ().prefab;
                // You’ll need to change LoadAssetAtPath to Resources.Load to get it to build
                CollectibleTreasure prefab = Resources.Load (name + ".prefab", typeof (CollectibleTreasure)) as CollectibleTreasure;
                if (!inventory.numberOfEachThingICollected.ContainsKey (prefab)) inventory.numberOfEachThingICollected.Add (prefab, 1);
                else inventory.numberOfEachThingICollected[prefab] += 1;
                Destroy (hit.transform.gameObject);
            }
        }
        //equivalent to GrabRight in UE4 version (right grip)    
        if (OVRInput.GetDown (OVRInput.RawButton.RHandTrigger)) {
            // outputText3.text = "Grip";
            //In Unity, I can't directly get the overlapping actors of a component. I need to query it manually with Physics.OverlapSphere or OnTriggerEnter
            //I overlap with 1 cm radius to try to get only things near hand
            //this will also return collider for the hand mesh if there is one. I disabled it but keep it in mind. You need to make sure hand is on a different layer
            //collectiblesMask is defined at the top right of the Inspector where it says Layer. The layer controls which things to hit (there is no "class filter" like in UE4)
            Collider[] overlappingThings = Physics.OverlapSphere (rightPointerObject.transform.position, 0.01f, collectiblesMask);
            if (overlappingThings.Length > 0) {
                attachGameObjectToAChildGameObject (overlappingThings[0].gameObject, rightPointerObject, AttachmentRule.KeepWorld, AttachmentRule.KeepWorld, AttachmentRule.KeepWorld, true);
                //I'm not bothering to check for nullity because layer mask should ensure I only collect collectibles.
                thingIGrabbed = overlappingThings[0].gameObject.GetComponent<CollectibleTreasure> ();
            }
        } else if (OVRInput.GetUp (OVRInput.RawButton.RHandTrigger) || OVRInput.GetUp (OVRInput.RawButton.A) || OVRInput.GetUp (OVRInput.RawButton.B) || OVRInput.GetUp (OVRInput.RawButton.RThumbstick)) {

            letGo ();

            //since you can't merge paths the way I did in BP, I need to create a function that does the force grab thing or else I would duplicate code
            //equivalent to ShootAndGrabNoSnap in UE4 version (A)
        } else if (OVRInput.GetDown (OVRInput.RawButton.A)) {
            // outputText3.text = "Force Grab at Distance";
            forceGrab (true);

            //equivalent to ShootAndGrabSnap in UE4 version (B)
        } else if (OVRInput.GetDown (OVRInput.RawButton.B)) {
            // outputText3.text = "Force Grab Snap";
            forceGrab (false);

            //equivalent to MagneticGrip in UE4 version (RS/R3)
        } else if (OVRInput.GetDown (OVRInput.RawButton.RThumbstick)) {
            // outputText3.text = "Magnetic Grip";
            Collider[] overlappingThings = Physics.OverlapSphere (rightPointerObject.transform.position, 1, collectiblesMask);
            if (overlappingThings.Length > 0) {
                CollectibleTreasure nearestCollectible = getClosestHitObject (overlappingThings);
                attachGameObjectToAChildGameObject (nearestCollectible.gameObject, rightPointerObject, AttachmentRule.SnapToTarget, AttachmentRule.SnapToTarget, AttachmentRule.KeepWorld, true);
                //I'm not bothering to check for nullity because layer mask should ensure I only collect collectibles.
                thingIGrabbed = nearestCollectible.gameObject.GetComponent<CollectibleTreasure> ();
            }
        }
        previousPointerPos = rightPointerObject.gameObject.transform.position;
    }
    int calculateScore () {
        int totalScore = 0;
        foreach (CollectibleTreasure treasure in inventory.numberOfEachThingICollected.Keys) {
            totalScore += inventory.numberOfEachThingICollected[treasure] * treasure.treasureValue;
        }
        return totalScore;
    }

    CollectibleTreasure getClosestHitObject (Collider[] hits) {
        float closestDistance = 10000.0f;
        CollectibleTreasure closestObjectSoFar = null;
        foreach (Collider hit in hits) {
            CollectibleTreasure c = hit.gameObject.GetComponent<CollectibleTreasure> ();
            if (c) {
                float distanceBetweenHandAndObject = (c.gameObject.transform.position - rightPointerObject.gameObject.transform.position).magnitude;
                if (distanceBetweenHandAndObject < closestDistance) {
                    closestDistance = distanceBetweenHandAndObject;
                    closestObjectSoFar = c;
                }
            }
        }
        return closestObjectSoFar;
    }
    //could have more easily just passed in attachment rule.... but I wanted to keep the code similar to the BP example
    void forceGrab (bool pressedA) {
        RaycastHit outHit;
        //notice I'm using the layer mask again
        if (Physics.Raycast (rightPointerObject.transform.position, rightPointerObject.transform.up, out outHit, 100.0f, collectiblesMask)) {
            AttachmentRule howToAttach = pressedA ? AttachmentRule.KeepWorld : AttachmentRule.SnapToTarget;
            attachGameObjectToAChildGameObject (outHit.collider.gameObject, rightPointerObject.gameObject, howToAttach, howToAttach, AttachmentRule.KeepWorld, true);
            thingIGrabbed = outHit.collider.gameObject.GetComponent<CollectibleTreasure> ();
            myName.text = outHit.collider.gameObject.name;
        }
    }

    void letGo () {
        if (thingIGrabbed) {
            //  CollectibleTreasure prefab = Resources.Load (name + ".prefab", typeof (CollectibleTreasure)) as CollectibleTreasure;
            //     if (!inventory.numberOfEachThingICollected.ContainsKey (prefab)) inventory.numberOfEachThingICollected.Add (prefab, 1);
            //     else inventory.numberOfEachThingICollected[prefab] += 1;
            //     Destroy (thingIGrabbed);

            // Collider[] overlappingThingsWithLeftHand = Physics.OverlapSphere (leftPointerObject.transform.position, 0.01f, collectiblesMask);
            // if (overlappingThingsWithLeftHand.Length > 0) {
            //     if (thingOnGun) {
            //         detachGameObject (thingOnGun, AttachmentRule.KeepWorld, AttachmentRule.KeepWorld, AttachmentRule.KeepWorld);
            //         simulatePhysics (thingOnGun, Vector3.zero, true);
            //     }
            //     attachGameObjectToAChildGameObject (overlappingThingsWithLeftHand[0].gameObject, leftPointerObject, AttachmentRule.SnapToTarget, AttachmentRule.SnapToTarget, AttachmentRule.KeepWorld, true);
            //     thingOnGun = overlappingThingsWithLeftHand[0].gameObject;
            //     thingIGrabbed = null;
            if (rightPointerObject) {

            } else {

            }

            if (camera) { } else { }
            if (rightPointerObject.gameObject.transform.position.y < camera.transform.position.y - 0.2 && rightPointerObject.gameObject.transform.position.y > camera.transform.position.y - 0.6 &&
                rightPointerObject.gameObject.transform.position.x < camera.transform.position.x + 0.2 && rightPointerObject.gameObject.transform.position.x > camera.transform.position.x - 0.6 &&
                rightPointerObject.gameObject.transform.position.z < camera.transform.position.z + 0.2 && rightPointerObject.gameObject.transform.position.z > camera.transform.position.z - 0.6) {
                //   Destroy(rightPointerObject.gameObject);
                name = thingIGrabbed.prefab;
                CollectibleTreasure prefab = Resources.Load (name, typeof (CollectibleTreasure)) as CollectibleTreasure;
                if (!inventory.numberOfEachThingICollected.ContainsKey (prefab)) inventory.numberOfEachThingICollected.Add (prefab, 1);
                else inventory.numberOfEachThingICollected[prefab] += 1;
                Destroy (thingIGrabbed.gameObject);
                thingIGrabbed = null;
            } else {
                detachGameObject (thingIGrabbed.gameObject, AttachmentRule.KeepWorld, AttachmentRule.KeepWorld, AttachmentRule.KeepWorld);
                simulatePhysics (thingIGrabbed.gameObject, (rightPointerObject.gameObject.transform.position - previousPointerPos) / Time.deltaTime, true);
                thingIGrabbed = null;
            }
        } else { }
    }

    //since Unity doesn't have sceneComponents like UE4, we can only attach GOs to other GOs which are children of another GO
    //e.g. attach collectible to controller GO, which is a child of VRRoot GO
    //imagine if scenecomponents in UE4 were all split into distinct GOs in Unity
    public void attachGameObjectToAChildGameObject (GameObject GOToAttach, GameObject newParent, AttachmentRule locationRule, AttachmentRule rotationRule, AttachmentRule scaleRule, bool weld) {
        GOToAttach.transform.parent = newParent.transform;
        handleAttachmentRules (GOToAttach, locationRule, rotationRule, scaleRule);
        if (weld) {
            simulatePhysics (GOToAttach, Vector3.zero, false);
        }
    }

    public static void detachGameObject (GameObject GOToDetach, AttachmentRule locationRule, AttachmentRule rotationRule, AttachmentRule scaleRule) {
        //making the parent null sets its parent to the world origin (meaning relative & global transforms become the same)
        GOToDetach.transform.parent = null;
        handleAttachmentRules (GOToDetach, locationRule, rotationRule, scaleRule);
    }

    public static void handleAttachmentRules (GameObject GOToHandle, AttachmentRule locationRule, AttachmentRule rotationRule, AttachmentRule scaleRule) {
        GOToHandle.transform.localPosition =
            (locationRule == AttachmentRule.KeepRelative) ? GOToHandle.transform.position :
            //technically don't need to change anything but I wanted to compress into ternary
            (locationRule == AttachmentRule.KeepWorld) ? GOToHandle.transform.localPosition :
            new Vector3 (0, 0, 0);

        //localRotation in Unity is actually a Quaternion, so we need to specifically ask for Euler angles
        GOToHandle.transform.localEulerAngles =
            (rotationRule == AttachmentRule.KeepRelative) ? GOToHandle.transform.eulerAngles :
            //technically don't need to change anything but I wanted to compress into ternary
            (rotationRule == AttachmentRule.KeepWorld) ? GOToHandle.transform.localEulerAngles :
            new Vector3 (0, 0, 0);

        GOToHandle.transform.localScale =
            (scaleRule == AttachmentRule.KeepRelative) ? GOToHandle.transform.lossyScale :
            //technically don't need to change anything but I wanted to compress into ternary
            (scaleRule == AttachmentRule.KeepWorld) ? GOToHandle.transform.localScale :
            new Vector3 (1, 1, 1);
    }

    public void simulatePhysics (GameObject target, Vector3 oldParentVelocity, bool simulate) {
        Rigidbody rb = target.GetComponent<Rigidbody> ();
        if (rb) {
            if (!simulate) {
                Destroy (rb);
            }
        } else {
            if (simulate) {
                //there's actually a problem here relative to the UE4 version since Unity doesn't have this simple "simulate physics" option
                //The object will NOT preserve momentum when you throw it like in UE4.
                //need to set its velocity itself.... even if you switch the kinematic/gravity settings around instead of deleting/adding rb
                Rigidbody newRB = target.AddComponent<Rigidbody> ();
                newRB.velocity = oldParentVelocity;
            }
        }
    }

}
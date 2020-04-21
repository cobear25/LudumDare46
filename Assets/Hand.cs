using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public bool hasStar = false;
    GameController gameController;
    // Start is called before the first frame update
    void Start()
    {
        gameController = (GameController)FindObjectOfType(typeof(GameController));
    }

    // Update is called once per frame
    void Update()
    {
    }
        
    void OnCollisionEnter2D(Collision2D col)
    {
        // Don't do anything if hand already has a star or the mouse is holding the hand down
        if (hasStar || !gameController.armLoose) { return; }
        // Grab the star upon collision
        if (col.gameObject.tag == "Star") {
            RelativeJoint2D newRelativeJoint = gameObject.AddComponent<RelativeJoint2D>();
            DistanceJoint2D newDistanceJoint2D = gameObject.AddComponent<DistanceJoint2D>();
            newDistanceJoint2D.distance = 0f;
            // newDistanceJoint2D.maxDistanceOnly = true;
            newRelativeJoint.connectedBody = col.rigidbody;
            newDistanceJoint2D.connectedBody = col.rigidbody;
            col.rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            hasStar = true;
            col.transform.parent = transform.parent;
            gameController.StarGrabbed(col.gameObject);
            col.gameObject.GetComponent<Star>().HasBeenGrabbed();
            col.rigidbody.velocity = Vector2.zero;
            GetComponent<AudioSource>().Play();
            col.transform.position = transform.position;
        } 
    }

    public void LetGoOfStar() {
        Destroy(GetComponent<RelativeJoint2D>());
        Destroy(GetComponent<DistanceJoint2D>());
        hasStar = false;
    }
}

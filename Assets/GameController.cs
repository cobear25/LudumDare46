using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public PlayerController player;
    public Transform moveToIndicator;
    public Transform hand;
    public Transform shoulder;
    public Color armColor;

    public float planetRadius = 10;
    public bool armOut = false;
    public bool armLoose = false;

    LineRenderer armLine;
    // Start is called before the first frame update
    void Start()
    {
        armLine = hand.gameObject.AddComponent<LineRenderer>();
        armLine.material = new Material(Shader.Find("Sprites/Default"));
        armLine.widthMultiplier = 0.1f;
        armLine.sortingOrder = hand.GetComponent<SpriteRenderer>().sortingOrder;
        armLine.positionCount = 2;
        armLine.startColor = armColor;
        armLine.endColor = armColor;
        armLine.numCapVertices = 10;
        armLine.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // hand.position = mousePos;
            float dx = mousePos.x - hand.position.x;
            float dy = mousePos.y - hand.position.y;
            hand.GetComponent<Rigidbody2D>().MovePosition(mousePos);
            hand.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            hand.GetComponent<SpringJoint2D>().dampingRatio = 0;
            hand.GetComponent<SpringJoint2D>().frequency = 1;
            hand.GetComponent<SpriteRenderer>().enabled = true;
            armLine.enabled = true;
            armOut = true;
            armLoose = false;
            player.ArmOut();
        }
        if (Input.GetMouseButtonUp(0)) {
            // release arm
            Invoke("ReleaseArm", 0.1f);
            Invoke("SlowDownArm", 1.0f);
        }
        if (Input.GetMouseButtonDown(1)) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float angle = Mathf.Atan2(mousePos.y, mousePos.x);
            player.GoToAngle(angle);
            moveToIndicator.position = new Vector2(Mathf.Cos(angle) * planetRadius, Mathf.Sin(angle) * planetRadius);
            moveToIndicator.GetComponent<Animator>().Play("MoveToIndicator");
        }
        if (armOut) {
            armLine.SetPositions(new Vector3[] {hand.position, shoulder.position} );
        }
        if (armLoose) {
            if (hand.GetComponent<Rigidbody2D>().velocity.magnitude <= 0.01f) {
                HideArm();
            }
        }
    }

    private void ReleaseArm() {
        armLoose = true;
    }

    private void SlowDownArm() {
        if (!armLoose) { return; }
        hand.GetComponent<SpringJoint2D>().dampingRatio = 0.8f;
        hand.GetComponent<SpringJoint2D>().frequency = 1.5f;
        Invoke("FullSlowdownArm", 0.5f);
    }

    private void FullSlowdownArm() {
        if (!armLoose) { return; }
        hand.GetComponent<SpringJoint2D>().frequency = 0f;
    }

    public void HideArm() {
        hand.GetComponent<SpriteRenderer>().enabled = false;
        armLine.enabled = false;
        armOut = false;
        armLoose = false;
        player.ArmAway();
    }

    public void StopIndicator() {
        moveToIndicator.GetComponent<Animator>().Play("Default");
    }

    void Awake () {
        // StartCoroutine (Routine());
    }

    private void OnDrawGizmos() {
        UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, planetRadius);
    }
}

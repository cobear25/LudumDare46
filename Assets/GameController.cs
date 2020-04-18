﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public PlayerController player;
    public Transform moveToIndicator;
    public Transform hand;
    public Transform shoulder;
    public Color armColor;
    public Transform leftEye;
    public Transform rightEye;
    public SpriteRenderer mouth;
    public GameObject plantPlaceholder;
    public GameObject starPrefab;
    public GameObject meteoritePrefab;
    public GameObject crack1Prefab;
    public GameObject crack2Prefab;
    public GameObject crack3Prefab;
    public GameObject plantPrefab;
    public Sprite[] mouthSprites;
    public Transform currentStar;

    public float planetRadius = 10;
    public bool armOut = false;
    public bool armLoose = false;
    public int planetStatus = 0;
    public bool hasStar = false;
    public bool aboveCrack = false;
    bool canCatch = true;

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
        plantPlaceholder.GetComponentInChildren<Animator>().Play("PlantEmpty");

        AddStar();
        Invoke("AddMeteorite", 5);
    }

    // Update is called once per frame
    void Update()
    {
        // stretch arm to catch
        if (canCatch && Input.GetMouseButton(0)) {
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
            hand.GetComponent<CircleCollider2D>().enabled = false;
            player.ArmOut();
            if (currentStar == null) {
                hand.GetComponent<Hand>().LetGoOfStar();
            }
        }
        // release arm
        if (Input.GetMouseButtonUp(0)) {
            Invoke("ReleaseArm", 0.1f);
            Invoke("SlowDownArm", 1.0f);
        }
        // right click to move to point
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

        if (hasStar && !aboveCrack) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float angle = Mathf.Atan2(mousePos.y, mousePos.x);
            Vector2 position = new Vector2(Mathf.Cos(angle) * planetRadius, Mathf.Sin(angle) * planetRadius);
            plantPlaceholder.transform.position = position;
            plantPlaceholder.transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * angle);
            plantPlaceholder.GetComponentInChildren<Animator>().Play("Idle");
            plantPlaceholder.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);

            // plant a star
            if (Input.GetMouseButtonDown(0))
            {
                PlantStar(position, angle, currentStar.gameObject);
            }
        } else {
            plantPlaceholder.GetComponentInChildren<Animator>().Play("PlantEmpty");
            Debug.Log("disable placeholder");
        }

        // camera zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel") * 5f;
        Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Camera.main.orthographicSize + scroll > 5 && Camera.main.orthographicSize + scroll < 10) {
            Camera.main.orthographicSize += scroll;
        } 
    }

    private void ReleaseArm() {
        armLoose = true;
        hand.GetComponent<CircleCollider2D>().enabled = true;
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

    private void PlantStar(Vector2 position, float angle, GameObject star) {
        GameObject plant = Instantiate(plantPrefab, position, Quaternion.identity);
        plant.transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * angle);
        currentStar.GetComponent<Star>().GoToTransform(plant.transform);
        hand.GetComponent<Hand>().LetGoOfStar();
        planetStatus++;
        hasStar = false;
        Invoke("EnableCatching", 1);
        Invoke("UpdatePlanetStatus", 7.5f);
    }

    public void HealCrack(GameObject crack) {
        currentStar.GetComponent<Star>().GoToTransform(crack.transform);
        hand.GetComponent<Hand>().LetGoOfStar();
        hasStar = false;
        Invoke("EnableCatching", 1);
    }

    private void EnableCatching() {
        canCatch = true;
    }

    void Awake () {
        // StartCoroutine (Routine());
    }

    private void OnDrawGizmos() {
        UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, planetRadius);
    }

    void AddStar() {
        float angle = Random.Range(0.0f, Mathf.PI * 2f);
        GameObject star = Instantiate(starPrefab, new Vector2(-16, 5), Quaternion.identity);
        star.transform.position = new Vector2(Mathf.Cos(angle) * 16, Mathf.Sin(angle) * 16);
        // go relatively towards the center
        float xDirection = star.transform.position.x < 0 ? 1 : -1;
        float yDirection = star.transform.position.y < 0 ? 1 : -1;
        star.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(0.5f, 1.5f) * xDirection, Random.Range(0.5f, 1.5f) * yDirection);
        Invoke("AddStar", Random.Range(5, 15));
    }

    void AddMeteorite() {
        float angle = Random.Range(0.0f, Mathf.PI * 2f);
        GameObject meteorite = Instantiate(meteoritePrefab, new Vector2(0, 16), Quaternion.identity);
        meteorite.transform.position = new Vector2(Mathf.Cos(angle) * 16, Mathf.Sin(angle) * 16);
        meteorite.GetComponent<Rigidbody2D>().velocity = new Vector2(-meteorite.transform.position.x / 4, -meteorite.transform.position.y / 4);
        Invoke("AddMeteorite", Random.Range(5, 15));
    }

    public void AddCrack(Vector2 position, Vector2 normal) {
        int rand = Random.Range(0, 3);
        switch (rand) {
            case 0:
                Instantiate(crack1Prefab, position, Quaternion.FromToRotation(Vector2.up, normal));
                break;
            case 1:
                Instantiate(crack2Prefab, position, Quaternion.FromToRotation(Vector2.up, normal));
                break;
            default:
                Instantiate(crack3Prefab, position, Quaternion.FromToRotation(Vector2.up, normal));
                break;
        }
        planetStatus--;
        UpdatePlanetStatus();
    }

    public void CrackHealed() {
        planetStatus++;
        UpdatePlanetStatus();
    }

    public void StarGrabbed(GameObject star) {
        hasStar = true;
        canCatch = false;
        currentStar = star.transform;
    }

    void UpdatePlanetStatus() {
        switch (planetStatus) {
            case 4:
                mouth.sprite = mouthSprites[0];
                break;
            case 3:
                mouth.sprite = mouthSprites[1];
                break;
            case 2:
                mouth.sprite = mouthSprites[2];
                break;
            case 1:
                mouth.sprite = mouthSprites[3];
                break;
            case 0:
                mouth.sprite = mouthSprites[4];
                break;
            case -1:
                mouth.sprite = mouthSprites[5];
                break;
            case -2:
                mouth.sprite = mouthSprites[6];
                break;
            case -3:
                mouth.sprite = mouthSprites[7];
                break;
            case -4:
                mouth.sprite = mouthSprites[8];
                break;
            default:
                break;
        }
    }
}

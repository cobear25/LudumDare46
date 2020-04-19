using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public PlayerController player;
    public SpriteRenderer planet;
    public Transform moveToIndicator;
    public Transform hand;
    public Transform shoulder;
    public Color armColor;
    public Transform leftEye;
    public Transform rightEye;
    public SpriteRenderer mouth;
    public SpriteRenderer leftEyeOutside;
    public SpriteRenderer rightEyeOutside;
    public GameObject plantPlaceholder;
    public GameObject starPrefab;
    public GameObject meteoritePrefab;
    public GameObject[] crackPrefabs;
    public GameObject plantPrefab;
    public Sprite[] mouthSprites;
    public Transform currentStar;
    public CinemachineVirtualCamera virtualCamera;
    public Transform backGlow;
    public GameObject gameOverPanel;
    public GameObject introPanel;
    public Text tutorialText;
    public GameObject skipTutorialButton;

    CinemachineBasicMultiChannelPerlin perlin;

    public float planetRadius = 10;
    public bool armOut = false;
    public bool armLoose = false;
    public int planetStatus = 0;
    public bool hasStar = false;
    public bool aboveCrack = false;
    public bool isGameOver = false;
    bool canCatch = false;
    bool inTutorial = false;

    public int level = 1;

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
        plantPlaceholder.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);

        for (int i = 0; i < 4; i++) {
            float angle = Random.Range(0.0f, Mathf.PI * 2f);
            if (i == 0) {
                GameObject crack = Instantiate(crackPrefabs[i]);
                crack.transform.position = new Vector2(Mathf.Cos(angle) * planetRadius, Mathf.Sin(angle) * planetRadius);
                crack.transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * angle - 90);
            } else {
                GameObject crack = Instantiate(crackPrefabs[i - 1]);
                crack.transform.position = new Vector2(Mathf.Cos(angle) * planetRadius, Mathf.Sin(angle) * planetRadius);
                crack.transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * angle - 90);
            }
            planetStatus++;
            UpdatePlanetStatus();
        }

        virtualCamera.enabled = false;
        gameOverPanel.SetActive(false);
        skipTutorialButton.SetActive(false);
        AddStar();
    }

    // Update is called once per frame
    void Update()
    {
        // camera zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel") * 5f;
        Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Camera.main.orthographicSize + scroll > 5 && Camera.main.orthographicSize + scroll < 10) {
            Camera.main.orthographicSize += scroll;
        } 

        if (isGameOver) {
            Color color = planet.color;
            planet.color = new Color(color.r, color.g, color.b, color.a - 0.001f);
            leftEye.GetComponent<SpriteRenderer>().color = planet.color;
            rightEye.GetComponent<SpriteRenderer>().color = planet.color;
            mouth.GetComponent<SpriteRenderer>().color = planet.color;
            leftEyeOutside.color = planet.color;
            rightEyeOutside.color = planet.color;
            player.GetComponentInChildren<SpriteRenderer>().color = planet.color;
            backGlow.GetComponent<SpriteRenderer>().color = planet.color;
            if (planet.color.a <= 0) {
                planet.GetComponent<CircleCollider2D>().enabled = false;
            }
            return;
        }

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
            if (inTutorial && tutorialStep == 1 && !hasCalledTutorial2)
            {
                hasCalledTutorial2 = true;
                Invoke("NextTutorialStep", 2);
            }
        }
        // right click to move to point
        if (Input.GetMouseButtonDown(1)) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float angle = Mathf.Atan2(mousePos.y, mousePos.x);
            player.GoToAngle(angle);
            moveToIndicator.position = new Vector2(Mathf.Cos(angle) * planetRadius, Mathf.Sin(angle) * planetRadius);
            moveToIndicator.GetComponent<Animator>().Play("MoveToIndicator");
            // for tutorial enable grabbing
            if (!hasStar && inTutorial) {
                canCatch = true;
                if (tutorialStep == 0) {
                    Invoke("NextTutorialStep", 2);
                }
            }
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

            // plant a star
            if (Input.GetMouseButtonDown(0))
            {
                PlantStar(position, angle, currentStar.gameObject);
            }
        } else {
            plantPlaceholder.GetComponentInChildren<Animator>().Play("PlantEmpty");
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
        hasStar = false;
        Invoke("EnableCatching", 1);
        if (inTutorial) {
            if (!meteoritesHaveBegun) {
                Invoke("AddMeteorite", 2);
            }
            Invoke("NextTutorialStep", 2);
        }
    }

    public void HealCrack(GameObject crack) {
        currentStar.GetComponent<Star>().GoToTransform(crack.transform);
        hand.GetComponent<Hand>().LetGoOfStar();
        hasStar = false;
        Invoke("EnableCatching", 1);
        if (inTutorial) {
            if (!meteoritesHaveBegun) {
                Invoke("AddMeteorite", 2);
            }
            Invoke("NextTutorialStep", 2);
        }
    }

    private void EnableCatching() {
        canCatch = true;
    }
    private bool hasCalledTutorial2 = false;
    private int tutorialStep = 0;
    private void NextTutorialStep() {
        tutorialStep += 1;
        switch (tutorialStep) {
            case 1:
                tutorialText.text = "Left-click to pull back your arm and launch it at a shooting star.";
                break;
            case 2:
                tutorialText.text = "Try your best to grab a star, you'll need it.";
                break;
            case 3:
                tutorialText.text = "Left-click on a crack to heal it, or anywhere else to plant a tree";
                break;
            case 4:
                tutorialText.text = "Plant trees to protect the planet, and fill all the cracks to heal it. Heal all the cracks to go on to the next level.";
                Invoke("EndTutorial", 15);
                break;
            default:
                break;
        }
    }

    public void EndTutorial() {
        tutorialText.text = "";
        inTutorial = false;
        if (!meteoritesHaveBegun) {
            AddMeteorite();
        }
        if (!hasStar) {
            canCatch = true;
        }
        skipTutorialButton.SetActive(false);
    }

    void Awake () {
        // StartCoroutine (Routine());
    }

    private void OnDrawGizmos() {
        #if UNITY_EDITOR
        UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, planetRadius);
        #endif
    }

    void AddStar() {
        float angle = Random.Range(0.0f, Mathf.PI * 2f);
        GameObject star = Instantiate(starPrefab, new Vector2(-16, 5), Quaternion.identity);
        star.transform.position = new Vector2(Mathf.Cos(angle) * 18, Mathf.Sin(angle) * 18);
        // go relatively towards the center
        float xDirection = star.transform.position.x < 0 ? 1 : -1;
        float yDirection = star.transform.position.y < 0 ? 1 : -1;
        star.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(0.5f, 1.5f) * xDirection, Random.Range(0.5f, 1.5f) * yDirection);
        int newStarFrequency = 15;
        switch (level) {
            case 1:
                newStarFrequency = Random.Range(4, 8);
                break;
            case 2:
                newStarFrequency = Random.Range(5, 10);
                break;
            case 3:
                newStarFrequency = Random.Range(5, 15);
                break;
            case 4:
                newStarFrequency = Random.Range(10, 20);
                break;
            case 5:
                newStarFrequency = Random.Range(15, 20);
                break;
            default:
                newStarFrequency = Random.Range(15, 20);
                break;
        }
        Invoke("AddStar", newStarFrequency);
    }

    bool meteoritesHaveBegun = false;
    void AddMeteorite() {
        meteoritesHaveBegun = true;
        float angle = Random.Range(0.0f, Mathf.PI * 2f);
        GameObject meteorite = Instantiate(meteoritePrefab, new Vector2(0, 16), Quaternion.identity);
        meteorite.transform.position = new Vector2(Mathf.Cos(angle) * 18, Mathf.Sin(angle) * 18);
        meteorite.GetComponent<Rigidbody2D>().velocity = new Vector2(-meteorite.transform.position.x / 4, -meteorite.transform.position.y / 4);
        int newMeteoriteFrequency = 15;
        switch (level) {
            case 1:
                newMeteoriteFrequency = Random.Range(15, 20);
                break;
            case 2:
                newMeteoriteFrequency = Random.Range(10, 20);
                break;
            case 3:
                newMeteoriteFrequency = Random.Range(10, 15);
                break;
            case 4:
                newMeteoriteFrequency = Random.Range(5, 15);
                break;
            case 5:
                newMeteoriteFrequency = Random.Range(5, 10);
                break;
            default:
                newMeteoriteFrequency = Random.Range(5, 10);
                break;
        }
        Invoke("AddMeteorite", newMeteoriteFrequency);
        if (inTutorial) {
            // end tutorial and start game
            inTutorial = false;
        }
    }

    public void AddCrack(Vector2 position, Vector2 normal) {
        virtualCamera.enabled = true;
        Invoke("EndShake", 0.15f);
        int rand = Random.Range(0, 3);
        Instantiate(crackPrefabs[rand], position, Quaternion.FromToRotation(Vector2.up, normal));
        planetStatus++;
        UpdatePlanetStatus();
    }

    void EndShake() {
        virtualCamera.enabled = false;
    }

    public void CrackHealed() {
        planetStatus--;
        UpdatePlanetStatus();
    }

    public void StarGrabbed(GameObject star) {
        hasStar = true;
        canCatch = false;
        currentStar = star.transform;
        if (inTutorial) {
            // show instructions for planting or healing
            if (tutorialStep == 2) {
                tutorialStep = 2;
                Invoke("NextTutorialStep", 2);
            }
        }
    }

    void UpdatePlanetStatus() {
        switch (planetStatus) {
            case 0:
                mouth.sprite = mouthSprites[0];
                backGlow.localScale = new Vector2(0.9f, 0.9f);
                break;
            case 1:
                mouth.sprite = mouthSprites[1];
                backGlow.localScale = new Vector2(0.89f, 0.89f);
                break;
            case 2:
                mouth.sprite = mouthSprites[2];
                backGlow.localScale = new Vector2(0.88f, 0.88f);
                break;
            case 3:
                mouth.sprite = mouthSprites[3];
                backGlow.localScale = new Vector2(0.87f, 0.87f);
                break;
            case 4:
                mouth.sprite = mouthSprites[4];
                backGlow.localScale = new Vector2(0.85f, 0.85f);
                break;
            case 5:
                mouth.sprite = mouthSprites[5];
                backGlow.localScale = new Vector2(0.83f, 0.83f);
                break;
            case 6:
                mouth.sprite = mouthSprites[6];
                backGlow.localScale = new Vector2(0.81f, 0.81f);
                break;
            case 7:
                mouth.sprite = mouthSprites[7];
                backGlow.localScale = new Vector2(0.79f, 0.79f);
                break;
            case 8:
                mouth.sprite = mouthSprites[8];
                backGlow.localScale = new Vector2(0.77f, 0.77f);
                break;
            default:
                break;
        }
        if (planetStatus <= 0) {
            // Level beat!
            LevelCompleted();
        }
        if (planetStatus >= 8) {
            // game over
            GameOver();
        }
    }

    void LevelCompleted() {

    }

    public void StartGame() {
        inTutorial = true;
        introPanel.SetActive(false);
        tutorialText.text = "Right-click to move around the planet";
        skipTutorialButton.SetActive(true);
    }

    void GameOver() {
        gameOverPanel.SetActive(true);
        isGameOver = true;
    }

    public void StartNextLevel() {
        level++;
    }

    public void TryAgain() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    GameController gameController;
    Transform goTo;
    bool startFading = false;
    AudioSource audioSource;
    public AudioSource healSound;
    // Start is called before the first frame update
    void Start()
    {
        gameController = (GameController)FindObjectOfType(typeof(GameController));
        Destroy(gameObject, 100);
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (goTo != null) {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            transform.position = Vector2.MoveTowards(transform.position, goTo.position, 7 * Time.deltaTime);
        } 

        if (startFading) {
            Color color = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, color.a - 0.005f);
            if (color.a <= 0) {
                Destroy(gameObject, 3);
            }
        } 
        float distanceFromCenter = Vector2.Distance(transform.position, Vector2.zero);
        audioSource.volume = 1 / (distanceFromCenter * 10);
    }

    public void GoToTransform(Transform t) {
        if (goTo != null) return;
        transform.parent = gameController.transform;
        goTo = t;
        Invoke("StartFading", 0.5f);
    }

    public void HasBeenGrabbed() {
        GetComponent<CircleCollider2D>().enabled = false;
    }

    void StartFading() {
        startFading = true;
        healSound.Play();
    }
}

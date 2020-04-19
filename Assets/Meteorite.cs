using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteorite : MonoBehaviour
{
    bool startFading = false;
    GameController gameController;
    public AudioSource fireSound;
    // Start is called before the first frame update
    void Start()
    {
        gameController = (GameController)FindObjectOfType(typeof(GameController));
        Destroy(gameObject, 100);
    }

    // Update is called once per frame
    void Update()
    {
        if (startFading) {
            Color color = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, color.a - 0.001f);
        } 
        float distanceFromCenter = Vector2.Distance(transform.position, Vector2.zero);
        fireSound.volume = 1 / (distanceFromCenter * 4);
        if (gameController.levelCompleted) {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (startFading) { return; }
        if (col.gameObject.tag == "Planet") {
            if (gameController.planetStatus > 0) {
                gameController.AddCrack(col.contacts[0].point, col.contacts[0].normal);
            }
            GetComponentInChildren<ParticleSystem>().Stop();
            Destroy(gameObject, 5);
            startFading = true;
            GetComponent<AudioSource>().Play();
        }
        if (col.gameObject.tag == "Plant") {
            GetComponentInChildren<ParticleSystem>().Stop();
            Destroy(gameObject, 5);
            startFading = true;
        }
        fireSound.Stop();
    }
}

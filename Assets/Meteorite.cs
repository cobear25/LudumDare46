using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteorite : MonoBehaviour
{
    bool startFading = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (startFading) {
            Color color = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, color.a - 0.001f);
        } 
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (startFading) { return; }
        if (col.gameObject.tag == "Planet") {
            GameController gc = (GameController)FindObjectOfType(typeof(GameController));
            gc.AddCrack(col.contacts[0].point, col.contacts[0].normal);
            GetComponentInChildren<ParticleSystem>().Stop();
            Destroy(gameObject, 5);
            startFading = true;
        }
    }
}

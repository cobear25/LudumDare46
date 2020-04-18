using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crack : MonoBehaviour
{
    GameController gameController;
    bool healing = false;
    // Start is called before the first frame update
    void Start()
    {
        gameController = (GameController)FindObjectOfType(typeof(GameController));
    }

    // Update is called once per frame
    void Update()
    {
        if (healing) {
            Color color = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, color.a - 0.01f);
            if (color.a <= 0) {
                gameController.CrackHealed();
                Destroy(gameObject);
            }
            // don't do any mouse actions if healing.
            return;
        }
        // don't do any mouse actions if no star in hand
        if (!gameController.hasStar) { return; }
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (GetComponent<BoxCollider2D>().bounds.Contains(new Vector3(mousePos.x, mousePos.y, transform.position.z))) {
            gameController.aboveCrack = true;
            GetComponent<SpriteRenderer>().color = Color.white;
            transform.localScale = new Vector2(1.1f, 1.1f);

            if (Input.GetMouseButtonDown(0)) {
                Invoke("Heal", 0.5f);
            }
        } else {
            gameController.aboveCrack = false;
            GetComponent<SpriteRenderer>().color = Color.black;
            transform.localScale = new Vector2(1.0f, 1.0f);
        }
    }

    void Heal() {
        gameController.HealCrack(gameObject);
        healing = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crack : MonoBehaviour
{
    GameController gameController;
    bool healing = false;
    Transform healingStar;
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
            Color starColor = healingStar.GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, color.a - 0.01f);
            healingStar.GetComponent<SpriteRenderer>().color = new Color(starColor.r, starColor.g, starColor.b, starColor.a - 0.01f);
            if (color.a <= 0) {
                gameController.CrackHealed();
                Destroy(gameObject);
                gameController.hasStar = false;
                Destroy(healingStar.gameObject);
            }
            // don't do any mouse actions if healing.
            return;
        }
        // don't do any mouse actions if no star in hand
        if (!gameController.hasStar) { return; }
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (GetComponent<BoxCollider2D>().bounds.Contains(new Vector3(mousePos.x, mousePos.y, transform.position.z))) {
            GetComponent<SpriteRenderer>().color = Color.white;
            transform.localScale = new Vector2(1.1f, 1.1f);

            if (Input.GetMouseButtonDown(0)) {
                gameController.currentStar.GetComponent<Star>().crackToHeal = transform;
                healingStar = gameController.currentStar;
                gameController.hand.GetComponent<Hand>().LetGoOfStar();
                Invoke("Heal", 0.5f);
            }
        } else {
            GetComponent<SpriteRenderer>().color = Color.black;
            transform.localScale = new Vector2(1.0f, 1.0f);
        }
    }

    void Heal() {
        healing = true;
    }
}

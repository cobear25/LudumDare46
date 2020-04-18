using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameController gameController;
    public float moveSpeed = 0.1f;

    private float toAngle = 0;
    private float currentAngle = 0;
    // Start is called before the first frame update
    void Start()
    {
        currentAngle = Mathf.Atan2(transform.position.y, transform.position.x);
        toAngle = currentAngle;
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(currentAngle - toAngle) < 0.05f) {
            return;
        }
        GetComponentInChildren<Animator>().Play("Run");
        float twoPi = Mathf.PI * 2;
        while (currentAngle < 0f) {
            currentAngle += twoPi;
        }
        while (toAngle < 0) {
            toAngle += twoPi;
        }

        while (currentAngle > twoPi) {
            currentAngle -= twoPi;
        }
        while (toAngle > twoPi) {
            toAngle -= twoPi;
        }
        // increment the angle if the shortest distance is clockwise
        bool goClockwise = true;
        if (toAngle > currentAngle) {
            if (toAngle - currentAngle > Mathf.PI) {
                goClockwise = false;
            }
        }
        if (toAngle < currentAngle) {
            if (currentAngle - toAngle < Mathf.PI) {
                goClockwise = false;
            }
        }
        if (goClockwise) {
            GetComponentInChildren<SpriteRenderer>().flipX = true;
            currentAngle += (moveSpeed * Time.deltaTime);
        } else {
            GetComponentInChildren<SpriteRenderer>().flipX = false;
            currentAngle -= (moveSpeed * Time.deltaTime);
        }
        Debug.Log("current: " + currentAngle);
        Debug.Log("to: " + toAngle);
        if (Mathf.Abs(currentAngle - toAngle) < (0.05f)) {
            currentAngle = toAngle;
            gameController.StopIndicator();
            GetComponentInChildren<Animator>().Play("Idle");
        }
        transform.position = new Vector2(Mathf.Cos(currentAngle) * gameController.planetRadius, Mathf.Sin(currentAngle) * gameController.planetRadius);
        transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * currentAngle);
    }

    public void GoToAngle(float angle) {
        // CGPoint(x: controlRing.position.x + cos(launcherAngle) * nobRadius, y: controlRing.position.y + sin(launcherAngle) * nobRadius)
        // transform.position = new Vector2(Mathf.Cos(angle) * gameController.planetRadius, Mathf.Sin(angle) * gameController.planetRadius);
        toAngle = angle;
    }
}

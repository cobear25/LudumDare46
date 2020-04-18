using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    public Transform crackToHeal;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (crackToHeal != null) {
            transform.position = Vector2.MoveTowards(transform.position, crackToHeal.position, 7 * Time.deltaTime);
        } 
    }
}

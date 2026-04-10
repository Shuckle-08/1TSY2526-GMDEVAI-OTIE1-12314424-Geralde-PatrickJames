using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASDMovement : MonoBehaviour
{
    public float speed = 10;
    public float rotationSpeed = 200;
    public float currentSpeed = 0;


    // Update is called once per frame
    void LateUpdate()
    {
        float translation = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;

        transform.Translate(0, 0, translation);
        currentSpeed = translation;

        transform.Rotate(0, rotation, 0);

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicroMousse : MonoBehaviour
{

    enum enum_state
    {
        idle,
        moveFront,
        turnLeft,
        turnRight,
        turnBack
    }

    enum_state state= enum_state.idle;


    // Start is called before the first frame update
    float distFront;
    float distLeft;
    float distRight;

    float posX = 0.0f;
    float posY = 0.0f;

    void Start()
    {

    }

    void FixedUpdate()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        Vector3 left = transform.TransformDirection(Vector3.left);
        Vector3 right = transform.TransformDirection(Vector3.right);

        distFront = -1;
        distLeft = -1;
        distRight = -1;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, fwd, out hit, 10))
        {
            distFront = hit.distance;
        }
        if (Physics.Raycast(transform.position, left, out hit, 10))
        {
            distLeft = hit.distance;
        }
        if (Physics.Raycast(transform.position, right, out hit, 10))
        {
            distRight = hit.distance;
        }
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * distFront, Color.red);
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.left) * distLeft, Color.blue);
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.right) * distRight, Color.green);

    }

    // Update is called once per frame
    void Update()
    {
        if (state == enum_state.moveFront)
        {
            transform.localPosition += new Vector3( 0, 0, 1 * Time.deltaTime);
        }
        if (state == enum_state.turnLeft)
        {
            transform.Rotate(0, -1, 0);
        }
        if (state == enum_state.turnRight)
        {
            transform.Rotate(0, 1, 0);
        }
        
        ComputeNextDest();
    }

    void ComputeNextDest()
    {
        if (distFront > 0.8f)
        {
            state = enum_state.moveFront;
            return;
        }

        if (distLeft > 1.0f)
        {
            state = enum_state.turnLeft;
            return;
        }

        if (distRight > 1.0f)
        {
            state = enum_state.turnRight;
            return;
        }
    }
}

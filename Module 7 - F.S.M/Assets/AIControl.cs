using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    Idle,
    Running,
    Casting
}

public class AIControl : MonoBehaviour
{
    public Transform player;

    Animator animator;

    float rotationSpeed = 2.0f;
    float speed = 2.0f;
    float visionDist = 20.0f;
    float visionAngle = 30.0f;
    float castRange = 5.0f;

    State state;

    private void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    void LateUpdate()
    {
        Vector3 dir = player.position - this.transform.position;
        float angle = Vector3.Angle(dir, this.transform.forward);

        if (dir.magnitude < visionDist && angle < visionAngle)
        {
            dir.y = 0;
            
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                        Quaternion.LookRotation(dir),
                                        Time.deltaTime * rotationSpeed);

            if (dir.magnitude > castRange)
            {   
                if (state != State.Running)
                {
                    state = State.Running;
                    animator.SetTrigger("isRunning");
                }
            }
            else 
            {
                if (state != State.Casting)
                {
                    state = State.Casting;
                    animator.SetTrigger("isCasting");
                }
            }
        }
        else {
            if (state != State.Idle)
            {
                state = State.Idle;
                animator.SetTrigger("isIdle");
            }
        }

        if (state == State.Running)
        {
            this.transform.Translate(0, 0, Time.deltaTime * speed);
        }
    }
}

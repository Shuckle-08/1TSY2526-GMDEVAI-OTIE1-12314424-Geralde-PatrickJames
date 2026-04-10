using UnityEngine;

public class Flee : NPCBaseSFM
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        NPC.GetComponent<TankAI>().StopFiring();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector3 direction = NPC.transform.position - opponent.transform.position;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        NPC.transform.rotation = Quaternion.Slerp(
            NPC.transform.rotation,
            Quaternion.LookRotation(direction),
            rotationSpeed * Time.deltaTime);

        NPC.transform.Translate(0f, 0f, Time.deltaTime * speed);
    }
}

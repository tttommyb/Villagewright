using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupMovement : MonoBehaviour
{
    [SerializeField] private Transform gate;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BoxCollider village_trigger;
    Vector3 target;

    private void Start()
    {
        target = gate.position;
    }

    void Update()
    {
        Vector3 force = (target - this.transform.position).normalized;
        force = new Vector3(force.x, 0, force.z);
        rb.MovePosition(rb.position + (force * Time.deltaTime * 5f));

        if (village_trigger.bounds.Contains(rb.transform.position)) 
        {
            target = new Vector3(0,this.transform.position.y, 0);
        }
    }
}

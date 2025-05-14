using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GroupMovement : MonoBehaviour
{
    [SerializeField] private Transform gate;
    [SerializeField] private Transform exit;
    private Vector3 attack_target;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BoxCollider village_trigger;

    private List<GameObject> attack_points;
    Vector3 target;
    GameObject target_gameobject;

    bool reached = false;

    static int i = 0;

    private void Start()
    {
        target = gate.position;

        Scene currentScene = SceneManager.GetActiveScene();

        List<GameObject> game_objects = new List<GameObject>();
        game_objects = currentScene.GetRootGameObjects().ToList<GameObject>();

        attack_points = GameObject.FindGameObjectsWithTag("AttackPoint").ToList<GameObject>();

        attack_target = attack_points[i].transform.position;
        target_gameobject = attack_points[i];
        Debug.Log(i);
        i++;
        if(i >= attack_points.Count) 
        {
            i = 0;
        }
        
    }

    void Update()
    {
        Debug.Log(target);   
        
            Vector3 force = (target - this.transform.position).normalized;
            force = new Vector3(force.x, 0, force.z);
            rb.MovePosition(rb.position + (force * Time.deltaTime * 15f));

            if (village_trigger.bounds.Contains(rb.transform.position) && reached == false)
            {
                target = new Vector3(attack_target.x, this.transform.position.y, attack_target.z);
            }

            if ((target - this.transform.position).magnitude < 2.5f)
            {
                if(reached == true)
                {
                    target = exit.transform.position;
                    InvokeRepeating("DeleteGroup", 5f, Mathf.Infinity);
                    return;
                }
                reached = true;
                target_gameobject.transform.parent.transform.parent.transform.position = Vector3.zero;
                target = new Vector3(gate.position.x, this.transform.position.y,gate.position.z);
            }
        
       
    }

    void DeleteGroup() 
    {
        Destroy(gameObject);
    }
}

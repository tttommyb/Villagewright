using PostFX;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Progress;

public class Builder : MonoBehaviour
{
    private GameObject current_structure; //Stores the current struture that has been selected

    [SerializeField] private GameObject rotation_circle;

    [SerializeField] private GameObject grid_renderer;

    [SerializeField] private GameObject scroll_bar;

    public int selection = -1;
    public int current_structure_index = -1;

    public List<List<GameObject>> inventory = new List<List<GameObject>>();
    public List<GameObject> placed = new List<GameObject>();

    [SerializeField] private GameObject player;

    int totalStructures = 0;

    float grid_size = 5;
    float angle = 0;

    List<GameObject> objects;

    [SerializeField] GameObject raid;

    Dictionary<string, List<Vector3>> starting_positions = new Dictionary<string, List<Vector3>>();
    Dictionary<string, List<Quaternion>> starting_rotations = new Dictionary<string, List<Quaternion>>();

    // Start is called before the first frame update
    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        objects = currentScene.GetRootGameObjects().ToList<GameObject>();

        foreach (GameObject @object in objects)
        {
            totalStructures++;
            if (!starting_positions.ContainsKey(@object.tag)) 
            {
                starting_positions.Add(@object.tag, new List<Vector3>());
                starting_rotations.Add(@object.tag, new List<Quaternion>());
            }
            starting_positions[@object.tag].Add(@object.transform.position);
            starting_rotations[@object.tag].Add(@object.transform.rotation);
            if (@object.GetComponent<Data>() != null)
            {

               bool dupe = false;
               foreach(List<GameObject> items in inventory) 
                {
                    if (items[0].tag == @object.tag) 
                    {
                        items.Add(@object);
                        dupe = true;
                        break;
                    }
                }
                if (!dupe) 
                {
                    List<GameObject> list = new List<GameObject>();
                    list.Add(@object);
                    inventory.Add(list);
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        Camera.main.GetComponent<TiltShift>().Offset = -((Screen.height / 2) - Input.mousePosition.y) / (Screen.height / 2);
        Camera.main.GetComponent<TiltShift>().XOffset = -((Screen.width / 2) - Input.mousePosition.x) / (Screen.width / 2);

        if (raid.transform.childCount > 0) 
        {
            return;
        }
        scroll_bar.SetActive(true);
        grid_renderer.SetActive(true);
        //Instantiates the selected object into the scene 

        if(current_structure_index != selection) 
        {
            current_structure = inventory[selection].First();
            current_structure_index = selection;
            current_structure.transform.rotation = player.transform.rotation;
        }
        if(current_structure != null) 
        {
            rotation_circle.transform.position = current_structure.transform.position + new Vector3(0, 0.1f, 0);

            rotation_circle.transform.rotation = Quaternion.Euler(new Vector3(90, current_structure.transform.rotation.eulerAngles.y, 0));
        }

        

        


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Input.GetKey(KeyCode.Q)) 
        {
            player.transform.Rotate(new Vector3(0, 100 * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.E))
        {
            player.transform.Rotate(new Vector3(0, -100 * Time.deltaTime, 0));
        }
        //If the R key is hold then rotate the object with mouse
        rotation_circle.GetComponent<SpriteRenderer>().enabled = false;


        if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Structure")))
        {
            if (hit.point != null && current_structure == null)
            {
                if (Input.GetMouseButtonDown(0))
                {

                    Debug.Log(hit.collider.gameObject.name);
                    current_structure = hit.collider.gameObject;
                    current_structure.GetComponent<BoxCollider>().enabled = false;
                    return;
                }
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            rotation_circle.GetComponent<SpriteRenderer>().enabled = true;
            if (current_structure != null)
            {
                angle -= Input.mousePositionDelta.x;

                Vector3 euler = current_structure.transform.rotation.eulerAngles;
                euler.y = Mathf.Round(angle / 45f) * 45f;
                current_structure.transform.rotation = Quaternion.Euler(euler);
            }
        }

        

        //If the player isnt rotating, then follow the mouse
        else if (Physics.Raycast(ray, out hit, 1000f,LayerMask.GetMask("Ground")))
        {
            if (hit.point != null && current_structure != null)
            {
                    current_structure.transform.position = new Vector3(Mathf.Round(hit.point.x / grid_size) * grid_size,
                                                                       hit.point.y,
                                                                       Mathf.Round(hit.point.z / grid_size) * grid_size);
            }
        }

        if (Input.GetMouseButtonDown(0) && current_structure != null) 
        {
            current_structure.GetComponent<BoxCollider>().enabled = true;
            foreach (List<GameObject> items in inventory)
            {
                if (items[0].tag == current_structure.tag)
                {
                    placed.Add(items[0]);
                    items.RemoveAt(0);
                }
            }

            current_structure = null;
            selection = -1;
            current_structure_index = -1;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
          Debug.Log(CalculateAccuracy());
        }
    }

    float CalculateAccuracy() 
    {
        GameObject[] tempObjsArray = new GameObject[objects.Count];
        objects.CopyTo(tempObjsArray);
        List<GameObject> tempObjs = tempObjsArray.ToList();


        List<float> distances = new List<float>();
        List<float> angles = new List<float>();

        foreach (KeyValuePair<string, List<Vector3>> kvp in starting_positions) 
        {
            string tag = kvp.Key;
            List<Vector3> positions = kvp.Value;
            List<Quaternion> rotations = starting_rotations[tag];

            int i = 0;
            foreach(Vector3 p in positions) 
            {
                float closest_distance = Mathf.Infinity;
                float closest_angle = Mathf.Infinity;
                GameObject closest_obj = null;
                foreach(GameObject obj in tempObjs) 
                {
                    if (placed.Contains(obj)) 
                    {
                        if (obj.tag == tag)
                        {
                            float dist = Vector3.Distance(p, obj.transform.position);
                            if (dist < closest_distance)
                            {
                                closest_distance = dist;
                                closest_obj = obj;
                            }

                            Quaternion r = rotations[i];
                            float angle = Quaternion.Angle(r, obj.transform.rotation);
                            if(angle < closest_angle)
                            {
                                closest_angle = angle;
                            }
                        }
                    }
                }
                tempObjs.Remove(closest_obj);
                if(closest_distance < Mathf.Infinity) 
                {
                    distances.Add(closest_distance);
                }
                if(closest_angle < Mathf.Infinity)
                {
                    angles.Add(closest_angle);
                }
                i++;

            }
        }

        float[] scores = new float[distances.Count];
        for(int i = 0; i < distances.Count; i++)
        {
            float position_score = Mathf.Clamp01(1f - (distances[i] / 20f));
            float rotation_score = Mathf.Clamp01(1f - (angles[i] / 180f));

            float final_score = (position_score * 0.8f + rotation_score * 0.2f);
            scores[i] = final_score;
        }

        float total = 0;
        foreach(float score in scores) 
        {
            total += score;    
        }
        return Mathf.RoundToInt((total / scores.Length) * 100);

    }


}



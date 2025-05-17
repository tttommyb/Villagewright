using PostFX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Builder : MonoBehaviour
{
    private GameObject current_structure; //Stores the current struture that has been selected

    [SerializeField] private GameObject rotation_circle;

    [SerializeField] private GameObject grid_renderer;

    [SerializeField] private GameObject scroll_bar;
    [SerializeField] private GameObject buttons;

    [SerializeField] private ShadowTurn sundial_script;

    [SerializeField] private GameObject end_screen;
    [SerializeField] private GameObject hud_canvas;

    [SerializeField] private AudioSource soundtrack;
    [SerializeField] private AudioSource place_structure_sound;

    [SerializeField] private Color blue_colour;
    [SerializeField] private Color red_colour;

    [SerializeField] private float build_time;

    [SerializeField] private GameObject star_panel;



    public int selection = -1;
    public int current_structure_index = -1;

    public List<List<GameObject>> inventory = new List<List<GameObject>>();
    public List<GameObject> placed = new List<GameObject>();

    [SerializeField] private GameObject player;

    int total_structures = 0;

    float grid_size = 5;
    float angle = 0;


    List<GameObject> objects;

    [SerializeField] GameObject raid;

    Dictionary<string, List<Vector3>> starting_positions = new Dictionary<string, List<Vector3>>();
    Dictionary<string, List<Quaternion>> starting_rotations = new Dictionary<string, List<Quaternion>>();

    GameObject hovered_structure;

    List<Color> saved_colours = new List<Color>();

    enum MODE
    {
        BUILD = 0, MOVE = 1, DELETE = 2
    }

    MODE current_mode = MODE.MOVE;

    // Start is called before the first frame update
    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        objects = currentScene.GetRootGameObjects().ToList<GameObject>();

        foreach (GameObject @object in objects)
        {
            if (@object.GetComponent<Data>() != null)
            {
                total_structures++;
                if (!starting_positions.ContainsKey(@object.tag))
                {
                    starting_positions.Add(@object.tag, new List<Vector3>());
                    starting_rotations.Add(@object.tag, new List<Quaternion>());
                }
                starting_positions[@object.tag].Add(@object.transform.position);
                starting_rotations[@object.tag].Add(@object.transform.rotation);
                addItemToInventory(@object);
            }
           
            
        }
       

    }

    // Update is called once per frame
    void Update()
    {
        

        Camera.main.GetComponent<TiltShift>().Offset = -((Screen.height / 2) - Input.mousePosition.y) / (Screen.height / 2);
        Camera.main.GetComponent<TiltShift>().XOffset = -((Screen.width / 2) - Input.mousePosition.x) / (Screen.width / 2);

        if (Input.GetKey(KeyCode.Q))
        {
            player.transform.Rotate(new Vector3(0, 100 * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.E))
        {
            player.transform.Rotate(new Vector3(0, -100 * Time.deltaTime, 0));
        }

        if (raid.transform.childCount > 0) 
        {
            return;
        }
        if(scroll_bar.activeInHierarchy == false)
        {
            buttons.GetComponent<CanvasGroup>().alpha = 1;
            scroll_bar.SetActive(true);
        }

        if (soundtrack.isPlaying == false)
        {
            soundtrack.Play();
        }


        if (sundial_script.getHalfCycles() > 3)
        {
            end_screen.SetActive(true);
            hud_canvas.SetActive(false);
            sundial_script.paused = true;
            float score = CalculateAccuracy();
            string enabled_contents = "Failed Contents";
            GameObject content = null;
            if(score > 50)
            {
                enabled_contents = "Passed Contents";

            }
            foreach(Transform t in end_screen.transform)
            {
                if(t.name == enabled_contents)
                {
                    t.gameObject.SetActive(true);
                    content = t.gameObject;
                }
            }
            int stars = 1;
            if(enabled_contents == "Passed Contents")
            {
                if(score > 75)
                {
                    stars = 2;
                }
                if(score == 100)
                {
                    stars = 3;
                }
            }
            int i = 1;
            foreach (Transform t in star_panel.transform)
            {
                if(i == stars)
                {
                    t.gameObject.SetActive(true);
                    i++;
                    break;
                }
                i++;
            }
            
            Debug.LogError(CalculateAccuracy());
        }

      
        if(grid_renderer.activeInHierarchy == false)
        {
            grid_renderer.SetActive(true);
            if(sundial_script.paused == true)
            {
                sundial_script.setCycleTime(build_time);
                sundial_script.paused = false;
                sundial_script.light_modifier = 2f;
            }

        }
        
        //Instantiates the selected object into the scene 

        if(current_structure_index != selection) 
        {
            current_structure = inventory[selection].First();
            current_structure_index = selection;
            current_structure.transform.rotation = Quaternion.Euler(0, player.transform.rotation.y / 45, 0);
        }
        if(current_structure != null) 
        {
            rotation_circle.transform.position = current_structure.transform.position + new Vector3(0, 0.1f, 0);

            rotation_circle.transform.rotation = Quaternion.Euler(new Vector3(90, current_structure.transform.rotation.eulerAngles.y, 0));
        }

        

        


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

  
        //If the R key is hold then rotate the object with mouse
        rotation_circle.GetComponentsInChildren<SpriteRenderer>()[0].enabled = false;
        rotation_circle.GetComponentsInChildren<SpriteRenderer>()[1].enabled = false;

       
        if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Structure")))
        {
            if (hit.point != null && current_structure == null)
            {
                if (current_mode == MODE.MOVE)
                {
                    if (hovered_structure == null)
                    {
                        changeColour(blue_colour, hit.collider.gameObject);
                        hovered_structure = hit.collider.gameObject;
                    }
                    if (Input.GetMouseButtonDown(0))
                    {

                        Debug.Log(hit.collider.gameObject.name);
                        current_structure = hit.collider.gameObject;
                        current_structure.GetComponent<BoxCollider>().enabled = false;

                        return;
                    }
                }
                else if (current_mode == MODE.DELETE)
                {

                    if (hovered_structure == null)
                    {
                        changeColour(red_colour, hit.collider.gameObject);
                        hovered_structure = hit.collider.gameObject;
                    }
                    if (Input.GetMouseButtonDown(0) && hovered_structure != null)
                    {
                        Debug.Log(placed.Remove(hovered_structure));
                        addItemToInventory(hovered_structure);
                        scroll_bar.GetComponentInChildren<DisplayInventory>().initialiseInventory();
                        hovered_structure.transform.position = new Vector3(hovered_structure.transform.position.x, 0, hovered_structure.transform.position.z);

                    }
                }
              
            }
        }
        else if(hovered_structure != null && current_structure == null) 
        {
            int i = 0;
            Debug.Log(saved_colours.Count);
            foreach(Material mat in hovered_structure.GetComponent<Renderer>().materials) 
            {
                uninitialiseTransparentMaterial(mat);
                mat.color = saved_colours[i];
                i++;
            }
            saved_colours.Clear();
            hovered_structure = null;
        }

        if (current_mode == MODE.BUILD)
        {
            scroll_bar.GetComponent<CanvasGroup>().alpha = 1;
        }
        else
        {
            scroll_bar.GetComponent<CanvasGroup>().alpha = 0;
        }


        if (Input.GetKey(KeyCode.R) && !Input.GetMouseButton(0))
        {
            
            if (current_structure != null)
            {
                rotation_circle.GetComponentsInChildren<SpriteRenderer>()[0].enabled = true;
                rotation_circle.GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;

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
            place_structure_sound.Play();
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

    public void setBuildMode(int mode) 
    {
        switch (mode) 
        {
            case 0:
                current_mode = MODE.BUILD;
                break;
            case 1:
                current_mode = MODE.MOVE;
                break;
            case 2:
                current_mode = MODE.DELETE;
                break;
        }
    }

    public void changeColour(Color new_colour, GameObject game_object) 
    {
        Renderer renderer = game_object.GetComponent<Renderer>();
        foreach(Material mat in renderer.materials) 
        {
            initialiseTransparentMaterial(mat);
            saved_colours.Add(mat.color);
            mat.color = new_colour;
        }
    }


    public void initialiseTransparentMaterial(Material mat)
    {
        mat.SetFloat("_Mode", 2); // Fade mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    public void uninitialiseTransparentMaterial(Material mat)
    {
        mat.SetFloat("_Mode", 0); // Opaque mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.EnableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = -1; // Use default queue
    }

    float CalculateAccuracy() 
    {
        GameObject[] tempObjsArray = new GameObject[placed.Count];
        if(placed.Count == 0 || placed.Count < total_structures/2)
        {
            return 0;
        }
        placed.CopyTo(tempObjsArray);
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
            if (distances[i] > 4)
            {
                final_score = 20;
            }
            scores[i] = final_score;
            
        }

        float total = 0;
        foreach(float score in scores) 
        {
            total += score;    
        }
        Debug.Log($"score length: {scores.Length}");

        return Mathf.RoundToInt((Mathf.Clamp01(total / total_structures)) * 100);

    }


    void addItemToInventory(GameObject game_object) 
    {
        bool dupe = false;
        foreach (List<GameObject> items in inventory)
        {
            if (items[0].tag == game_object.tag)
            {
                items.Add(game_object);
                dupe = true;
                break;
            }
        }
        if (!dupe)
        {
            List<GameObject> list = new List<GameObject>();
            list.Add(game_object);
            inventory.Add(list);
        }
    }

    public void nextLevel()
    {
        if(SceneManager.GetActiveScene().buildIndex + 1 >= SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(0);
            return;

        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void returnToMenu()
    {
        SceneManager.LoadScene("MainMenu");

    }

    public void replayScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

}



using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DisplayInventory : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject removed_icons;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject scroll_view;
    [SerializeField] private GameObject viewport;
    [SerializeField] private Font font;

    List<Transform> configured_objects = new List<Transform>();

    float fade_speed = 0.25f;

    bool faded = false;

    void OnEnable()
    {
        initialiseInventory();
    }


    public void Update()
    {
        bool removed = false;
        Transform removed_transform = null;
        foreach(Transform button in content.transform) 
        {
                Debug.Log(int.Parse(button.name));
            if(int.Parse(button.name) < player.GetComponent<Builder>().inventory.Count) 
            {
               button.gameObject.GetComponentInChildren<UnityEngine.UI.Text>().text = "x" + player.GetComponent<Builder>().inventory[int.Parse(button.name)].Count;
                if (player.GetComponent<Builder>().inventory[int.Parse(button.name)].Count <= 0)
                {
                    removed_transform = button.transform;
                    player.GetComponent<Builder>().inventory.RemoveAt(int.Parse(button.name));
                    removed = true;
                }
            }
            if (removed == true)
            {
                button.name = (int.Parse(button.name) - 1).ToString();  
            }
        }
        if (removed_transform != null)
        {
            removed_transform.SetParent(removed_icons.transform);
        }

        if (faded) { return; }
        bool allFaded = true;
        foreach (List<GameObject> items in player.GetComponent<Builder>().inventory)
        {
            foreach (GameObject item in items)
            {
                ParticleSystem[] particles = item.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particle in particles)
                {
                    particle.Stop();
                }

                foreach (Transform t in item.transform)
                {
                    if (t.tag == "Ruin")
                    {
                        foreach (Transform t2 in t)
                        {
                            if(t2.tag == "Fire") { continue; }
                            Renderer renderer = t2.GetComponent<Renderer>();

                            if (!configured_objects.Contains(t2))
                            {
                                initialiseFadeMaterial(renderer);
                                configured_objects.Add(t2);
                            }

                            if (renderer != null && renderer.material != null)
                            {
                                Color color = renderer.material.color;
                                if (color.a > 0)
                                {
                                    color.a -= fade_speed * Time.deltaTime;
                                    renderer.material.color = color;
                                    allFaded = false; // Still fading
                                }
                            }
                        }
                    }
                }
            }
        }

        if (allFaded)
        {
            faded = true;
            for(int i = 0; i < 1;)
            {
                Destroy(configured_objects[i].gameObject);
                configured_objects.RemoveAt(i);
                if(configured_objects.Count == 0)
                {
                    i++;
                }
            }
        }

    }
    public void buttonClick(string index) 
    {
        if (player.GetComponent<Builder>().inventory[int.Parse(index)].Count > 0) 
        {
            player.GetComponent<Builder>().selection = int.Parse(index);
        }
    }

    public void initialiseFadeMaterial(Renderer r) 
    {
        Material mat = r.material;

        // ONE-TIME setup: Ensure the material supports transparency
        mat.SetFloat("_Mode", 2); // Fade mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    public void initialiseInventory()
    {
        foreach (Transform t in content.transform)
        {
            Destroy(t.gameObject);
        }

        int i = 0;
        foreach (List<GameObject> items in player.GetComponent<Builder>().inventory)
        {
            Data inventory_data = items[0].GetComponent<Data>();
            GameObject image_object = new GameObject("InventoryItem", typeof(RectTransform));
            image_object.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { buttonClick(image_object.name); });

            image_object.AddComponent<UnityEngine.UI.Image>().sprite = inventory_data.icon;

            GameObject text_object = new GameObject();
            text_object.AddComponent<UnityEngine.UI.Text>().text = "x" + items.Count.ToString();
            UnityEngine.UI.Text text = text_object.GetComponent<UnityEngine.UI.Text>();
            text.font = font;
            text.fontSize = 45;
            text.alignment = TextAnchor.LowerRight;


            image_object.name = i.ToString();
            i++;
            image_object.transform.SetParent(content.transform);
            text_object.transform.SetParent(image_object.transform);



        }
    }
}


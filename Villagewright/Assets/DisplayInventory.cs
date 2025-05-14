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
    [SerializeField] private Font font;

    void OnEnable()
    {
        
        int i = 0;
        foreach(List<GameObject> items in player.GetComponent<Builder>().inventory)
        {
            Data inventory_data = items[0].GetComponent<Data>();
            GameObject image_object = new GameObject("InventoryItem", typeof(RectTransform));
            image_object.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { buttonClick(image_object.name); } );

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
    }
    public void buttonClick(string index) 
    {
        if (player.GetComponent<Builder>().inventory[int.Parse(index)].Count > 0) 
        {
            player.GetComponent<Builder>().selection = int.Parse(index);
        }
    }

}

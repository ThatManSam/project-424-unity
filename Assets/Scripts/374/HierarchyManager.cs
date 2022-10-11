using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HierarchyManager : MonoBehaviour
{
    //[SerializeField]
    //public List<GameObject> goList = new List<GameObject>();

    [SerializeField]
    private GameObject ItemPrefab;

    [SerializeField]
    private Transform ContentContainer;

    public void AddObject(GameObject go)
    {

        //goList.Add(go);

        // Instatiate scroll item prefab
        var itemGO = Instantiate(ItemPrefab);

        // Set object name
        itemGO.name = "HierarchyItem_" + go.name;
        itemGO.GetComponentInChildren<Text>().text = go.name;

        // Add onClick listener to button
        Button btn = itemGO.GetComponentInChildren<Button>();
        btn.onClick.AddListener(delegate { this.GetComponent<SelectionManager>().SetSelection(go); });

        //itemGO.GetComponentInChildren<Selectable>().Select();
        //item_go.GetComponent<Image>().color = i % 2 == 0 ? Color.yellow : Color.cyan;
        itemGO.transform.SetParent(ContentContainer, false);
        itemGO.transform.localScale = Vector2.one;
        //PrintList();
    }

    public void RemoveObject(GameObject go)
    {
        //goList.Remove(go);
        Destroy(GameObject.Find("HierarchyItem_" + go.name));

    }

    //public void PrintList()
    //{
    //    Debug.Log("GameObject List:");
    //    foreach (GameObject go in goList)
    //    {
    //        Debug.Log("> " + go.name);
    //    }
    //}

    public void Console(string name)
    {
        Debug.Log("~~~~~ SELECTION: " + name + " ~~~~~~");
    }
}

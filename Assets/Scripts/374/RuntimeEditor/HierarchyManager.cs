using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HierarchyManager : MonoBehaviour
{

    [SerializeField]
    private GameObject ItemPrefab;

    [SerializeField]
    private Transform ContentContainer;

    public void AddObject(GameObject go)
    {
        // Instatiate scroll item prefab
        var itemGO = Instantiate(ItemPrefab);

        // Set object name
        itemGO.name = "HierarchyItem_" + go.name;
        itemGO.GetComponentInChildren<Text>().text = go.name;

        // Add onClick listener to button
        Button btn = itemGO.GetComponentInChildren<Button>();
        btn.onClick.AddListener(delegate { this.GetComponent<SelectionManager>().SetSelection(go); });

        // Add item to content container
        itemGO.transform.SetParent(ContentContainer, false);
        itemGO.transform.localScale = Vector2.one;
    }

    public void RemoveObject(GameObject go)
    {
        Destroy(GameObject.Find("HierarchyItem_" + go.name));
    }

}

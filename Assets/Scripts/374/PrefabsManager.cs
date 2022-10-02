using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefabsManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> ObjectPanelsList = new List<GameObject>();

    [SerializeField]
    private GameObject objectText;

    private int pos = 0;

    // 0 = left, 1 = right
    public void Scroll(int direction)
    {
        if (direction == 0)
        {
            if (pos == 0)
            {
                ObjectPanelsList[pos].SetActive(false);
                pos = ObjectPanelsList.Count - 1;
                ObjectPanelsList[pos].SetActive(true);
                objectText.GetComponent<Text>().text = ObjectPanelsList[pos].name;
            }
            else
            {
                ObjectPanelsList[pos].SetActive(false);
                pos--;
                ObjectPanelsList[pos].SetActive(true);
                objectText.GetComponent<Text>().text = ObjectPanelsList[pos].name;
            }
        }
        else if (direction == 1)
        {
            if (pos == ObjectPanelsList.Count - 1)
            {
                ObjectPanelsList[pos].SetActive(false);
                pos = 0;
                ObjectPanelsList[pos].SetActive(true);
                objectText.GetComponent<Text>().text = ObjectPanelsList[pos].name;
            }
            else
            {
                ObjectPanelsList[pos].SetActive(false);
                pos++;
                ObjectPanelsList[pos].SetActive(true);
                objectText.GetComponent<Text>().text = ObjectPanelsList[pos].name;
            }
        }
        else
        {
            //nothing
        }
    }
}

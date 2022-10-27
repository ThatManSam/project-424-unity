using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadingLabelController : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] Text message;

    public static DownloadingLabelController Instance;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public DownloadingLabelController SetMessage(string message)
    {
        this.message.text = message;
        return Instance;
    }

    public DownloadingLabelController Show()
    {
        canvas.SetActive(true);
        return Instance;
    }

    public DownloadingLabelController Hide()
    {
        canvas.SetActive(false);
        return Instance;

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

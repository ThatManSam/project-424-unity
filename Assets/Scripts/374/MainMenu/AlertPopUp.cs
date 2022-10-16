using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlertDialog
{
    public string Title = "Error:";
    public string Message = "Default Message";
}

public class AlertPopUp : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] Text alertTitle;
    [SerializeField] Text alertMessage;
    [SerializeField] Button closeAlertButton;

    AlertDialog alertDialog = new AlertDialog();

    public static AlertPopUp Instance;

    void Awake()
    {
        Instance = this;

        closeAlertButton.onClick.RemoveAllListeners();
        closeAlertButton.onClick.AddListener(Hide);

        Hide();
    }

    public AlertPopUp SetTitle(string title)
    {
        alertDialog.Title = title;
        return Instance;
    }

    public AlertPopUp SetMessage(string message)
    {
        alertDialog.Message = message;
        return Instance;
    }

    public AlertPopUp Show()
    {
        alertTitle.text = alertDialog.Title;
        alertMessage.text = alertDialog.Message;

        canvas.SetActive(true);
        return Instance;
    }

    public void Hide()
    {
        canvas.SetActive(false);

        alertDialog = new AlertDialog();
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

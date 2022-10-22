using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalSite : MonoBehaviour
{

    public string siteName;

    public void OpenExternalLink()
    {
        Application.OpenURL(siteName);
    }
}

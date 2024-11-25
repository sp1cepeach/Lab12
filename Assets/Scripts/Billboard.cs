using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static WeatherManager;

public class Billboard : MonoBehaviour
{
    [SerializeField] private string webImageUrl;
    private Texture2D webImage;

    public IEnumerator DownloadImage(Action<Texture2D> callback)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(webImageUrl);
        yield return request.SendWebRequest();
        callback(DownloadHandlerTexture.GetContent(request));
    }

    private void Start()
    {
        GetWebImage(OnImageLoaded);
    }

    void GetWebImage(Action<Texture2D> callback)
    {
        if (webImage == null)
        {
            StartCoroutine(DownloadImage(callback));
        }
        else
        {
            callback(webImage);
        }
    }

    public void OnImageLoaded(Texture2D texture)
    {
        webImage = texture;
        GetComponent<MeshRenderer>().material.mainTexture = texture;
    }
}

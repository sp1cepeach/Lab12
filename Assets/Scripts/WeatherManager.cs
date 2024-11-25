using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherManager : MonoBehaviour
{
    public enum City
    {
        Orlando,
        Paris,
        Beijing,
        London,
        Tokyo
    }

    [SerializeField] City city;
    [SerializeField] Material skyboxDay;
    [SerializeField] Material skyboxNight;
    [SerializeField] Material skyboxRain;
    private new Light light;
    private string apiKey;
    private string xmlApi;

    private IEnumerator CallAPI(string url, Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"network problem: {request.error}");
            }
            else if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"response error: {request.responseCode}");
            }
            else
            {
                callback(request.downloadHandler.text);
            }
        }
    }

    public IEnumerator GetWeatherXML(Action<string> callback, City city)
    {
        return CallAPI(xmlApi + "&q=" + city.ToString(), callback);
    }

    private void Awake()
    {
        light = FindAnyObjectByType<Light>().GetComponent<Light>();
        apiKey = File.ReadAllText(Application.dataPath + "/openweathermap.key");
        xmlApi = "http://api.openweathermap.org/data/2.5/weather?appid=" + apiKey;
    }

    private void OnValidate()
    {
        StartCoroutine(GetWeatherXML(OnXMLDataLoaded, city));
    }

    private void Start()
    {
        StartCoroutine(GetWeatherXML(OnXMLDataLoaded, city));
    }

    public void OnXMLDataLoaded(string data)
    {
        Debug.Log("Data: " + data);
        WeatherResponse response = JsonUtility.FromJson<WeatherResponse>(data);
        Debug.Log("weather: " + response.weather[0].id + " " + response.weather[0].main);
        Debug.Log("dt: " + response.dt);
        Debug.Log("timezone: " + response.timezone);
        Debug.Log("sys.sunrise: " + response.sys.sunrise);
        Debug.Log("sys.sunset: " + response.sys.sunset);
        if (response.weather[0].main == "Rain")
        {
            RenderSettings.skybox = skyboxRain;
            light.intensity = 0.75f;
        }
        else
        {
            //DateTime localTime = DateTimeOffset.FromUnixTimeSeconds(response.dt).DateTime;
            //Debug.Log("LocalTime: " + localTime.ToString());
            //if (6 <= localTime.Hour && localTime.Hour <= 18)
            if (response.sys.sunrise <= response.dt && response.dt < response.sys.sunset)
            {
                RenderSettings.skybox = skyboxDay;
                light.intensity = 1;
            }
            else
            {
                RenderSettings.skybox = skyboxNight;
                light.intensity = 0.5f;
            }
        }
    }

    [System.Serializable]
    class WeatherResponse
    {
        public Weather[] weather;
        public int dt;
        public int timezone;
        public Sys sys;
    }

    [System.Serializable]
    class Weather
    {
        public int id;
        public string main;
    }

    [System.Serializable]
    class Sys
    {
        public int sunrise;
        public int sunset;
    }
}

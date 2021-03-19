using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PreMenu : MonoBehaviour
{
    public Text text;
    public Slider slider;
    public string[] urls;

    private int check = 0;
    private float progress = 0;

    void Start()
    {
        text.text = "Проверка подключения к интернету";
        StartCoroutine(ProgressCheck(0.4f));
        StartCoroutine(TestConnection());
        
        

        if (check == 5) text.text = "Подключение к интернету отсутствует!";
    }

    IEnumerator TestConnection()
    {
        foreach (string url in urls)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.isNetworkError == false)
            {
                text.text = "Подключено!";
                StartCoroutine(ProgressCheck(1f));
                yield break;
            }
            else if (request.isNetworkError == true)
            {
                check++;
                text.text = "Подключение к интернету отсутствует!";
            }

        }
    }

    IEnumerator ProgressCheck(float percent)
    {
        while (slider.value < percent)
        {
            slider.value = progress;
            progress += 0.005f;
            if (slider.value == 1f) SceneManager.LoadScene("Game", LoadSceneMode.Single);
            yield return new WaitForSeconds(0.01f);
        }
    }

}

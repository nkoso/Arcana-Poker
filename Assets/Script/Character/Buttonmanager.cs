using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
public class Buttonmanager : MonoBehaviour
{
    public Sprite[] charagra=new Sprite[10];
    private Level level;
    public GameObject panel;
    public GameObject helpPanel;
    private string charaName=null;
    private GameObject button;
    private bool mult;
    private System.Random random;
    Dictionary<string, Color> color = new Dictionary<string, Color>()
    {
        {"White",new Color(0.9f,0.9f,0.9f,1.0f) },
        {"Gray",new Color(0.8f,0.8f,0.8f,1.0f) },
        {"Purple",new Color(1.0f,0.0f,1.0f,0.7f) },
        {"Black",new Color(0.2f,0.2f,0.2f,0.85f) },
        { "Green" ,new Color(0.0f,0.8f,0.0f,0.7f)},
    };

    private void Start()
    {
        random = new System.Random(CurrentTimeSeed());
    }
    
    public void ButtonClick(GameObject obj)
    {
        if (button != null)
        {
            button.GetComponent<Image>().color = color["Green"];
            button.transform.GetChild(0).GetComponent<Text>().color = color["Black"];
        }
        obj.GetComponent<Image>().color = color["Purple"];
        obj.transform.GetChild(0).GetComponent<Text>().color = color["Gray"];
        button = obj;
        charaName = obj.name;
    }
    public void StartButton()
    {
        if (charaName!=null)
        {
            panel.SetActive(true);
            mult = false;
        }
    }
    public void Multi()
    {
        if (charaName!=null)
        {
            mult = true;
            panel.SetActive(true);
        }
    }
    public void LevelButton(string str)
    {
        if (str == "easy") level = Level.easy;
        else if (str == "normal") level = Level.normal;
        else if (str == "hard") level = Level.hard;
        else if (str == "expert") level = Level.expert;
        SceneManager.sceneLoaded += GameSceneLoaded;
        if (!mult) SceneManager.LoadScene("poker");
        else SceneManager.LoadScene("PhotonConnect");
    }
    public void ExitButton()
    {
        SceneManager.LoadScene("StartGamen");
    }
    private void GameSceneLoaded(Scene next, LoadSceneMode mode)
    {
        if (next.name=="poker")
        {
            // シーン切り替え後のスクリプトを取得
            var gameManager = GameObject.FindWithTag("GameManager").GetComponent<CardManager>();
            // データを渡す処理
            gameManager.nameset(charaName, Num2Name(random.Next(0,10)));
            gameManager.level = level;
            // イベントから削除
        }
        else if (next.name == "PhotonConnect")
        {
            var manager = GameObject.FindWithTag("GameManager").GetComponent<Maching>();
            manager.level= level;
            manager.charaName = charaName;
        }
        SceneManager.sceneLoaded -= GameSceneLoaded;
    }
    private string Num2Name(int num)
    {
        if (num == 0) return "reimu";
        else if (num == 1) return "marisa";
        else if (num == 2) return "sakuya";
        else if (num == 3) return "youmu";
        else if (num == 4) return "remiria";
        else if (num == 5) return "fran";
        else if (num == 6) return "yuyuko";
        else if (num == 7) return "sanae";
        else if (num == 8) return "satori";
        else if (num == 9) return "koisi";
        else return "reimu";
    }
    public int CurrentTimeSeed()
    {
        DateTime dt = DateTime.Now;
        return dt.Day + dt.Hour + dt.Minute + dt.Second + dt.Millisecond;
    }
    public void help()
    {
        helpPanel.SetActive(true);
    }
}

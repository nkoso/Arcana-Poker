using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

/*Cardのデータを表示するパネル
 * 表示するデータをPanelSet()で受け取っって表示する。
 */
public class CardPanel : MonoBehaviour
{
    [SerializeField] Image card_gra;
    [SerializeField] Text Attack;
    [SerializeField] Text Defense;
    [SerializeField] Text Effect;
    private Level level;
    [SerializeField] Transform HT;
    Dictionary<string, Color> color = new Dictionary<string, Color>()
    {
        {"White",new Color(0.9f,0.9f,0.9f,1.0f) },
        {"Gray",new Color(0.8f,0.8f,0.8f,1.0f) },
        {"Purple",new Color(1.0f,0.0f,1.0f,0.7f) },
        {"Black",new Color(0.2f,0.2f,0.2f,0.85f) },
        { "Green" ,new Color(0.0f,0.8f,0.0f,0.7f)},
    };
    private bool[,] HSupportTable = new bool[23, 23];

    private void Start()
    {
        var trigger = GetComponent<EventTrigger>();
        var entry= new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) => {
            gameObject.SetActive(false);
             });
        trigger.triggers.Add(entry);
    }
    public void PanelSet(string name)
    {
        Card card = arcana2Card(name,"");
        card_gra.sprite = Resources.Load<Sprite>("Deck/" + card.Number.ToString());
        Attack.text = "攻撃力：" + card.Attack.ToString();
        Defense.text = "守備力：" + card.Defense.ToString();
        Effect.text = card.Etext;
        foreach(Transform item in HT)
        {
            Destroy(item.gameObject);
        }
        for(int i = 0; i < card.Htext_sentence.Length; i++)
        {
            GameObject obj = Instantiate((GameObject)Resources.Load("Prefab/Text/HEtext"));
            obj.transform.SetParent(HT, false);
            string text = card.Htext_sentence[i] + Environment.NewLine;
            if (level == Level.easy || level == Level.normal)
            {
                text = "このレベルでは、役は発動しません。";
                obj.GetComponent<Text>().text = text;
                break;
            }
            if (HSupportTable[card.Number,card.hand[i]]) obj.GetComponent<Text>().color = color["Purple"];
            text = text.Replace(" ", "\u00A0");
            obj.GetComponent<Text>().text = text;
        }
    }
    public void TableSet(int i,int j)
    {
        HSupportTable[i, j] = true;
        HSupportTable[j, i] = true;
    }
    public void TableReset()
    {
        for(int i = 0; i < 23; i++)
        {
            for (int j = 0; j < 23; j++) HSupportTable[i, j] = false;
        }
    }
    public void LevelSet(Level l)
    {
        level = l;
    }
    Card arcana2Card(string name, string tag)
    {
        if (name == "The Fool") return new The_Fool(tag);
        else if (name == "The Magician") return new The_Magician(tag);
        else if (name == "The High Priestess") return new The_High_Priestess(tag);
        else if (name == "The Empress") return new The_Empress(tag);
        else if (name == "The Emperor") return new The_Emperor(tag);
        else if (name == "The Hierophant") return new The_Hierophant(tag);
        else if (name == "The Lovers") return new The_Lovers(tag);
        else if (name == "The Chariot") return new The_Chariot(tag);
        else if (name == "Strength") return new Strength(tag);
        else if (name == "The Hermit") return new The_Hermit(tag);
        else if (name == "Wheel of Fortune") return new Wheel_of_Fortune(tag);
        else if (name == "Justice") return new Justice(tag);
        else if (name == "The Hanged Man") return new The_Hanged_Man(tag);
        else if (name == "Death") return new Death(tag);
        else if (name == "Temperance") return new Temperance(tag);
        else if (name == "The Devil") return new The_Devil(tag);
        else if (name == "The Tower") return new The_Tower(tag);
        else if (name == "The Star") return new The_Star(tag);
        else if (name == "The Moon") return new The_Moon(tag);
        else if (name == "The Sun") return new The_Sun(tag);
        else if (name == "Judgement") return new Judgement(tag);
        else if (name == "The World") return new The_World(tag);
        return new Citizen(tag);
    }
}

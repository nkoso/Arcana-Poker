using UnityEngine;
using UnityEngine.UI;
using System;

/*手札とフィールドゾーンのスクリプト
 * クリックされた時に、プレイヤー側でカード選択中ならボタンを表示する。
 * 手札ならば、detailのボタンもだす。
 * セットボタン０、ー１
 * detail -0.8 -0.65
 * field 0,-1.2
 */

public class zoneScriptP : Photon.MonoBehaviour
{
   private PlayerP player;
    private GameObject panel;
    private CardManagerP manager;
    private Text eText;
    private bool flag;
    public bool Locked { get; set; }
    public string number { get; set; }
    private GameObject[] adtext = new GameObject[2];
    private Color[] textColor = { Color.red, Color.blue };
    private Vector3[] textPos = { new Vector3(-0.8f, 0.3f, 0.0f), new Vector3(0.8f, 0.3f, 0.0f) };
    private Vector3[] buttonPos = { new Vector3(0f, -1.05f, 0f), new Vector3(0f, -1.2f, 0f), new Vector3(-0.8f, -0.65f, 0f) , new Vector3(-0.8f, 0.65f, 0f) };

    // Start is called before the first frame update
    void Start()
    {
        flag = false;
        manager = GameObject.FindWithTag("GameManager").GetComponent<CardManagerP>();
        panel = GameObject.FindWithTag("PlayerPanel");
        if (tag.Contains("Player"))
        {
            player = transform.root.gameObject.GetComponent<PlayerP>();
            eText = GameObject.FindWithTag("EtextP").GetComponent<Text>();
        }
        else if (tag.Contains("Enemy"))
        {
            player = GameObject.Find("Player").GetComponent<PlayerP>();
            eText = GameObject.FindWithTag("EtextE").GetComponent<Text>();
        }
    }
    
    private void OnMouseDown()
    {
        if (transform.childCount != 0&&manager.state != State.Battle)
        {
            if (tag.Contains("Player"))
            {
                if (manager.state == State.Set && !Locked)
                {
                    GameObject button = Instantiate((GameObject)Resources.Load("Prefab/Button/cardButton"));
                    button.name = name;
                    button.transform.SetParent(transform, false);
                    if (name.Contains("Hand"))
                    {
                        button.GetComponent<Button>().onClick.AddListener(() => { player.Hand2Field(number); });
                        button.transform.localPosition = buttonPos[0];
                        button.transform.GetChild(0).GetComponent<Text>().text = "SET";
                    }
                    else if (name.Contains("Field"))
                    {
                        button.GetComponent<Button>().onClick.AddListener(() => { player.Field2Hand(number); });
                        button.transform.localPosition = buttonPos[1];
                        button.transform.GetChild(0).GetComponent<Text>().text = "BACK";
                    }
                    button.transform.SetParent(manager.UIpanel, true);
                    button.transform.localScale = new Vector3(1f, 1f, 0f);
                    player.buffer.Add(button);                    
                }
                GameObject Dbutton = Instantiate((GameObject)Resources.Load("Prefab/Button/DetailButton"));
                Dbutton.name = name;
                Dbutton.transform.SetParent(transform, false);
                Dbutton.transform.localPosition = buttonPos[2];
                Dbutton.GetComponent<Button>().onClick.AddListener(detail);
                Dbutton.transform.SetParent(manager.UIpanel, true);
                Dbutton.transform.localScale = new Vector3(1f, 1f, 0f);
                player.buffer.Add(Dbutton);
            }
            else
            {
                if (transform.GetChild(0).GetComponent<SpriteRenderer>().sprite != manager.cardback)
                {
                    GameObject Dbutton = Instantiate((GameObject)Resources.Load("Prefab/Button/DetailButton"));
                    Dbutton.name = name;
                    Dbutton.transform.SetParent(transform, false);
                    Dbutton.transform.localPosition = buttonPos[3];
                    Dbutton.GetComponent<Button>().onClick.AddListener(detail);
                    Dbutton.transform.SetParent(manager.UIpanel, true);
                    Dbutton.transform.localRotation = Quaternion.identity;
                    Dbutton.transform.localScale = new Vector3(1f, 1f, 1f);
                    player.buffer.Add(Dbutton);
                }
            }
        }
    }
    
    private void OnMouseEnter()
    {
        if (manager.state != State.Battle)
        {
            if (tag.Contains("Player") && transform.childCount != 0 && !flag && name.Contains("Hand"))
            {
                CardText();
            }
            else if (tag.Contains("Enemy") && transform.childCount != 0 && !flag)
            {
                if (transform.GetChild(0).GetComponent<SpriteRenderer>().sprite != manager.cardback)
                {
                    CardText();
                }
            }
        }
    }
    
    private void CardText()
    {
        flag = true;
        Card card = arcana2Card(transform.GetChild(0).name, tag);
        for(int i = 0; i < 2; i++)
        {
            GameObject atext = Instantiate((GameObject)Resources.Load("Prefab/Text/AText"));
            atext.GetComponent<Text>().color = textColor[i];
            atext.transform.SetParent(transform, false);
            atext.transform.localPosition = textPos[i];
            atext.GetComponent<Text>().text = i == 0 ? card.Attack.ToString() : card.Defense.ToString();
            atext.transform.SetParent(panel.transform, true);
            adtext[i] = atext;
        }
        eText.enabled = true;
        string num = card.Number == 22 ? "0" : card.Number.ToString();
        string str = "Name: " + num2arcana(card.Number) + "     " + "Number: " + num + Environment.NewLine;
        if (card.Number == 22)
        {
            str += "詳細で確認";
        }
        else str += card.Etext;
        eText.text = str;
    }
    private void OnMouseExit()
    {
        if (flag)
        {
            flag = false;
            for (int i = 0; i < 2; i++) Destroy(adtext[i]);
            eText.enabled = false;
        }
    }
    //CardPanelにデータを投げる
    private void detail()
    {
        player.cardPanel.SetActive(true);
        player.cardPanel.GetComponent<CardPanel>().PanelSet(transform.GetChild(0).gameObject.name);
    }
    public static string num2arcana(int i)
    {
        if (i == 22) return "The Fool";
        else if (i == 1) return "The Magician";
        else if (i == 2) return "The High Priestess";
        else if (i == 3) return "The Empress";
        else if (i == 4) return "The Emperor";
        else if (i == 5) return "The Hierophant";
        else if (i == 6) return "The Lovers";
        else if (i == 7) return "The Chariot";
        else if (i == 8) return "Strength";
        else if (i == 9) return "The Hermit";
        else if (i == 10) return "Wheel of Fortune";
        else if (i == 11) return "Justice";
        else if (i == 12) return "The Hanged Man";
        else if (i == 13) return "Death";
        else if (i == 14) return "Temperance";
        else if (i == 15) return "The Devil";
        else if (i == 16) return "The Tower";
        else if (i == 17) return "The Star";
        else if (i == 18) return "The Moon";
        else if (i == 19) return "The Sun";
        else if (i == 20) return "Judgement";
        else if (i == 21) return "The World";
        return "Citizen";
    }
    public static Card arcana2Card(string name, string tag)
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

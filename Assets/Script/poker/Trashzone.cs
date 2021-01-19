using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Trashzoneのスクリプト
 * Trashのカードの表示など
 */
public class Trashzone : MonoBehaviour
{
    private List<string> trashcard=new List<string>(); //Trashにあるカードの一覧。今のところ使用していない
    private GameObject menu; //TrashUI
    private Transform TrashTop; //トラッシュの一番上のカード。表示用
    public GameObject cardpanel; //CArdPanel。TrashUIを開いた時に邪魔だから消す用
    private CardManager manager;

    private void Start()
    {
        manager = GameObject.FindWithTag("GameManager").GetComponent<CardManager>();
    }
    void Update()
    {
        if (TrashTop != null)
        {
            Vector3 pos = new Vector3(0, 0, 0f);
            TrashTop.localPosition = pos;
        }
    }
    private void OnMouseDown()
    {
        if (menu==null)
        {
            menu = Instantiate((GameObject)Resources.Load("Prefab/MenuBar/TrashMenue"));
            menu.name = "tmenu";
            menu.transform.SetParent(transform, false);
            menu.transform.localPosition = Vector3.zero;
            menu.transform.SetParent(manager.UIpanel, true);
            menu.transform.localRotation = Quaternion.identity;
            menu.transform.localScale = new Vector3(1f, 1f, 1f);
            TrashPanel tp = menu.GetComponent<TrashPanel>();
            tp.ListSet(trashcard,cardpanel);
        }
    }
    public void MenuOn()
    {
        GetComponent<BoxCollider2D>().enabled = true;
    }
    public void ChangeCard(string[] change, string after)
    {
        for (int i = 0; i < trashcard.Count; i++)
        {
            for (int j = 0; j < change.Length; j++)
            {
                if (trashcard[i].Contains(change[j]))
                {
                    trashcard.Remove(trashcard[i]);
                    trashcard.Add(after);
                }
            }
        }
    }
    public void MenuOff()
    {
        if (menu != null) menu.GetComponent<TrashPanel>().Reset();
        GetComponent<BoxCollider2D>().enabled = false;
    }
    //TrashTopを入れ替える。ノードの追加
    public void AddTrash(Transform obj)
    {
        if (TrashTop != null) Destroy(TrashTop.gameObject);
        obj.parent = transform;
        TrashTop = obj;
        trashcard.Add(obj.name);
    }
    public List<string> GetListTrashCard()
    {
        return trashcard;
    }
    public void Salvage(string str)
    {
        trashcard.Remove(str);
        if (TrashTop != null)
        {
            if (TrashTop.name == str)
            {
                if (trashcard.Count == 0)
                {
                    TrashTop.GetComponent<SpriteRenderer>().enabled = false;
                    Destroy(TrashTop.gameObject);
                }else
                {
                    GameObject obj = new GameObject(trashcard[trashcard.Count - 1]);
                    obj.tag = tag;
                    obj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    obj.AddComponent<SpriteRenderer>().sprite = manager.arcana[arcana2num(trashcard[trashcard.Count-1])];
                    obj.transform.parent = transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                    TrashTop = obj.transform;
                }
            }
        }
        
    }
    int arcana2num(string name)
    {
        if (name == "The Fool") return 22;
        else if (name == "The Magician") return 1;
        else if (name == "The High Priestess") return 2;
        else if (name == "The Empress") return 3;
        else if (name == "The Emperor") return 4;
        else if (name == "The Hierophant") return 5;
        else if (name == "The Lovers") return 6;
        else if (name == "The Chariot") return 7;
        else if (name == "Strength") return 8;
        else if (name == "The Hermit") return 9;
        else if (name == "Wheel of Fortune") return 10;
        else if (name == "Justice") return 11;
        else if (name == "The Hanged Man") return 12;
        else if (name == "Death") return 13;
        else if (name == "Temperance") return 14;
        else if (name == "The Devil") return 15;
        else if (name == "The Tower") return 16;
        else if (name == "The Star") return 17;
        else if (name == "The Moon") return 18;
        else if (name == "The Sun") return 19;
        else if (name == "Judgement") return 20;
        else if (name == "The World") return 21;
        return 0;
    }
}

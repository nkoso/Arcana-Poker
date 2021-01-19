using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutIn : MonoBehaviour
{
    public Image Back;
    public Image CharaGra;
    public Text text;
    public Text Name;
    private Animation anim;
    public bool Playing { get; set; }
    private void Start()
    {
        anim = GetComponent<Animation>();
    }
    public void Set(string name,string tag)
    {
        Playing = true;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        anim = GetComponent<Animation>();
        

        switch (name)
        {
            case "reimu":
                text.text = "自分の守備力の合計を+4する。";
                Name.color = Color.blue;
                Name.text = "夢境「二重結界」";
                break;
            case "fran":
                text.text = "自分のHPを3点減少させる。";
                Name.color = Color.red;
                Name.text = "秘弾「そして誰もいなくなるか？」";
                break;
            case "marisa":
                text.text = "自分の攻撃力の合計を+4する。";
                Name.color = Color.red;
                Name.text = "恋符「マスタースパーク」";
                break;
            case "sakuya":
                text.text = "自分の場のカードを自分の手札と入れ替える。";
                Name.color = Color.blue;
                Name.text = "幻世「ザ・ワールド」";
                break;
            case "youmu":
                text.text = "このターン、相手の場のカードの効果・役を無効にする。";
                Name.color = Color.red;
                Name.text = "断霊剣「成仏得脱斬」";
                break;
            case "remiria":
                text.text = "ドローする代わりに、デッキからカードを選んで手札に加える。";
                Name.color = Color.red;
                Name.text = "運命「ミゼラブルフェイト」";
                break;
            case "yuyuko":
                text.text = "自分の手札1枚とトラッシュのカードを入れ替える。";
                Name.color = Color.magenta;
                Name.text = "符牒「死蝶の舞-桜花-」";
                break;
            case "sanae":
                text.text = "自分のHPを3点回復する。";
                Name.color = Color.blue;
                Name.text = "開海「モーゼの奇跡」";
                break;
            case "koisi":
                text.text = "自分に対する効果を無効にする。";
                Name.color = Color.red;
                Name.text = "深層「無意識の遺伝子」";
                break;
            case "satori":
                text.text = "2ターンの間、相手の手札と場のカードを公開した状態にする。";
                Name.color = Color.red;
                Name.text = "地霊符「マインドステラスチール」";
                break;
        }
        Back.sprite = Resources.Load<Sprite>("CutIn/" + name + "back");
        CharaGra.sprite = Resources.Load<Sprite>("CutIn/" + name);
        StartCoroutine("play");
    }
    IEnumerator play()
    {
        Playing = true;
        anim.Play();
        yield return new WaitUntil(() => !anim.isPlaying);
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        Playing = false;
    }
    
    
}

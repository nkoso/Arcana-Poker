using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpPanel : MonoBehaviour
{
    [SerializeField] Image png;
    [SerializeField] Text text;
    private int maxpage;
    private int minpage;
    private int page;
    // Start is called before the first frame update
    
    private void PageSet(int min ,int max)
    {
        minpage = min;
        maxpage = max;
        page = min;
        text.text = "1 / " + (max - min + 1).ToString();
        png.sprite = Resources.Load<Sprite>("help/" + min.ToString());
    }
    //true 1 false -1
    public void PageMove(bool move)
    {
        if (move)
        {
            if (page < maxpage)
            {
                page++;
                text.text = (page-minpage+1).ToString()+" / " + (maxpage - minpage + 1).ToString();
                png.sprite = Resources.Load<Sprite>("help/" + page.ToString());
            }
        }
        else
        {
            if (page > minpage)
            {
                page--;
                text.text = (page - minpage + 1).ToString() + " / " + (maxpage - minpage + 1).ToString();
                png.sprite = Resources.Load<Sprite>("help/" + page.ToString());
            }
        }
    }
    public void Top() { PageSet(1, 1); }
    public void Kihon() { PageSet(2, 2); }
    public void Card() { PageSet(3, 4); }
    public void Hand() { PageSet(5, 9); }
    public void Skill() { PageSet(10, 11); }
    public void Close()
    {
        PageSet(1, 1);
        gameObject.SetActive(false);
    }
    
}

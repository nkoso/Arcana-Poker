using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
 * Trashのリストのノードのスクリプト
 * Set(,)でノードのテキストとボタンをおした時に開くパネルをセット。
 * 今の所CardPanelを開く所にしか使っていない。
 */
public class TrashNode : MonoBehaviour
{
    public Text text;
    public Button button;
    private GameObject panel;
    
    public void Set(string str,GameObject panelO)
    {
        text.text = str;
        panel = panelO;
        button.GetComponent<Button>().onClick.AddListener(panelSet);
    }
    public string Name() { return text.text; }
    public void panelSet()
    {
        panel.SetActive(true);
        panel.GetComponent<CardPanel>().PanelSet(text.text);
    }
    
}

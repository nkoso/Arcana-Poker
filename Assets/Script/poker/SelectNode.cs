using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectNode : MonoBehaviour
{
    public Text text;
    public Button DButton;
    public Button SButton;
    private GameObject panel;
    private BattleTurn battle;
    private BattleTurnP battleP;
    private Player player;
    private PlayerP playerP;

    private void Awake()
    {
        GameObject obj = GameObject.FindWithTag("GameManager");
        GameObject pobj = GameObject.Find("Player");
        if (obj.GetComponent<BattleTurn>())
        {
            battle = obj.GetComponent<BattleTurn>();
            player = pobj.GetComponent<Player>();
        }
        else if (obj.GetComponent<BattleTurnP>())
        {
            battleP = obj.GetComponent<BattleTurnP>();
            playerP= pobj.GetComponent<PlayerP>();
        }

    }
    public void Set(string str, GameObject panel0)
    {
        text.text = str;
        panel = panel0;
        DButton.GetComponent<Button>().onClick.AddListener(panelSet);
        if (battle != null)
        {
            SButton.GetComponent<Button>().onClick.AddListener(() => { battle.SelectedAction(text.text, gameObject); });
            SButton.GetComponent<Button>().onClick.AddListener(() => { player.SelectedAction(text.text, gameObject); });
        }
        else if (battleP != null)
        {
            SButton.GetComponent<Button>().onClick.AddListener(() => { battleP.SelectedAction(text.text, gameObject); });
            SButton.GetComponent<Button>().onClick.AddListener(() => { playerP.SelectedAction(text.text, gameObject); });
        }
    }

    public string Name() { return text.text; }
    public void panelSet()
    {
        panel.SetActive(true);
        panel.GetComponent<CardPanel>().PanelSet(text.text);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerP : CharacterP
{
    public GameObject startbutton;
    public GameObject cardPanel;
    public List<GameObject> buffer;
    private GameObject[] CSButton = new GameObject[2];
    private Vector3[] cspos = { new Vector3(135f, 15f, 0f), new Vector3(135f, -33f, 0f) };
    public Image CSPanel;
    private bool[,] HEtable = new bool[23, 23];
    private List<GameObject> supportbuf = new List<GameObject>();

    private void Start()
    {
        startbutton.GetComponent<Button>().onClick.AddListener(() => { manager.BattleStart(); });
        for (int i = 0; i < 23; i++)
        {
            List<int> list = arcana2Card(num2arcana(i), "").hand;
            for (int j = 0; j < list.Count; j++) HEtable[i, list[j]] = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (manager.state == State.Set && FieldCount == Field.Length) startbutton.SetActive(true);
        else startbutton.SetActive(false);
    }

    /*邪魔なUIを除去する
        * 調整する余地あり
        */
    public void PlayerUIReset()
    {
        for (int i = 0; i < buffer.Count; i++) Destroy(buffer[i]);
        buffer = new List<GameObject>();
        cardPanel.SetActive(false);
    }
    //ハンドサポート
    public void HandSupport()
    {
        SupportReset();
        List<int> list = new List<int>();
        int[] table = new int[23];
        for (int i = 0; i < handmax; i++)
        {
            if (Hand[i].childCount != 0)
            {
                int num = arcana2num(Hand[i].GetChild(0).name);
                list.Add(num);
                table[num] = i;
            }
        }
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = i; j < list.Count; j++)
            {
                if (HEtable[list[i], list[j]])
                {
                    Debug.Log(list[i].ToString() + list[j].ToString());
                    SupportSet(Hand[table[list[i]]].GetChild(0));
                    SupportSet(Hand[table[list[j]]].GetChild(0));
                    cardPanel.GetComponent<CardPanel>().TableSet(list[i], list[j]);
                }
            }
        }
    }
    private void SupportSet(Transform parent)
    {
        //重複させない
        if (parent.childCount != 0) return;
        GameObject sup = Instantiate((GameObject)Resources.Load("Prefab/HandSupportor"), parent);
        sup.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
        sup.transform.localPosition = new Vector3(0f, 3.7f, 0f);
        supportbuf.Add(sup);
    }
    public void SupportReset()
    {
        for (int i = 0; i < supportbuf.Count; i++)
        {
            if (supportbuf[i] != null)
            {
                supportbuf[i].transform.parent = null;
                Destroy(supportbuf[i]);
            }
        }

        cardPanel.GetComponent<CardPanel>().TableReset();
        supportbuf = new List<GameObject>();
    }
    public void Hand2Field(string str)
    {
        PlayerUIReset();
        for (int i = 0; i < Field.Length; i++)
        {
            if (Field[i].childCount == 0)
            {
                Transform card = Hand[int.Parse(str)].GetChild(0);
                if ((card.name.Contains("Fool") || card.name.Contains("World")) && manager.GetBattleTurn() < 5)
                {
                    Debug.Log("条件を満たしていません");
                    break;
                }
                photonview.RPC("RPCHand2Field", PhotonTargets.Others, str);
                CardMovement(card, Field[i]);
                FieldCount++;
                break;
            }
        }
    }
    [PunRPC]
    private void RPCHand2Field(string str)
    {
        manager.CharacterMediator("Hand2Field",tag,str);
    }
    public void Field2Hand(string str)
    {
        PlayerUIReset();
        for (int i = 0; i < Hand.Length; i++)
        {
            if (Hand[i].childCount == 0)
            {
                Transform card = Field[int.Parse(str)].GetChild(0);
                photonview.RPC("RPCField2Hand", PhotonTargets.Others, str);
                CardMovement(card, Hand[i]);
                FieldCount--;
                break;
            }
        }
    }
    [PunRPC]
    private void RPCField2Hand(string str)
    {
        manager.CharacterMediator("Field2Hand", tag,str);
    }
    public void Hand2Deck(string str) { StartCoroutine(hand2Deck(str)); }
    IEnumerator hand2Deck(string str)
    {
        synch = false;
        List<Transform> list = new List<Transform>();
        if (str.Contains("all"))
        {
            for (int i = 0; i < Hand.Length; i++)
            {
                if (Hand[i].childCount != 0)
                {
                    Transform card = Hand[i].GetChild(0);
                    Deck.Add(arcana2num(card.name));
                    list.Add(card);
                }
            }
        }
        else
        {
            Transform card = Hand[int.Parse(str)].GetChild(0);
            Deck.Add(arcana2num(card.name));
            list.Add(card);
        }
        CardMovementD(list.ToArray(), Deckzone);
        yield return new WaitUntil(() => !anim.isPlaying);
        for (int i = 0; i < list.Count; i++) Destroy(list[i].gameObject);
        synch = true;
    }
    public void Hand2Trash(string str) { StartCoroutine(hand2Trash(str)); }
    IEnumerator hand2Trash(string str)
    {
        synch = false;
        Transform card = Hand[int.Parse(str)].GetChild(0);
        CardMovement(card, Trash.transform);
        Trash.AddTrash(card);
        yield return new WaitUntil(() => !anim.isPlaying);
        synch = true;
    }
    public void DT2Hand(string pick, string type) { StartCoroutine(dt2Hand(pick, type)); }
    IEnumerator dt2Hand(string pick, string type)
    {
        synch = false;
        Transform tr = Hand[0];
        if (type == "Deck")
        {
            GameObject obj = Instantiate((GameObject)Resources.Load("Prefab/Card"), Deckzone.position, Quaternion.identity);
            tr = obj.transform;
            Decksearch(arcana2num(pick));
        }
        else if (type == "Trash")
        {
            GameObject obj = Instantiate((GameObject)Resources.Load("Prefab/Card"), Trash.transform.position, Quaternion.identity);
            tr = obj.transform;
            Trash.Salvage(pick);
        }
        tr.tag = tag;
        tr.name = pick;
        tr.GetComponent<SpriteRenderer>().sprite = manager.arcana[arcana2num(pick)];
        Handset(tr);
        yield return new WaitUntil(() => set);
        synch = true;
    }
    protected override IEnumerator marigan()
    {
        mariganb = false;
        selected = false;
        selectCount = handmax;
        selectCansel = false;
        selectName = new List<string>();
        manager.state = State.Select;
        SynchCheck();
        yield return new WaitUntil(() => RPCsynch);
        battle.CanselSet();
        battle.HSelSet(tag);
        yield return new WaitUntil(() => selected || selectCansel);
        PlayerUIReset();
        if (selectName.Count == 0)
        {
            mariganb = true;
            yield break;
        }
        for (int i = 0; i < selectName.Count; i++)
        {
            Hand2Deck(selectName[i]);
            yield return new WaitUntil(() => synch);
        }
        DeckShuffle();
        yield return new WaitUntil(() => synch);
        Draw(selectName.Count);
        yield return new WaitUntil(() => synch);
        mariganb = true;
    }
    protected override void SelectInit()
    {
        CSPanel.enabled = true;
        selected = false;
        selectCount = 1;
        selectCansel = false;
        selectName = new List<string>();
        manager.state = State.Select;
        CSButton = new GameObject[2];
        for (int i = 0; i < 2; i++)
        {
            CSButton[i] = Instantiate((GameObject)Resources.Load("Prefab/Button/cardButton"));
            CSButton[i].transform.SetParent(graphic.transform, false);
            switch (i)
            {
                case 0:
                    CSButton[0].GetComponent<Button>().onClick.AddListener(() => { SelectedAction("Yes", CSButton[0]); });
                    CSButton[0].transform.localPosition = cspos[0];
                    CSButton[0].transform.GetChild(0).GetComponent<Text>().text = "YES";
                    break;
                case 1:
                    CSButton[1].name = "No";
                    CSButton[1].GetComponent<Button>().onClick.AddListener(() => { SelectedAction("No", CSButton[1]); });
                    CSButton[1].transform.localPosition = cspos[1];
                    CSButton[1].transform.GetChild(0).GetComponent<Text>().text = "NO";
                    break;
            }
            CSButton[i].transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
    
    protected override IEnumerator cSkillProcessE(string type)
    {
        if (!CSFlag)
        {
            if (CName == "koisi")
            {
                SelectInit();
                SynchCheck();
                yield return new WaitUntil(() => RPCsynch);                    
                yield return new WaitUntil(() => selectCansel || selected);
                for (int i = 0; i < 2; i++) Destroy(CSButton[i]);
                CSPanel.enabled = false;
                if (selected)
                {
                    cutin.Set(CName, tag);
                    yield return new WaitUntil(() => !cutin.Playing);
                    koisiF = true;
                    CSFlag = true;
                    SynchCheck();
                    yield return new WaitUntil(() => RPCsynch);
                }
            }
        }
        CSbk = true;
    }
    protected override IEnumerator cSKillProcess(CSkillType type)
    {
        if (!CSFlag)
        {
            if (type == CSkillType.TurnS)
            {
                if (CName == "fran")
                {
                    SelectInit();
                    SynchCheck();
                    yield return new WaitUntil(() => RPCsynch);
                    yield return new WaitUntil(() => selectCansel || selected);
                    for (int i = 0; i < 2; i++) Destroy(CSButton[i]);
                    CSPanel.enabled = false;
                    if (selected)
                    {
                        cutin.Set(CName, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        battle.CSkoisi(tag);
                        yield return new WaitUntil(() => (bool)battle.GetCharacterData("CSbk", tag));
                        if ((bool)battle.GetCharacterData("koisiF", tag))
                        {
                            CSFlag = true;
                            CSb = true;
                            yield break;
                        }
                        battle.CSfran(tag);
                        CSFlag = true;
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                    }
                }
                else if (CName == "sanae")
                {
                    SelectInit();
                    SynchCheck();
                    yield return new WaitUntil(() => RPCsynch);
                    yield return new WaitUntil(() => selectCansel || selected);
                    for (int i = 0; i < 2; i++) Destroy(CSButton[i]);
                    CSPanel.enabled = false;
                    if (selected)
                    {
                        cutin.Set(CName, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        HPChange(3);
                        CSFlag = true;
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                    }
                }
                else if (CName == "remiria")
                {
                    SelectInit();
                    SynchCheck();
                    yield return new WaitUntil(() => RPCsynch);
                    yield return new WaitUntil(() => selectCansel || selected);
                    for (int i = 0; i < 2; i++) Destroy(CSButton[i]);
                    CSPanel.enabled = false;
                    if (selected)
                    {
                        cutin.Set(CName, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        selected = false;
                        selectCount = 2;
                        selectName = new List<string>();
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                        battle.SelPanelSet(tag, "Deck");
                        yield return new WaitUntil(() => selected);
                        PlayerUIReset();
                        for (int i = 0; i < 2; i++)
                        {
                            DT2Hand(selectName[i], "Deck");
                            yield return new WaitUntil(() => synch);
                        }
                        DeckShuffle();
                        yield return new WaitUntil(() => synch);
                        CSFlag = true;
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                    }
                }
                else if (CName == "satori")
                {
                    SelectInit();
                    SynchCheck();
                    yield return new WaitUntil(() => RPCsynch);
                    yield return new WaitUntil(() => selectCansel || selected);
                    for (int i = 0; i < 2; i++) Destroy(CSButton[i]);
                    CSPanel.enabled = false;
                    if (selected)
                    {
                        cutin.Set(CName, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        battle.CSkoisi(tag);
                        yield return new WaitUntil(() => (bool)battle.GetCharacterData("CSbk",tag));
                        if ((bool)battle.GetCharacterData("koisiF",tag))
                        {
                            CSFlag = true;
                            CSb = true;
                            yield break;
                        }
                        battle.CSsatori(tag);
                        yield return new WaitUntil(() => battle.CSflag(tag));
                        CSFlag = true;
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                    }
                }
                else if (CName == "yuyuko")
                {
                    SelectInit();
                    SynchCheck();
                    yield return new WaitUntil(() => RPCsynch);
                    yield return new WaitUntil(() => selectCansel || selected);
                    for (int i = 0; i < 2; i++) Destroy(CSButton[i]);
                    CSPanel.enabled = false;
                    if (selected)
                    {
                        cutin.Set(CName, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        //手札を捨てる
                        selected = false;
                        selectCount = 1;
                        selectName = new List<string>();
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                        battle.HSelSet(tag);
                        yield return new WaitUntil(() => selected);
                        PlayerUIReset();
                        Hand2Trash(selectName[0]);
                        yield return new WaitUntil(() => synch);
                        //サルベージする。
                        selected = false;
                        selectCount = 1;
                        selectName = new List<string>();
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                        battle.SelPanelSet(tag, "Trash");
                        yield return new WaitUntil(() => selected);
                        PlayerUIReset();
                        DT2Hand(selectName[0], "Trash");
                        yield return new WaitUntil(() => synch);
                        CSFlag = true;
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                    }
                }
            }
            else if (type == CSkillType.DamegeF)
            {
                if (CName == "reimu")
                {
                    SelectInit();
                    SynchCheck();
                    yield return new WaitUntil(() => RPCsynch);
                    yield return new WaitUntil(() => selectCansel || selected);
                    for (int i = 0; i < 2; i++) Destroy(CSButton[i]);
                    CSPanel.enabled = false;
                    if (selected)
                    {
                        cutin.Set(CName, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        battle.CSreimu(tag);
                        CSFlag = true;
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                    }
                }
                else if (CName == "marisa")
                {
                    SelectInit();
                    SynchCheck();
                    yield return new WaitUntil(() => RPCsynch);
                    yield return new WaitUntil(() => selectCansel || selected);
                    for (int i = 0; i < 2; i++) Destroy(CSButton[i]);
                    CSPanel.enabled = false;
                    if (selected)
                    {
                        cutin.Set(CName, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        battle.CSmarisa(tag);
                        CSFlag = true;
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                    }
                }
                manager.state = State.Battle;
            }
            else if (type == CSkillType.BattleF)
            {
                if (CName == "youmu")
                {
                    SelectInit();
                    SynchCheck();
                    yield return new WaitUntil(() => RPCsynch);
                    yield return new WaitUntil(() => selectCansel || selected);
                    for (int i = 0; i < 2; i++) Destroy(CSButton[i]);
                    CSPanel.enabled = false;
                    if (selected)
                    {
                        cutin.Set(CName, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        battle.CSyoumu(tag);
                        CSFlag = true;
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                    }
                }
            }
            else if (type == CSkillType.BattleS)
            {
                if (CName == "sakuya")
                {
                    SelectInit();
                    SynchCheck();
                    yield return new WaitUntil(() => RPCsynch);
                    yield return new WaitUntil(() => selectCansel || selected);
                    for (int i = 0; i < 2; i++) Destroy(CSButton[i]);
                    CSPanel.enabled = false;
                    if (selected)
                    {
                        cutin.Set(CName, tag);
                        yield return new WaitUntil(() => !cutin.Playing);

                        for (int i = 0; i < 2; i++)
                        {
                            Handset(Field[i].GetChild(0));
                            yield return new WaitUntil(() => set);
                        }
                        manager.state = State.Set;
                        //Setにすると普通のセットが動く。スタートボタンのメソッドを変更しておく
                        Button start = startbutton.GetComponent<Button>();
                        start.onClick.RemoveAllListeners();
                        start.onClick.AddListener(() => { SelectedAction("Yes", start.gameObject); });
                        selected = false;
                        selectCount = 1;
                        selectName = new List<string>();
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                        yield return new WaitUntil(() => selected);
                        start.onClick.RemoveAllListeners();
                        FieldCount = 0;
                        start.onClick.AddListener(() => { manager.BattleStart(); });
                        CSFlag = true;
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                    }
                }
            }
        }
        CSb = true;
    }
}


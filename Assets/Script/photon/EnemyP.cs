using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*エネミー用のスクリプト
 * playerと違ってボタン操作がない
 * なので操作と思考が直結している。
 * 理想はcharacterごとに思考パターンがあること(完全ランダム・攻撃重視・追い詰められるとパターン変更など)
 */
public class EnemyP : CharacterP
{
    public void Hand2Field(string str)
    {
        StartCoroutine(hand2Field(str));
    }
    IEnumerator hand2Field(string str)
    {
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
                if (GetHEbuf(121) && i == 0)
                {
                    Flip(new List<GameObject> { card.gameObject });
                    yield return new WaitUntil(() => flipb);
                }
                CardMovement(card, Field[i]);
                FieldCount++;
                break;
            }
        }
    }
    public void Field2Hand(string str)
    {
        StartCoroutine(field2Hand(str));
    }
    IEnumerator field2Hand(string str)
    {
        for (int i = 0; i < Hand.Length; i++)
        {
            if (Hand[i].childCount == 0)
            {
                Transform card = Field[int.Parse(str)].GetChild(0);
                if (GetHEbuf(121) && str.Contains("0"))
                {
                    Flipura(new List<GameObject> { card.gameObject });
                    yield return new WaitUntil(() => flipb);
                }
                CardMovement(card, Hand[i]);
                Hand[i].GetChild(0).localPosition = Vector3.zero;
                FieldCount--;
                break;
            }
        }
    }
    /*手札からデッキに戻す。マジシャンなど
     * 引数で戻す枚数を決めている。all以外だと1枚戻す。
     * 戻すカードを５ターンより短いならワールドとフール優先。そうでないなら数字の低いカードを戻すようにした。
     * synchで同期をとる
     */
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
    /*手札からトラッシュへ
     * synchで同期ををとる
     * 
     */

    public void Hand2Trash(string str) { StartCoroutine(hand2Trash(str)); }
    IEnumerator hand2Trash(string str)
    {
        synch = false;
        Transform card = Hand[int.Parse(str)].GetChild(0);
        Flip(new List<GameObject> { card.gameObject });
        yield return new WaitUntil(() => flipb);
        CardMovement(card, Trash.transform);
        Trash.AddTrash(card);
        yield return new WaitUntil(() => !anim.isPlaying);
        synch = true;
    }
    public void DT2Hand(string pick, string type) { StartCoroutine(dt2Hand(pick, type)); }
    IEnumerator dt2Hand(string pick, string type)
    {
        synch = false;
        Transform tr = null;
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
        tr.GetComponent<SpriteRenderer>().sprite = manager.cardback;
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
        yield return new WaitUntil(() => selected || selectCansel);
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
        selected = false;
        selectCount = 1;
        selectCansel = false;
        selectName = new List<string>();
        manager.state = State.Select;
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
                    if (selected)
                    {
                        cutin.Set(CName, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        selected = false;
                        selectCount = 2;
                        selectName = new List<string>();
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                        yield return new WaitUntil(() => selected);
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
                        yield return new WaitUntil(() => selected);
                        Hand2Trash(selectName[0]);
                        yield return new WaitUntil(() => synch);
                        //サルベージする
                        selected = false;
                        selectCount = 1;
                        selectName = new List<string>();
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                        yield return new WaitUntil(() => selected);
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
                manager.state = State.Battle;
            }
            else if (type == CSkillType.BattleS)
            {
                if (CName == "sakuya")
                {
                    SelectInit();
                    SynchCheck();
                    yield return new WaitUntil(() => RPCsynch);
                    yield return new WaitUntil(() => selectCansel || selected);
                    if (selected)
                    {
                        cutin.Set(CName, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        for (int i = 0; i < 2; i++)
                        {
                            Flipura(new List<GameObject> { Field[i].GetChild(0).gameObject });
                            yield return new WaitUntil(() => flipb);
                            Handset(Field[i].GetChild(0));
                            yield return new WaitUntil(() => set);
                        }
                        //Setにすると普通のセットが動く。スタートボタンのメソッドを変更しておく
                        selected = false;
                        selectCount = 1;
                        selectName = new List<string>();
                        SynchCheck();
                        yield return new WaitUntil(() => RPCsynch);
                        yield return new WaitUntil(() => selected);
                        FieldCount = 0;
                        Flip(new List<GameObject> { Field[0].GetChild(0).gameObject, Field[1].GetChild(0).gameObject });
                        yield return new WaitUntil(() => flipb);
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
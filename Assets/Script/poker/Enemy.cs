using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*エネミー用のスクリプト
 * playerと違ってボタン操作がない
 * なので操作と思考が直結している。
 * 理想はcharacterごとに思考パターンがあること(完全ランダム・攻撃重視・追い詰められるとパターン変更など)
 */
public class Enemy : Character
{
    private bool[,] HEtable = new bool[23, 23];
    public bool synch_enemy { get; set; }
    public Player player;
    private bool[] Attacker = new bool[5]; //0:ハングドマン　1:暗闇　
    public bool Waiting { get; set; }

    private void Start()
    {
        for (int i = 0; i < 23; i++)
        {
            List<int> list = arcana2Card(num2arcana(i), "").hand;
            for (int j = 0; j < list.Count; j++) HEtable[i, list[j]] = true;
        }
    }
    
    /*手札からカードを場に出す。
     * 一番キャラクターの個性が出るところだと思う。
     */
    public void Hand2Field() { StartCoroutine("hand2Field"); }
    IEnumerator hand2Field()
    {
        if (FieldCount == 0)
        {

            if ((player.GetHEbuf(2221) || player.GetHEbuf(182) || (player.satoriF - manager.GetBattleTurn()) > 0)
                && !(GetHEbuf(2221) || GetHEbuf(182) || GetHEbuf(121) || (satoriF - manager.GetBattleTurn()) > 0))
            {
                Waiting = true;
                yield return new WaitUntil(() => !Waiting);
                Transform[] setter = notblindFieldSet();
                for (int i = 0; i < 2; i++)
                {
                    CardMovement(setter[i], Field[i]);
                    yield return new WaitUntil(() => !anim.isPlaying);
                    FieldCount++;
                }
                manager.EnemyWait=false;
            }
            else
            {
                Transform[] setter = CardSetThinking2();
                for (int i = 0; i < 2; i++)
                {
                    CardMovement(setter[i], Field[i]);
                    yield return new WaitUntil(() => !anim.isPlaying);
                    FieldCount++;
                    if (GetHEbuf(121) && i == 0)
                    {
                        Flip(new List<GameObject> { Field[i].GetChild(0).gameObject });
                        yield return new WaitUntil(() => flipb);
                    }
                }
            }
            yield break;
        }
        else if (FieldCount == 1)
        {
            Transform setter = CardSetThinking1();
            for(int i = 0; i < Field.Length; i++)
            {
                if (Field[i].childCount == 0)
                {
                    CardMovement(setter, Field[i]);
                    yield return new WaitUntil(() => !anim.isPlaying);
                    FieldCount++;
                    yield break;
                }
            }
        }
        /*for (int i = 0; i < Field.Length; i++)
        {
            if (Field[i].childCount == 0)
            {
                for (int j = 0; j < Hand.Length; j++)
                {
                    if (Hand[j].childCount != 0)
                    {
                        Transform card = Hand[j].GetChild(0);
                        if ((card.name.Contains("Fool") || card.name.Contains("World")) && manager.GetBattleTurn() < 5) continue;
                        //if (!(card.name.Contains("Judge") || card.name.Contains("Tower"))) continue;
                        CardMovement(card, Field[i]);
                        yield return new WaitUntil(() => !anim.isPlaying);
                        FieldCount++;
                        break;
                    }

                }
                
            }
        }*/
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
        yield return new WaitUntil(()=>!anim.isPlaying);
        for (int i = 0; i < list.Count; i++) Destroy(list[i].gameObject);
        synch = true;
    }
    /*手札からトラッシュへ
     * 引数で捨てるカードを指定。これ捨てるカードはこっちで決めたい感ある。
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
    protected override void SelectInit()
    {
        selected = false;
        selectCount = 1;
        selectCansel = false;
        selectName = new List<string>();
        manager.state = State.Select;
    }
    protected override IEnumerator marigan()
    {
        mariganb = false;
        yield return new WaitForSeconds(0.5f);
        mariganb = true;
    }
    protected override IEnumerator cSkillProcessE(string method)
    {
        if (!CSFlag)
        {
            if (graphic.sprite.name == "koisi")
            {
                if (koisiThink(method))
                {
                    cutin.Set(graphic.sprite.name, tag);
                    yield return new WaitUntil(() => !cutin.Playing);
                    koisiF = true;
                    CSFlag = true;
                }
            }
        }
        CSb = true;
        CSbk = true;
    }
    protected override IEnumerator cSKillProcess(CSkillType type)
    {
        CSb = false;
        if (!CSFlag)
        {
            if (type == CSkillType.TurnS)
            {
                if (graphic.sprite.name == "fran")
                {
                    if (player.HP <= 3 || manager.GetBattleTurn() >= 10)
                    {
                        cutin.Set(graphic.sprite.name, tag);
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
                    }
                }
                else if (graphic.sprite.name == "sanae")
                {
                    if (HP <= 7)
                    {
                        cutin.Set(graphic.sprite.name, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        HPChange(3);
                        CSFlag = true;
                    }
                }
                else if (graphic.sprite.name == "remiria")
                {
                    //暗闇の先の次のターンにしようが強い
                    if (remiriaThink())
                    {
                        cutin.Set(graphic.sprite.name, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        List<int> searchNum = new List<int>();
                        if (Attacker[1])
                        {
                            if (ListExist(Deck, 19))
                            {
                                searchNum.Add(19);
                                if (ListExist(Deck, 5)) searchNum.Add(5);
                                if (ListExist(Deck, 21)) searchNum.Add(21);
                                if (ListExist(Deck, 14)) searchNum.Add(14);
                            }
                            if (ListExist(Deck, 7))
                            {
                                searchNum.Add(7);
                                if (ListExist(Deck, 4)) searchNum.Add(4);
                                if (ListExist(Deck, 17)) searchNum.Add(17);
                                if (ListExist(Deck, 13)) searchNum.Add(13);
                            }
                            if (ListExist(Deck, 8))
                            {
                                searchNum.Add(8);
                                if (ListExist(Deck, 11)) searchNum.Add(11);
                                if (ListExist(Deck, 15)) searchNum.Add(15);
                            }
                            if (ListExist(Deck, 4))
                            {
                                searchNum.Add(4);
                                if (ListExist(Deck, 3)) searchNum.Add(3);
                            }
                        }
                        /*else if (player.Attacker[1] || player.Attacker[0])
                        {
                            if (ListExist(Deck, 3))
                            {
                                search.Add(3);
                                if (ListExist(Deck, 11)) search.Add(11);
                                if (ListExist(Deck, 3)) search.Add(3);
                            }
                            if (ListExist(Deck, 2))
                            {
                                search.Add(2);
                                if (ListExist(Deck, 17)) search.Add(17);
                                if (ListExist(Deck, 3)) search.Add(3);
                            }
                            if (ListExist(Deck, 20))
                            {
                                search.Add(20);
                                if (ListExist(Deck, 5)) search.Add(5);
                                if (ListExist(Deck, 19)) search.Add(19);
                                if (ListExist(Deck, 4)) search.Add(4);
                            }
                            if (ListExist(Deck, 10) || ListExist(Deck, 6))
                            {
                                search.Add(10);
                                search.Add(6);
                            }
                            if (ListExist(Deck, 18) || ListExist(Deck, 16))
                            {
                                search.Add(18);
                                search.Add(16);
                            }
                        }*/
                        if (searchNum.Count > 2)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                DT2Hand(num2arcana(searchNum[i]), "Deck");
                                yield return new WaitUntil(() => synch);
                            }
                        }
                        else
                        {
                            Deck = Deck.OrderBy(i => System.Guid.NewGuid()).ToList();
                            Draw(2);
                            yield return new WaitUntil(() => synch);
                        }
                        DeckShuffle();
                        yield return new WaitUntil(() => synch);
                        CSFlag = true;
                    }
                }
                else if (graphic.sprite.name == "satori")
                {
                    if (satoriThink())
                    {
                        cutin.Set(graphic.sprite.name, tag);
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
                    }
                }
                else if (graphic.sprite.name == "yuyuko")
                {
                    if (yuyukoThink())
                    {
                        cutin.Set(graphic.sprite.name, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        int pos = -1;
                        int temp = 100;
                        for (int i = 0; i < Hand.Length; i++)
                        {
                            if (Hand[i].childCount != 0)
                            {
                                Card card = arcana2Card(Hand[i].GetChild(0).name, "");
                                if (temp > card.Number)
                                {
                                    temp = card.Number;
                                    pos = i;
                                }
                            }
                        }
                        Hand2Trash(pos.ToString());
                        yield return new WaitUntil(() => synch);
                        List<string> list = Trash.GetListTrashCard();
                        int count = 0;
                        string tikara = "";
                        foreach (string item in list)
                        {
                            if (item.Contains("Devil") || item.Contains("Strength"))
                            {
                                tikara = item;
                                count++;
                            }
                        }
                        if (count != 1)
                        {
                            if (HP >player.HP)
                            {
                                foreach (string item in list)
                                {
                                    if (item.Contains("World") || item.Contains("Fool"))
                                    {
                                        DT2Hand(item, "Trash");
                                        yield return new WaitUntil(() => synch);
                                    }
                                }
                            }
                            else if (HP <= player.HP)
                            {
                                foreach (string item in list)
                                {
                                    if (item.Contains("World"))
                                    {
                                        DT2Hand(item, "Trash");
                                        yield return new WaitUntil(() => synch);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //devil or strength
                            DT2Hand(tikara, "Trash");
                            yield return new WaitUntil(() => synch);
                        }
                    }
                }
                manager.state = State.Set;
            }
            else if (type == CSkillType.DamegeF)
            {
                if (graphic.sprite.name == "reimu")
                {
                    if (battle.reimuThink())
                    {
                        cutin.Set(graphic.sprite.name, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        battle.CSreimu(tag);
                        CSFlag = true;
                    }
                }
                else if (graphic.sprite.name == "marisa")
                {
                    if (battle.marisaThink())
                    {
                        cutin.Set(graphic.sprite.name, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        battle.CSreimu(tag);
                        CSFlag = true;
                    }
                }
                manager.state = State.Battle;
            }
            else if (type == CSkillType.BattleF)
            {
                if (graphic.sprite.name == "youmu")
                {
                    if (battle.youmuThink())
                    {
                        cutin.Set(graphic.sprite.name, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        battle.CSyoumu(tag);
                        CSFlag = true;
                    }
                }
                manager.state = State.Battle;
            }
            else if (type == CSkillType.BattleS)
            {
                if (graphic.sprite.name == "sakuya")
                {
                    if (battle.sakuyaThink())
                    {
                        cutin.Set(graphic.sprite.name, tag);
                        yield return new WaitUntil(() => !cutin.Playing);
                        List<GameObject> backc = new List<GameObject>();
                        for (int i = 0; i < 2; i++) backc.Add(Field[i].GetChild(0).gameObject);
                        Flipura(backc);
                        yield return new WaitUntil(() => flipb);
                        for (int i = 0; i < 2; i++)
                        {
                            Handset(Field[i].GetChild(0));
                            yield return new WaitUntil(() => set);
                        }
                        Hand2Fields();
                        yield return new WaitUntil(() => synch_enemy);
                        List<GameObject> backch = new List<GameObject>();
                        for (int i = 0; i < 2; i++) backch.Add(Field[i].GetChild(0).gameObject);
                        Flip(backch);
                        CSFlag = true;
                    }
                }
                manager.state = State.Battle;
            }
        }
        CSb = true;
    }

    //think系
    //デビル、HE121,HE161は無視する
    private bool koisiThink(string method)
    {
        if (player.graphic.sprite.name == "satori")
        {
            if (method.Contains("cSkill")) return true;
        }
        if (method.Contains("HE182") || method.Contains("HE2221")) return true;
        else if (method.Contains("World") && (HP <= 7 || Random.value < 0.9f)) return true;
        else if (method.Contains("HE1411") && (HP - player.HP) > 10) return true;
        else if (method.Contains("HE149") || method.Contains("HE150"))
        {
            for (int i = 0; i < Hand.Length; i++)
            {
                if (Hand[i].childCount != 0)
                {
                    string n = Hand[i].GetChild(0).name;
                    if (n.Contains("World") || n.Contains("Fool")
                        || ((n.Contains("Sun") || n.Contains("Judgement")) && Random.value < 0.6f)) return true;
                }
            }
        }
        else if (method.Contains("HE136"))
        {
            for (int i = 0; i < 2; i++)
            {
                if (Field[i].GetChild(0).name.Contains("Lovers")) return false;
            }
            if (HP <= 3) return true;
        }
        else if (method.Contains("HE2110") && Random.value < 0.5f) return true;
        else if (method.Contains("HE186") && Random.value < 0.6f) return true;
        if (player.graphic.sprite.name == "fran")
        {
            if (method.Contains("cSkill") && HP <= 3) return true;
        }

        return false;
    }

    private bool satoriThink()
    {
        if (manager.GetBattleTurn() <= 5) return false;
        else if (manager.GetBattleTurn() == 9) return true;
        if (player.GetHEbuf(182) || player.GetHEbuf(186) || player.GetHEbuf(2221)) return false;
        for (int i = 0; i < Hand.Length; i++)
        {
            if (Hand[i].childCount != 0)
            {
                if (Hand[i].GetChild(0).name.Contains("World"))
                {
                    return true;
                }
            }
        }
        return false;
    }
    private bool remiriaThink()
    {
        if (Attacker[1]) return true;

        if (manager.GetBattleTurn() == 7) return true;
        return false;
    }
   
    private bool yuyukoThink()
    {
        List<string> list = Trash.GetListTrashCard();
        if (manager.GetBattleTurn() > 6)
        {
            int checker = 0;
            //捨てたくないカードたち
            for (int i = 0; i < Hand.Length; i++)
            {
                if (Hand[i].childCount != 0)
                {
                    if (Hand[i].GetChild(0).name.Contains("Devil")
                        || Hand[i].GetChild(0).name.Contains("Strength")
                        || Hand[i].GetChild(0).name.Contains("Fool")
                        || Hand[i].GetChild(0).name.Contains("World")) checker++;
                }
            }
            if (checker == 2) return false;
            int count = 0;
            foreach (string item in list)
            {
                if (item.Contains("Devil") || item.Contains("Strength")) count++;
            }
            if (count != 1)
            {
                if (HP > player.HP)
                {
                    foreach (string item in list)
                    {
                        if (item.Contains("World") || item.Contains("Fool")) return true;
                    }
                }
                else if (HP <=player.HP)
                {
                    foreach (string item in list)
                    {
                        if (item.Contains("World")) return true;
                    }
                }
            }
            else
            {
                //devil or strength
                if (manager.GetBattleTurn() == 10) return true;
                else return false;
            }
        }
        return false;
    }
    public Transform[] notblindFieldSet()
    {
        Transform[] setter = new Transform[2];//場に出すtransform
        int[] table = new int[23];//手札のいちとそこのカードの番号の対応表
        List<Card> cardlist = new List<Card>(); //出せるカードの一覧
        int[] same = { -1, -1 };
        for (int i = 0; i < Hand.Length; i++)
        {
            if (Hand[i].childCount != 0)
            {
                Card card = arcana2Card(Hand[i].GetChild(0).name, "");
                cardlist.Add(card);
                if (table[card.Number] != 0)
                {
                    same[0] = i;
                    same[1] = card.Number;
                }
                else
                {
                    table[card.Number] = i;
                }
            }
        }
        Card[] arrayP = new Card[2];
        bool worldf = table[21] != 0;
        for (int i = 0; i < 2; i++) arrayP[i] = arcana2Card(player.GetFieldCard(i), player.tag);
        int num1p = arrayP[0].Number > arrayP[1].Number ? arrayP[0].Number : arrayP[1].Number;
        int num2p = arrayP[0].Number > arrayP[1].Number ? arrayP[1].Number : arrayP[0].Number;
        if (worldf && !((num1p == 22 && num2p != 15) || battle.youmuflag() || (num1p == 20 && num2p == 5) || (num1p == 18 && num2p == 16) ||
            (num1p == 17 && num1p == 2) || (num1p == 3 && num2p == 2) || (num1p == 10 && num2p == 6)) || (num1p == 14 && num2p == 9))
        {
            if (table[10] != 0)
            {
                setter[0] = Hand[table[21]].GetChild(0);
                setter[1] = Hand[table[10]].GetChild(0);
            }
            else if (table[5] != 0)
            {
                setter[0] = Hand[table[21]].GetChild(0);
                setter[1] = Hand[table[5]].GetChild(0);
            }
            else
            {
                for (int i = 0; i < 23; i++)
                {
                    if (table[i] != 0)
                    {
                        setter[0] = Hand[table[21]].GetChild(0);
                        setter[1] = Hand[table[i]].GetChild(0);
                    }
                }
            }
            return setter;
        }
        bool fieldstate = !((num1p == 22 && num2p != 15) || battle.youmuflag() || (num1p == 20 && num2p == 5) || (num1p == 18 && num2p == 16) ||
            (num1p == 17 && num1p == 2) || (num1p == 3 && num2p == 2) || (num1p == 10 && num2p == 6)) || (num1p == 14 && num2p == 9);
        /*
         * 思考プロセス
         * 役ができるなら出す。ただし、8+15の役はできるだけ出さない
         * 21と０は同時には出さない。
         */
        for (int i = 0; i < cardlist.Count - 1; i++)
        {
            if ((cardlist[i].Number == 22 || cardlist[i].Number == 21) && manager.GetBattleTurn() < 5) continue;
            for (int j = i + 1; j < cardlist.Count; j++)
            {
                if ((cardlist[j].Number == 22 || cardlist[j].Number == 21) && manager.GetBattleTurn() < 5) continue;
                if (HEtable[cardlist[i].Number, cardlist[j].Number])
                {
                    int num1 = cardlist[i].Number < cardlist[j].Number ? cardlist[i].Number : cardlist[j].Number;
                    int num2 = cardlist[i].Number < cardlist[j].Number ? cardlist[j].Number : cardlist[i].Number;
                    if ((num1 == 21 && num2 == 22) ||
                    (num1 == 8 && num2 == 15 && manager.GetBattleTurn() < 2) ||
                    (num1 == 11 && num2 == 14 && player.HP < HP) ||
                    (num1 == 9 && num2 == 11 && manager.GetBattleTurn() < 2) ||
                    (!fieldstate && (num1 > 17 || num2 > 17))) continue;
                    setter[0] = Hand[table[num1]].GetChild(0);
                    setter[1] = Hand[table[num2]].GetChild(0);
                    return setter;
                }
            }
        }
        /*役がないのでランダムに出す。0と21は同時には出さない
         * 同じカードは出さない
         */
        cardlist = cardlist.OrderBy(i => System.Guid.NewGuid()).ToList();
        for (int i = 0; i < cardlist.Count - 1; i++)
        {
            if ((cardlist[i].Number == 22 || cardlist[i].Number == 21) && manager.GetBattleTurn() < 5) continue;
            for (int j = i + 1; j < cardlist.Count; j++)
            {
                if ((cardlist[j].Number == 22 || cardlist[j].Number == 21) && manager.GetBattleTurn() < 5) continue;
                int num1 = cardlist[i].Number < cardlist[j].Number ? cardlist[i].Number : cardlist[j].Number;
                int num2 = cardlist[i].Number < cardlist[j].Number ? cardlist[j].Number : cardlist[i].Number;
                if ((num1 == 21 && num2 == 22) || (num1 == num2) ||
                    (num1 == 8 && num2 == 15 && manager.GetBattleTurn() < 2) ||
                    (num1 == 11 && num2 == 14 && player.HP < HP) ||
                    (num1 == 9 && num2 == 11 && manager.GetBattleTurn() < 2) ||
                    (!fieldstate && (num1 > 17 || num2 > 17))) continue;
                setter[0] = Hand[table[num1]].GetChild(0);
                setter[1] = Hand[table[num2]].GetChild(0);
                return setter;
            }
        }
        int fieldset = 0;
        for (int i = 0; i < table.Length; i++)
        {
            if (table[i] != 0)
            {
                setter[fieldset] = Hand[table[i]].GetChild(0);
                fieldset++;
                if (fieldset == 2) return setter;
            }

        }
        return setter;
    }
    /*1まいのカードを出す思考
     */
    private Transform CardSetThinking1()
    {
        Transform setter = Hand[0];//場に出すカード
        int[] table = new int[23];//手札のいちとそこのカードの番号の対応表
        List<Card> cardlist = new List<Card>(); //出せるカードの一覧
        Card Fcard = new Death("");//場に出ているカード

        for (int i = 0; i < 2; i++)
        {
            if (Field[i].childCount != 0)
            {
                Fcard = arcana2Card(Field[i].GetChild(0).name, "");
                break;
            }
        }
        for (int i = 0; i < Hand.Length; i++)
        {
            if (Hand[i].childCount != 0)
            {
                Card card = arcana2Card(Hand[i].GetChild(0).name, "");
                cardlist.Add(card);
                table[card.Number] = i;
            }
        }
        for (int i = 0; i < cardlist.Count - 1; i++)
        {
            if ((cardlist[i].Number == 22 || cardlist[i].Number == 21) && manager.GetBattleTurn() < 5) continue;
            if (HEtable[cardlist[i].Number, Fcard.Number])
            {
                int num1 = cardlist[i].Number < Fcard.Number ? cardlist[i].Number : Fcard.Number;
                int num2 = cardlist[i].Number < Fcard.Number ? Fcard.Number : cardlist[i].Number;
                if ((num1 == 21 && num2 == 22) ||
                    (num1 == 8 && num2 == 15 && manager.GetBattleTurn() < 2) ||
                    (num1 == 11 && num2 == 14 && player.HP < HP) ||
                    (num1 == 9 && num2 == 11 && manager.GetBattleTurn() < 2) ||
                    ((num1 == 22 || num2 == 22) && Attacker[1])) continue;
                setter = Hand[table[cardlist[i].Number]].GetChild(0);
                return setter;
            }
        }
        cardlist = cardlist.OrderBy(i => System.Guid.NewGuid()).ToList();
        for (int i = 0; i < cardlist.Count - 1; i++)
        {
            if ((cardlist[i].Number == 22 || cardlist[i].Number == 21) && manager.GetBattleTurn() < 5) continue;
            int num1 = cardlist[i].Number < Fcard.Number ? cardlist[i].Number : Fcard.Number;
            int num2 = cardlist[i].Number < Fcard.Number ? Fcard.Number : cardlist[i].Number;
            if ((num1 == 21 && num2 == 22) || (num1 == 8 && num2 == 15 && manager.GetBattleTurn() < 2) ||
                    (num1 == 11 && num2 == 14 && player.HP < HP) ||
                    (num1 == 9 && num2 == 11 && manager.GetBattleTurn() < 2) ||
                    ((num1 == 22 || num2 == 22) && Attacker[1])) continue;
            setter = Hand[table[cardlist[i].Number]].GetChild(0);
            return setter;
        }
        setter = Hand[table[cardlist[0].Number]].GetChild(0);
        return setter;
    }
    /*
     * 2枚のカードを出す場合の思考
     */
    private Transform[] CardSetThinking2()
    {
        Transform[] setter = new Transform[2];//場に出すtransform
        int[] table = new int[23];//手札のいちとそこのカードの番号の対応表
        List<Card> cardlist = new List<Card>(); //出せるカードの一覧
        int[] same = { -1, -1 };
        for (int i = 0; i < Hand.Length; i++)
        {
            if (Hand[i].childCount != 0)
            {
                Card card = arcana2Card(Hand[i].GetChild(0).name, "");
                cardlist.Add(card);
                if (table[card.Number] != 0)
                {
                    same[0] = i;
                    same[1] = card.Number;
                }
                else
                {
                    table[card.Number] = i;
                }
            }
        }
        /*
         * 思考プロセス
         * 役ができるなら出す。ただし、8+15の役はできるだけ出さない
         * 21と０は同時には出さない。
         */
        for (int i = 0; i < cardlist.Count - 1; i++)
        {
            if ((cardlist[i].Number == 22 || cardlist[i].Number == 21) && manager.GetBattleTurn() < 5) continue;
            for (int j = i + 1; j < cardlist.Count; j++)
            {
                if ((cardlist[j].Number == 22 || cardlist[j].Number == 21) && manager.GetBattleTurn() < 5) continue;
                if (HEtable[cardlist[i].Number, cardlist[j].Number])
                {
                    int num1 = cardlist[i].Number < cardlist[j].Number ? cardlist[i].Number : cardlist[j].Number;
                    int num2 = cardlist[i].Number < cardlist[j].Number ? cardlist[j].Number : cardlist[i].Number;
                    if ((num1 == 21 && num2 == 22) ||
                    (num1 == 8 && num2 == 15 && manager.GetBattleTurn() < 2) ||
                    (num1 == 11 && num2 == 14 && player.HP < HP) ||
                    (num1 == 9 && num2 == 11 && manager.GetBattleTurn() < 2) ||
                    (num1==10&&num2==8&&manager.GetBattleTurn()!=1)||
                    ((num1 == 22 || num2 == 22) && Attacker[1])) continue;
                    setter[0] = Hand[table[num1]].GetChild(0);
                    setter[1] = Hand[table[num2]].GetChild(0);
                    return setter;
                }
            }
        }
        /*役がないのでランダムに出す。0と21は同時には出さない
         * 同じカードは出さない
         */
        cardlist = cardlist.OrderBy(i => System.Guid.NewGuid()).ToList();
        for (int i = 0; i < cardlist.Count - 1; i++)
        {
            if ((cardlist[i].Number == 22 || cardlist[i].Number == 21) && manager.GetBattleTurn() < 5) continue;
            for (int j = i + 1; j < cardlist.Count; j++)
            {
                if ((cardlist[j].Number == 22 || cardlist[j].Number == 21) && manager.GetBattleTurn() < 5) continue;
                int num1 = cardlist[i].Number < cardlist[j].Number ? cardlist[i].Number : cardlist[j].Number;
                int num2 = cardlist[i].Number < cardlist[j].Number ? cardlist[j].Number : cardlist[i].Number;
                if ((num1 == 21 && num2 == 22) || (num1 == num2) ||
                    (num1 == 8 && num2 == 15 && manager.GetBattleTurn() < 2) ||
                    (num1 == 11 && num2 == 14 && player.HP < HP) ||
                    (num1 == 9 && num2 == 11 && manager.GetBattleTurn() < 2) ||
                    ((num1 == 22 || num2 == 22) && Attacker[1])) continue;
                setter[0] = Hand[table[num1]].GetChild(0);
                setter[1] = Hand[table[num2]].GetChild(0);
                return setter;
            }
        }
        /*出したくないものしかない場合、出すしかないよなあ
         */
        if (cardlist[0] == cardlist[1] && cardlist.Count <= 2)
        {
            setter[0] = Hand[table[cardlist[0].Number]].GetChild(0);
            setter[1] = Hand[same[0]].GetChild(0);
        }
        else if (cardlist.Count == 4)
        {
            //ここに来るのは今の所002121の時のみなので、21,21を出す。
            int temp = 0;
            for (int i = 0; i < Hand.Length; i++)
            {
                if (Hand[i].childCount != 0)
                {
                    if (Hand[i].GetChild(0).name.Contains("World"))
                    {
                        setter[temp] = Hand[i].GetChild(0);
                        temp++;
                    }
                }
            }
            if (temp == 2) return setter;
            else
            {
                setter[0] = Hand[table[cardlist[0].Number]].GetChild(0);
                setter[1] = Hand[same[0]].GetChild(0);
            }
        }

        return setter;
    }
    
    //昨夜の効果で出す
    public void Hand2Fields() { StartCoroutine("hand2Fields"); }
    IEnumerator hand2Fields()
    {
        synch_enemy = false;
        Transform[] setter = notblindFieldSet();
        for (int i = 0; i < 2; i++)
        {
            CardMovement(setter[i], Field[i]);
            yield return new WaitUntil(() => !anim.isPlaying);
            FieldCount++;
        }
        synch_enemy = true;

    }
}

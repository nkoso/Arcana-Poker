using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

public class BattleTurn : MonoBehaviour
{
    CardManager manager;
    [SerializeField] Player player;
    [SerializeField] Enemy enemy;
    public ParticleSystem expload; //攻守エフェクト
    
    private GameObject[] adtext = new GameObject[4];
    private int[] advalue = new int[4];
    public GameObject UIPanel;
    private Card[] cardlist=new Card[4]; //0,1 : player 2,3 :enemy
    private bool[,] HEtable = new bool[23,23];
    private bool hand_processed = false;
    private bool hand_effect;
    private bool[] HEsetterP = new bool[15];
    private int[] HETurn2P = new int[5];
    private bool[] HEsetterE = new bool[15];
    private int[] HETurn2E = new int[5];
    public Text PHtext, EHtext; //役のテキスト

    private bool effect_processed = false;
    private GameObject effect;
    private int[] hanged=new int[2];
    private int[] AnchDef = new int[2]; //守備力減少
    private bool[] FoolFlag = new bool[2];
    private int[] APlus = new int[2];
    private int[] DPlus = new int[2];
    private int[] HCount = new int[2];
    private bool[] IgnoreD = new bool[2];
    private int counterCoef = 0;
    int p_dmg;
    int e_dmg;
    private List<string> selectName = new List<string>();
    private int selectCount = 0;
    private bool selected;
    private bool selectCansel=false;
    private bool[] youmu = new bool[2];
    private bool fieldreset;
    private Transform fool_obj;
    private int[] CaE_buffer = new int[4];
    private int[] HE_buffer = new int[2];
    
    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(CurrentTimeSeed());
        manager = GetComponent<CardManager>();
        for (int i = 0; i < 23; i++)
        {
            List<int> list = arcana2Card(num2arcana(i), "").hand;
            for(int j = 0; j < list.Count; j++) HEtable[i, list[j]] = true;
        }
        for (int i = 0; i < 2; i++) HCount[i] = 0;
        
    }
    public void BattleStart() { StartCoroutine("battleStart"); }
    IEnumerator battleStart()
    {
        //妖夢
        player.CSkillProcess(CSkillType.BattleF);
        enemy.CSkillProcess(CSkillType.BattleF);
        yield return new WaitUntil(() => player.CSb && enemy.CSb);

        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < 2; i++)list.Add(enemy.Field[i].GetChild(0).gameObject);
        enemy.Flip(list);
        yield return new WaitUntil(() => enemy.flipb);
        if (player.GetHEbuf(121)) player.SetHEbuf(121, false);
        if (enemy.GetHEbuf(121)) enemy.SetHEbuf(121, false);
        //咲夜
        player.CSkillProcess(CSkillType.BattleS);
        enemy.CSkillProcess(CSkillType.BattleS);
        yield return new WaitUntil(() => player.CSb && enemy.CSb);
        manager.state = State.Battle;
        StartCoroutine("Battle");
    }
    IEnumerator Battle()
    {
        for (int i = 0; i < 4; i++) advalue[i] = 0;
        p_dmg = 0;
        e_dmg = 0;
        fieldreset = false;
        for (int i = 0; i < 2; i++) cardlist[i] = arcana2Card(player.GetFieldCard(i), player.tag);
        for (int i = 0; i < 2; i++) cardlist[i + 2] = arcana2Card(enemy.GetFieldCard(i), enemy.tag);

        if (manager.level == Level.hard||manager.level==Level.expert)
        {
            //ラグナロク
            if (player.GetHEbuf(1312))
            {
                for (int i = 0; i < 2; i++)
                {
                    if (cardlist[i].Number < 10 && cardlist[i].Number > 0)
                    {
                        cardlist[i].Attack+=1;
                        cardlist[i].Defense += 1;
                    }
                }
            }
            if (enemy.GetHEbuf(1312))
            {
                for (int i = 2; i < 4; i++)
                {
                    if (cardlist[i].Number < 10 && cardlist[i].Number>0)
                    {
                        cardlist[i].Attack += 1;
                        cardlist[i].Defense += 1;
                    }
                }
            }
            //役の処理
            HEordering();
            for (int i = 0; i < 2; i++)
            {
                if (HE_buffer[i] == 0)
                {
                    if (!youmu[1])
                    {
                        int num1 = cardlist[0].Number > cardlist[1].Number ? cardlist[0].Number : cardlist[1].Number;
                        int num2 = cardlist[0].Number > cardlist[1].Number ? cardlist[1].Number : cardlist[0].Number;
                        if (HEtable[num1, num2]) HEmanager(num1, num2, player.tag);
                    }
                }
                else if (HE_buffer[i] == 1)
                {
                    if (!youmu[0])
                    {
                        int num1 = cardlist[2].Number > cardlist[3].Number ? cardlist[2].Number : cardlist[3].Number;
                        int num2 = cardlist[2].Number > cardlist[3].Number ? cardlist[3].Number : cardlist[2].Number;
                        if (HEtable[num1, num2]) HEmanager(num1, num2, enemy.tag);
                    }
                }
                else if (HE_buffer[i] == -1) break;
                yield return new WaitUntil(() => !hand_processed);
                if (fieldreset)
                {
                    StartCoroutine("Battle");
                    yield break;
                }
            }
            //効果処理終了
            if (player.HP <= 0 || enemy.HP <= 0)
            {
                manager.GameEnd();
                yield break;
            }
        }

        //カード効果処理
        if (youmu[1]) for (int i = 0; i < 2; i++) cardlist[i].Effect = false;
        if (youmu[0]) for (int i = 2; i < 4; i++) cardlist[i].Effect = false;
        CaEordering();
        for (int i = 0; i < 4; i++)
        {
            if (CaE_buffer[i] != -1 && cardlist[CaE_buffer[i]].Effect)
            {
                effector(CaE_buffer[i]);
                Cardeffect(cardlist[CaE_buffer[i]].Number, cardlist[CaE_buffer[i]].tag);
                yield return new WaitUntil(() => !effect_processed);
                yield return new WaitForSeconds(1.0f);
                Destroy(effect);
            }
        }
        //効果処理終了
        if (player.HP<=0 || enemy.HP <= 0)
        {
            manager.GameEnd();
            yield break;
        }
        //破天荒なる教皇
        if((bool)HEgetter(195, "Player"))
        {
            DPlus[1] = 0;
            enemy.AD = false;
            HEsetterE[2] = false;
            for (int i = 2; i < 4; i++)
            {
                Card def = arcana2Card(num2arcana(cardlist[i].Number), "");
                cardlist[i].Defense = def.Defense;
            }
        }
        if((bool)HEgetter(195, "Enemy"))
        {
            DPlus[0] = 0;
            player.AD = false;
            HEsetterP[2] = false;
            for (int i = 0; i < 2; i++)
            {
                Card def = arcana2Card(num2arcana(cardlist[i].Number), "");
                cardlist[i].Defense = def.Defense;
            }
        }
        //ダメージ処理
        //カード攻守計算
        for(int i = 0; i < 2; i++)
        {
            advalue[0] += cardlist[i].Attack;
            advalue[1] += cardlist[i].Defense;
            advalue[2] += cardlist[i + 2].Attack;
            advalue[3] += cardlist[i + 2].Defense;
        }
        adtextSet();
        yield return new WaitForSeconds(0.7f);
        //暗闇の先にあるもの
        if ((int)HEgetter(1512, "Player") == 2)
        {
            advalue[0] *= 2;
            advalue[1] *= 2;
        }
        if ((int)HEgetter(1512, "Enemy") == 2)
        {
            advalue[2] *= 2;
            advalue[3] *= 2;
        }
        p_dmg = (advalue[1] > advalue[2]) ? 0 : advalue[1] - advalue[2];
        e_dmg = (advalue[3] > advalue[0]) ? 0 : advalue[3] - advalue[0];
        //霊夢と魔理沙
        player.CSkillProcess(CSkillType.DamegeF);
        enemy.CSkillProcess(CSkillType.DamegeF);
        yield return new WaitUntil(() => player.CSb && enemy.CSb);
        manager.state = State.Battle;
        advalue[0] += APlus[0];
        advalue[1] += DPlus[0];
        advalue[2] += APlus[1];
        advalue[3] += DPlus[1];

        //sun　効果処理 数値減少系
        if (AnchDef[0] > 0) advalue[3] = (advalue[3] > AnchDef[0]) ? advalue[3] - AnchDef[0] : 0;
        if (AnchDef[1] > 0) advalue[1] = (advalue[1] > AnchDef[1]) ? advalue[1] - AnchDef[1] : 0;

        adtextTe();
        if (AnchDef[0]>0 || AnchDef[1]>0) yield return new WaitForSeconds(0.6f);
        for (int i = 0; i < 4; i++) adtext[i].GetComponent<adtextPop>().Play();
        yield return new WaitUntil(() => adtext[0].GetComponent<adtextPop>().synch&& adtext[1].GetComponent<adtextPop>().synch&& adtext[2].GetComponent<adtextPop>().synch&& adtext[3].GetComponent<adtextPop>().synch);
        expload.Play();

        //ダメージ計算時
        //防御無視系統
        advalue[3] = IgnoreD[0] ? 0 : advalue[3];
        advalue[1] = IgnoreD[1] ? 0 : advalue[1];

        p_dmg = (advalue[1] > advalue[2]) ? 0 : advalue[1] - advalue[2];
        e_dmg = (advalue[3] > advalue[0]) ? 0 : advalue[3] - advalue[0];

        //Hanged Man 処理
        if (p_dmg < 0 && hanged[0] == 1) p_dmg -= 1;
        if (p_dmg < 0 && hanged[1] == 2) p_dmg -= 2;
        if (e_dmg < 0 && hanged[1] == 1) e_dmg -= 1;
        if (e_dmg < 0 && hanged[0] == 2) e_dmg -= 2;

        
        yield return new WaitForSeconds(0.5f);
        //義憤の女帝 カウンター系統
        if ((bool)HEgetter(113, "Player") && (bool)HEgetter(113, "Enemy"))
        {
            int temp = e_dmg;
            e_dmg += p_dmg < 0 ? counterCoef * p_dmg : 0;
            p_dmg += temp < 0 ? counterCoef * temp : 0;
        }
        else if ((bool)HEgetter(113, "Player"))
        {
            e_dmg += p_dmg < 0 ? counterCoef * p_dmg : 0;
            p_dmg = 0;
        }
        else if ((bool)HEgetter(113, "Enemy"))
        {
            p_dmg += e_dmg < 0 ? counterCoef * e_dmg : 0;
            e_dmg = 0;
        }

        player.HPChange(p_dmg);
        enemy.HPChange(e_dmg);
        //王へ至る道 与ダメージ回復
        if ((bool)HEgetter(74, "Player")) player.HPChange(-e_dmg / 2);
        if ((bool)HEgetter(74, "Enemy")) enemy.HPChange(-p_dmg / 2);
        //怒涛なる戦車　被ダメージ回復
        if ((bool)HEgetter(177, "Player")) player.HPChange(-p_dmg / 2);
        if ((bool)HEgetter(177, "Enemy")) enemy.HPChange(-e_dmg / 2);
        StartCoroutine(TurnEnd());
    }
    private bool foolsynch = false;
    IEnumerator FoolE(string type)
    {
        foolsynch = false;
        bool flg = type == "Player";
        manager.state = State.Select;
        if (flg)
        {
            SelectSetting(1);
            if ((bool)HEgetter(2212, "Player")) SelPanelSet(enemy.tag, "Trash");
            FSelSet();
            CanselSet();
            yield return new WaitUntil(() => selectCansel || selected);
            player.PlayerUIReset();
            if (selected)
            {
                if (selectName[0] == "0") fool_obj = enemy.Field[0].transform.GetChild(0);
                else if (selectName[0] == "1") fool_obj = enemy.Field[1].transform.GetChild(0);
                else
                {
                    GameObject obj = new GameObject(selectName[0]);
                    obj.tag = player.tag;
                    obj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    obj.AddComponent<SpriteRenderer>().sprite = manager.arcana[arcana2num(selectName[0])];
                    obj.transform.position = enemy.Trash.transform.position;
                    enemy.Trash.Salvage(selectName[0]);
                    fool_obj = obj.transform;
                }
                SelectSetting(1);
                HSelSet(player.tag);

                yield return new WaitUntil(() => selected);
                manager.state = State.Battle;
                player.PlayerUIReset();
                player.Hand2Trash(selectName[0]);
                yield return new WaitUntil(() => player.synch);
                player.Handset(fool_obj);
                yield return new WaitUntil(() => player.set);
            }
            FoolFlag[0] = false;
        }
        else
        {
            int number = -1;
            Transform obj = null;
            for (int i = 0; i < 2; i++)
            {
                if (isArie(cardlist[i]))
                {
                    if (cardlist[i].Number > number)
                    {
                        number = cardlist[i].Number;
                        obj = player.Field[i].transform.GetChild(0);
                    }
                }
            }
            if (number != -1)
            {
                int pos = -1;
                int temp = 100;
                for (int j = 0; j < enemy.Hand.Length; j++)
                {
                    if (enemy.Hand[j].transform.childCount != 0)
                    {
                        string name = enemy.Hand[j].GetChild(0).name;
                        if (arcana2num(name) < number)
                        {
                            if (temp > arcana2num(name))
                            {
                                temp = arcana2num(name);
                                pos = j;
                            }
                        }
                    }

                }
                if (pos != -1)
                {
                    obj.GetComponent<SpriteRenderer>().sprite = manager.cardback;
                    enemy.Hand2Trash(pos.ToString());
                    yield return new WaitUntil(() => enemy.synch);
                    enemy.Handset(obj);
                    yield return new WaitUntil(() => enemy.set);
                }

            }
            FoolFlag[1] = false;
        }
        manager.state = State.Battle;
        foolsynch = true;
    }
    IEnumerator TurnEnd()
    {
        //終了処理
        //Foolの処理
        if (FoolFlag[0] && player.HandCardNum()>0&& FoolFlag[1] && enemy.HandCardNum() > 0)
        {
            if (Random.value <0.5f)
            {
                StartCoroutine(FoolE("Player"));
                yield return new WaitUntil(() => foolsynch);
                StartCoroutine(FoolE("Enemy"));
                yield return new WaitUntil(() => foolsynch);
            }
            else
            {
                StartCoroutine(FoolE("Enemy"));
                yield return new WaitUntil(() => foolsynch);
                StartCoroutine(FoolE("Player"));
                yield return new WaitUntil(() => foolsynch);
            }
        }
        else if (FoolFlag[0] && player.HandCardNum() > 0)
        {
            StartCoroutine(FoolE("Player"));
            yield return new WaitUntil(() => foolsynch);
        }
        else if (FoolFlag[1] &&enemy.HandCardNum()>0)
        {
            StartCoroutine(FoolE("Enemy"));
            yield return new WaitUntil(() => foolsynch);
        }
        //崩壊の足音
        if ((int)HEgetter(1613, "Player") > 0) enemy.HPChange(-2);
        if ((int)HEgetter(1613, "Enemy") > 0) player.HPChange(-2);

        for (int i = 0; i < 2; i++)
        {
            if (hanged[i] != 0) hanged[i] = (hanged[i] + 1) % 3;
            AnchDef[i] = 0;
            DPlus[i] = 0;
            APlus[i] = 0;
            IgnoreD[i] = false;
        }
        //欺瞞の女教皇
        if ((int)HEgetter(182, "Player") == 2)
        {
            List<GameObject> list = new List<GameObject>();
            enemy.SetHEbuf(182, false);
            for(int i = 0; i < enemy.Hand.Length; i++)
            {
                if (enemy.Hand[i].childCount != 0) list.Add(enemy.Hand[i].GetChild(0).gameObject);
            }
            enemy.Flipura(list);
            yield return new WaitUntil(() => enemy.flipb);
        }
        if ((int)HEgetter(182, "Enemy") == 2)
        {
            player.SetHEbuf(182, false);
        }
        for (int i = 0; i < HETurn2P.Length; i++)
        {
            if (HETurn2P[i] != 0) HETurn2P[i] = (HETurn2P[i] + 1) % 3;
            if (HETurn2E[i] != 0) HETurn2E[i] = (HETurn2E[i] + 1) % 3;
        }
        for (int i = 0; i < HEsetterP.Length; i++)
        {
            HEsetterP[i] = false;
            HEsetterE[i] = false;
        }
        if (!HEtable[6, 10])
        {
            HEtable[6, 10] = true;
            HEtable[10, 6] = true;
        }
        if(!HEtable[18, 16])
        {
            HEtable[18, 16] = true;
            HEtable[16, 18] = true;
        }
        if (!HEtable[4, 1])
        {
            HEtable[4, 1] = true;
            HEtable[1, 4] = true;
        }
        if (!HEtable[20, 4])
        {
            HEtable[20, 4] = true;
            HEtable[4, 20] = true;
        }
        if (!HEtable[10, 3])
        {
            HEtable[10, 3] = true;
            HEtable[3, 10] = true;
        }
        player.AD = false;
        enemy.AD = false;
        manager.TurnEnd();
    }
    //効果の順番決め
    private int orderSet(int[] array, int order, int value)
    {
        array[order] = value;
        return ++order;
    }
    private void HEordering()
    {
        for (int i = 0; i < HE_buffer.Length; i++) HE_buffer[i] = -1;
        int order = 0;
        int num0 = cardlist[0].Number > cardlist[1].Number ? cardlist[0].Number : cardlist[1].Number;
        int num1 = cardlist[0].Number > cardlist[1].Number ? cardlist[1].Number : cardlist[0].Number;
        int num2 = cardlist[2].Number > cardlist[3].Number ? cardlist[2].Number : cardlist[3].Number;
        int num3 = cardlist[2].Number > cardlist[3].Number ? cardlist[3].Number : cardlist[2].Number;
        //先手必勝
        if (num0 == 10 && num1 == 8 && manager.GetBattleTurn() == 0) order = orderSet(HE_buffer, order, 0);
        if (num2 == 10 && num3 == 8 && manager.GetBattleTurn() == 0) order = orderSet(HE_buffer, order, 1);
        //鮮烈なる力 20枚
        if (num0 == 15 && num1 == 8 && player.Trash.GetListTrashCard().Count >= 20) order = orderSet(HE_buffer, order, 0);
        if (num2 == 15 && num3 == 8 && enemy.Trash.GetListTrashCard().Count >= 20) order = orderSet(HE_buffer, order, 0);
        //世界の真理
        if (num0 == 21 && num1 == 5) order = orderSet(HE_buffer, order, 0);
        if (num2 == 21 && num3 == 5) order = orderSet(HE_buffer, order, 1);
        //神の審判
        if (num0 == 20 && num1 == 16) order = orderSet(HE_buffer, order, 0);
        if (num2 == 20 && num3 == 16) order = orderSet(HE_buffer, order, 1);
        //法王の裁き
        if (num0 == 20 && num1 == 5) order = orderSet(HE_buffer, order, 0);
        if (num2 == 20 && num3 == 5) order = orderSet(HE_buffer, order, 1);
        //断罪の皇帝
        if (num0 == 20 && num1 == 4) order = orderSet(HE_buffer, order, 0);
        if (num2 == 20 && num3 == 4) order = orderSet(HE_buffer, order, 1);
        //狂乱の坩堝
        if (num0 == 18 && num1 == 16) order = orderSet(HE_buffer, order, 0);
        if (num2 == 18 && num3 == 16) order = orderSet(HE_buffer, order, 1);
        //恋人は惹かれ合う
        if (num0 == 10 && num1 == 6) order = orderSet(HE_buffer, order, 0);
        if (num2 == 10 && num3 == 6) order = orderSet(HE_buffer, order, 1);
        //傾国の美女
        if (num0 == 10 && num1 == 3) order = orderSet(HE_buffer, order, 0);
        if (num2 == 10 && num3 == 3) order = orderSet(HE_buffer, order, 1);
        //チャンスメイキング
        if (num0 == 4 && num1 == 1) order = orderSet(HE_buffer, order, 0);
        if (num2 == 4 && num3 == 1) order = orderSet(HE_buffer, order, 1);
        //胎動する世界
        if (num0 == 21 && num1 == 10) order = orderSet(HE_buffer, order, 0);
        if (num2 == 21 && num3 == 10) order = orderSet(HE_buffer, order, 1);
        //混沌導く奇術師
        if (num0 == 22 && num1 == 15) order = orderSet(HE_buffer, order, 0);
        if (num2 == 22 && num3 == 15) order = orderSet(HE_buffer, order, 1);
        //止水の隠者
        if (num0 == 14 && num1 == 9) order = orderSet(HE_buffer, order, 0);
        if (num2 == 14 && num3 == 9) order = orderSet(HE_buffer, order, 1);
        //聖女の抱擁　星の導き 正義の法　導き手
        if ((num0 == 3 && num1 == 2) || (num0 == 17 && num1 == 2) || (num0 == 9 && num1 == 6) || (num0 == 11 && num1 == 5)) order = orderSet(HE_buffer, order, 0);
        if ((num2 == 3 && num3 == 2) || (num2 == 17 && num3 == 2) || (num2 == 9 && num3 == 6) || (num2 == 11 && num3 == 5)) order = orderSet(HE_buffer, order, 1);
        //雨除けの巫女
        if (num0 == 19 && num1 == 2) order = orderSet(HE_buffer, order, 0);
        if (num2 == 19 && num3 == 2) order = orderSet(HE_buffer, order, 1);
        int[] randomarr = { 0, 1 };
        randomarr = randomarr.OrderBy(i => System.Guid.NewGuid()).ToArray();
        for (int i = 0; i < 2; i++)
        {
            if (!ListExist(HE_buffer, randomarr[i]))
            {
                if ((randomarr[i] == 0 && HEtable[num0, num1] && !(num0 == 16 && num1 == 1))
                    || (randomarr[i] == 1 && HEtable[num2, num3] && !(num2 == 16 && num3 == 1)))
                {
                    order = orderSet(HE_buffer, order, randomarr[i]);
                }
            }
        }
        
        //禍殃なる魔術師
        if (num0 == 16 && num1 == 1) order = orderSet(HE_buffer, order, 0);
        if (num2 == 16 && num3 == 1) order = orderSet(HE_buffer, order, 1);
    }
    private void CaEordering()
    {
        for (int i = 0; i < CaE_buffer.Length; i++) CaE_buffer[i] = -1;
        int order = 0;
        for (int i = 0; i < 4; i++)
        {
            if (cardlist[i].Effect && cardlist[i].Number == 22) order = orderSet(CaE_buffer, order, i);
        }
        for (int i = 0; i < 4; i++)
        {
            if (cardlist[i].Effect && cardlist[i].Number == 18) order = orderSet(CaE_buffer, order, i);
        }
        for (int i = 0; i < 4; i++)
        {
            if (cardlist[i].Effect && cardlist[i].Number == 15) order = orderSet(CaE_buffer, order, i);
        }
        int[] randomarr = { 0, 1, 2, 3 };
        randomarr = randomarr.OrderBy(i => System.Guid.NewGuid()).ToArray();
        for (int i = 0; i < 4; i++)
        {
            if (cardlist[randomarr[i]].Effect && !ListExist(CaE_buffer, randomarr[i])) order = orderSet(CaE_buffer, order, randomarr[i]);
        }
    }
    /*num1>num2
     * 役効果
     */
    private void HEmanager(int num1 ,int num2,string tag)
    {
        hand_processed = true;
        if (num1 == 22)
        {
            if (num2 == 21) StartCoroutine(HE2221(tag));
            else if (num2 == 15) StartCoroutine(HE2215(tag));
            else if (num2 == 12) StartCoroutine(HE2212(tag));
            else if (num2 == 7) StartCoroutine(HE227(tag));
        }
        else if (num1 == 21)
        {
            if (num2 == 5) StartCoroutine(HE215(tag));
            else if (num2 == 10) StartCoroutine(HE2110(tag));
            else if (num2 == 14) StartCoroutine(HE2114(tag));
        }
        else if (num1 == 20)
        {
            if (num2 == 4) StartCoroutine(HE204(tag));
            else if (num2 == 5) StartCoroutine(HE205(tag));
            else if (num2 == 16) StartCoroutine(HE2016(tag));
            else if (num2 == 19) StartCoroutine(HE2019(tag));
        }
        else if (num1 == 19)
        {
            if (num2 == 2) StartCoroutine(HE192(tag));
            else if (num2 == 5) StartCoroutine(HE195(tag));
            else if (num2 == 14) StartCoroutine(HE1914(tag));
        }
        else if (num1 == 18)
        {
            if (num2 == 2) StartCoroutine(HE182(tag));
            else if (num2 == 6) StartCoroutine(HE186(tag));
            else if (num2 == 16) StartCoroutine(HE1816(tag));
            else if (num2 == 17) StartCoroutine(HE1817(tag));
        }
        else if (num1 == 17)
        {
            if (num2 == 2) StartCoroutine(HE172(tag));
            else if (num2 == 7) StartCoroutine(HE177(tag));
            else if (num2 == 10) StartCoroutine(HE1710(tag));
        }
        else if (num1 == 16)
        {
            if (num2 == 1) StartCoroutine(HE161(tag));
            else if (num2 == 13) StartCoroutine(HE1613(tag));
        }
        else if (num1 == 15)
        {
            if (num2 == 6) StartCoroutine(HE156(tag));
            else if (num2 == 8) StartCoroutine(HE158(tag));
            else if (num2 == 12) StartCoroutine(HE1512(tag));
        }
        else if (num1 == 14)
        {
            if (num2 == 9) StartCoroutine(HE149(tag));
            else if (num2 == 11) StartCoroutine(HE1411(tag));
        }
        else if (num1 == 13)
        {
            if (num2 == 6) StartCoroutine(HE136(tag));
            else if (num2 == 7) StartCoroutine(HE137(tag));
            else if (num2 == 12) StartCoroutine(HE1312(tag));
        }
        else if (num1 == 12)
        {
            if (num2 == 1) StartCoroutine(HE121(tag));
        }
        else if (num1 == 11)
        {
            if (num2 == 3) StartCoroutine(HE113(tag));
            else if (num2 == 5) StartCoroutine(HE115(tag));
            else if (num2 == 8) StartCoroutine(HE118(tag));
        }
        else if (num1 == 10)
        {
            if (num2 == 3) StartCoroutine(HE103(tag));
            else if (num2 == 6) StartCoroutine(HE106(tag));
            else if (num2 == 8) StartCoroutine(HE108(tag));
        }
        else if (num1 == 9)
        {
            if (num2 == 1) StartCoroutine(HE91(tag));
            else if (num2 == 4) StartCoroutine(HE94(tag));
            else if (num2 == 6) StartCoroutine(HE96(tag));
            else if (num2 == 8) StartCoroutine(HE98(tag));
            
        }
        else if (num1 == 8)
        {
            if (num2 == 1) StartCoroutine(HE81(tag));
        }
        else if (num1 == 7)
        {
            if (num2 == 3) StartCoroutine(HE73(tag));
            if (num2 == 4) StartCoroutine(HE74(tag));
        }
        else if (num1 == 5)
        {
            if (num2 == 2) StartCoroutine(HE52(tag));
        }
        else if (num1 == 4)
        {
            if (num2 == 1) StartCoroutine(HE41(tag));
            if (num2 == 3) StartCoroutine(HE43(tag));
        }
        else if (num1 == 3)
        {
            if (num2 == 2) StartCoroutine(HE32(tag));
        }
    }
    /*セットするのは最初だけ
     * falseはこのメソッドを経由しない
     */
    private void HEsetter(int num, string tag)
    {
        if (tag == "Player")
        {
            if (num == 74 || num == 94) HEsetterP[0] = true;
            else if (num == 177) HEsetterP[1] = true;
            else if (num == 113 || num == 81) HEsetterP[2] = true;
            else if (num == 195) HEsetterP[3] = true;
            else if (num == 2019) HEsetterP[4] = true;
            else if (num == 91) HEsetterP[5] = true;
            else if (num == 2212) HEsetterP[6] = true;
            else if (num == 121) HEsetterP[7] = true;
            else if (num == 1710) HEsetterP[8] = true;
            else if (num == 81) HEsetterP[9] = true;
            else if (num == 1512) HETurn2P[0] = 1;
            else if (num == 1613) HETurn2P[1] = 1;
            else if (num == 182) HETurn2P[2] = 1;
        }
        else if (tag == "Enemy")
        {
            if (num == 74 || num == 94) HEsetterE[0] = true;
            else if (num == 177) HEsetterE[1] = true;
            else if (num == 113 || num == 81) HEsetterE[2] = true;
            else if (num == 195) HEsetterE[3] = true;
            else if (num == 2019) HEsetterE[4] = true;
            else if (num == 91) HEsetterE[5] = true;
            else if (num == 2212) HEsetterE[6] = true;
            else if (num == 121) HEsetterE[7] = true;
            else if (num == 1710) HEsetterE[8] = true;
            else if (num == 81) HEsetterE[9] = true;
            else if (num == 1512) HETurn2E[0] = 1;
            else if (num == 1613) HETurn2E[1] = 1;
            else if (num == 182) HETurn2E[2] = 1;
        }
    }
    private object HEgetter(int num, string tag)
    {
        if (tag == "Player")
        {
            if (num == 74 || num == 94) return HEsetterP[0];
            else if (num == 177) return HEsetterP[1];
            else if (num == 113) return HEsetterP[2];
            else if (num == 195) return HEsetterP[3];
            else if (num == 2019) return HEsetterP[4];
            else if (num == 91) return HEsetterP[5];
            else if (num == 2212) return HEsetterP[6];
            else if (num == 121) return HEsetterP[7];
            else if (num == 1710) return HEsetterP[8];
            else if (num == 81) return HEsetterP[9];
            else if (num == 1512) return HETurn2P[0];
            else if (num == 1613) return HETurn2P[1];
            else if (num == 182) return HETurn2P[2];
        }
        else if (tag == "Enemy")
        {

            if (num == 74 || num == 94) return HEsetterE[0];
            else if (num == 177) return HEsetterE[1];
            else if (num == 113) return HEsetterE[2];
            else if (num == 195) return HEsetterE[3];
            else if (num == 2019) return HEsetterE[4];
            else if (num == 91) return HEsetterE[5];
            else if (num == 2212) return HEsetterE[6];
            else if (num == 121) return HEsetterE[7];
            else if (num == 1710) return HEsetterE[8];
            else if (num == 81) return HEsetterE[9];
            else if (num == 1512) return HETurn2E[0];
            else if (num == 1613) return HETurn2E[1];
            else if (num == 182) return HETurn2E[2];
        }
        return null;
    }
    IEnumerator HE41(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "チャンスメイク";
            PHtext.color = Color.blue;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
            List<GameObject> clist = new List<GameObject>();
            for (int i = 0; i < 2; i++) clist.Add(enemy.Field[i].GetChild(0).gameObject);
            enemy.Flipura(clist);
            yield return new WaitUntil(() => enemy.flipb);
            for (int i = 0; i < 2; i++)
            {
                enemy.Handset(enemy.Field[i].GetChild(0));
                yield return new WaitUntil(() => enemy.set);
            }

            int[] random = new int[enemy.Hand.Length];
            for (int i = 0; i < random.Length; i++) random[i] = i;
            random = random.OrderBy(i => System.Guid.NewGuid()).ToArray();
            int counter = 0;
            for (int i = 0; i < random.Length; i++)
            {
                if (enemy.Hand[random[i]].childCount != 0)
                {
                    enemy.Fieldset(enemy.Hand[random[i]].GetChild(0));
                    yield return new WaitUntil(() => enemy.set);
                    if (++counter == 2) break;
                }
            }
            enemy.FieldCount = 0;
            List<GameObject> ef = new List<GameObject>();
            for (int i = 0; i < 2; i++) ef.Add(enemy.Field[i].GetChild(0).gameObject);
            enemy.Flip(ef);
            yield return new WaitUntil(() => enemy.flipb);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "チャンスメイク";
            EHtext.color = Color.blue;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
            for (int i = 0; i < 2; i++)
            {
                player.Handset(player.Field[i].GetChild(0));
                yield return new WaitUntil(() => player.set);
            }
            int[] random = new int[player.Hand.Length];
            for (int i = 0; i < random.Length; i++) random[i] = i;
            random = random.OrderBy(i => System.Guid.NewGuid()).ToArray();
            int counter = 0;
            for (int i = 0; i < random.Length; i++)
            {
                if (player.Hand[random[i]].childCount != 0)
                {
                    player.Fieldset(player.Hand[random[i]].GetChild(0));
                    yield return new WaitUntil(() => player.set);
                    if (++counter==2) break;
                }
            }
            player.FieldCount = 0;
            List<GameObject> pf = new List<GameObject>();
            for (int i = 0; i < 2; i++) pf.Add(player.Field[i].GetChild(0).gameObject);
            player.Flip(pf);
            yield return new WaitUntil(() => player.flipb);
        }
        fieldreset = true;
        HEtable[4, 1] = false;
        HEtable[1, 4] = false;
        hand_effect = false;
    }
    IEnumerator HE96(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "導き手";
            PHtext.color = Color.blue;
            player.AD = true;
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "導き手";
            EHtext.color = Color.blue;
            enemy.AD = true;
        }
        hand_effect = false;
    }
    IEnumerator HE73(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "アマゾネスクイーン";
            PHtext.color = Color.blue;
            AnchDef[0] += 2;
            DPlus[0] += 2;
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "アマゾネスクイーン";
            EHtext.color = Color.blue;
            AnchDef[1] += 2;
            DPlus[1] += 2;
        }
        hand_effect = false;
    }
    IEnumerator HE108(string tag)
    {
        
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "先手必勝";
            PHtext.color = Color.blue;
            if (manager.GetBattleTurn() != 0)
            {
                hand_effect = false;
                yield break;
            }
            enemy.HPChange(-3);
            player.HPChange(3);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "先手必勝";
            EHtext.color = Color.blue;
            if (manager.GetBattleTurn() != 0)
            {
                hand_effect = false;
                yield break;
            }
            enemy.HPChange(3);
            player.HPChange(-3);
        }
        hand_effect = false;
    }
    IEnumerator HE52(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "救済するもの";
            PHtext.color = Color.blue;
            player.HPChange(4);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "救済するもの";
            EHtext.color = Color.blue;
            enemy.HPChange(4);
        }
        hand_effect = false;
    }
    IEnumerator HE103(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "傾国の美女";
            PHtext.color = Color.blue;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
            for (int i = 0; i < enemy.Hand.Length; i++)
            {
                if (enemy.Hand[i].childCount != 0)
                {
                    string Cname = enemy.Hand[i].GetChild(0).name;
                    if (Cname.Contains("Chariot")||Cname.Contains("Emperor")||Cname.Contains("Lovers"))
                    {
                        Transform card = enemy.Hand[i].GetChild(0);
                        if (enemy.GetHEbuf(2221) || enemy.GetHEbuf(182) || (enemy.satoriF - manager.GetBattleTurn()) > 0)
                        {
                            card.GetComponent<SpriteRenderer>().sprite = manager.arcana[0];
                        }
                        else card.GetComponent<SpriteRenderer>().sprite = manager.cardback;
                        card.name = "Citizen";
                    }
                }
            }
            for (int i = 2; i < 4; i++)
            {
                string Cname = cardlist[i].Name;
                if (Cname.Contains("Chariot") || Cname.Contains("Emperor") || Cname.Contains("Lovers"))
                {
                    Transform card = enemy.Field[i-2].GetChild(0);
                    card.GetComponent<SpriteRenderer>().sprite = manager.arcana[0];
                    card.name = "Citizen";
                    fieldreset = true;
                    HEtable[10, 3] = false;
                    HEtable[3, 10] = false;
                }
            }
            enemy.ChangeCard(new int[] { 4, 6, 7 }, 0);
            enemy.Trash.ChangeCard(new string[] { "Chariot", "Emperor", "Lovers" }, "Citizen");
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "傾国の美女";
            EHtext.color = Color.blue;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
            for (int i = 0; i < player.Hand.Length; i++)
            {
                if (player.Hand[i].childCount != 0)
                {
                    string Cname = player.Hand[i].GetChild(0).name;
                    if (Cname.Contains("Chariot") || Cname.Contains("Emperor") || Cname.Contains("Lovers"))
                    {
                        Transform card = player.Hand[i].GetChild(0);
                        card.GetComponent<SpriteRenderer>().sprite = manager.arcana[0];
                        card.name = "Citizen";
                    }
                }
            }
            for (int i = 0; i < 2; i++)
            {
                string Cname = cardlist[i].Name;
                if (Cname.Contains("Chariot") || Cname.Contains("Emperor") || Cname.Contains("Lovers"))
                {
                    Transform card = player.Field[i].GetChild(0);
                    card.GetComponent<SpriteRenderer>().sprite = manager.arcana[0];
                    card.name = "Citizen";
                    fieldreset = true;
                    HEtable[10, 3] = false;
                    HEtable[3, 10] = false;
                }
            }
            player.ChangeCard(new int[] { 4, 6, 7 }, 0);
            player.Trash.ChangeCard(new string[] { "Chariot", "Emperor", "Lovers" }, "Citizen");
        }
        hand_effect = false;
    }
    IEnumerator HE2114(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "理想郷";
            PHtext.color = Color.blue;
            if (manager.GetBattleTurn() >= 9)
            {
                hand_effect = false;
                yield break;
            } 
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
            enemy.HPChange(player.HP - enemy.HP);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "理想郷";
            EHtext.color = Color.blue;
            if (manager.GetBattleTurn() >= 9)
            {
                hand_effect = false;
                yield break;
            }
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
            player.HPChange(enemy.HP - player.HP);
        }
        hand_effect = false;
    }
    IEnumerator HE192(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "雨除けの巫女";
            PHtext.color = Color.blue;
            player.SetHEbuf(192, true);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "雨除けの巫女";
            EHtext.color = Color.blue;
            enemy.SetHEbuf(192, true);
        }
        hand_effect = false;
    }
    IEnumerator HE1312(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "ラグナロク";
            PHtext.color = Color.blue;
            player.SetHEbuf(1312, true);
            player.HPChange(-2);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "ラグナロク";
            EHtext.color = Color.blue;
            enemy.SetHEbuf(1312, true);
            enemy.HPChange(-2);
        }
        hand_effect = false;
    }
    IEnumerator HE81(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "ベクトル操作";
            PHtext.color = Color.blue;
            HEsetter(81, tag);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "ベクトル操作";
            EHtext.color = Color.blue;
            HEsetter(81, tag);
        }
        counterCoef = 1;
        hand_effect = false;
    }
    IEnumerator HE156(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "禁断の果実";
            PHtext.color = Color.blue;
            player.SetHEbuf(156,true);
            player.HPChange(-3);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "禁断の果実";
            EHtext.color = Color.blue;
            enemy.SetHEbuf(156, true);
            enemy.HPChange(-3);
        }
        hand_effect = false;
    }
    public void HE156_CharacterBack(string tag)
    {
        StartCoroutine(HE156_1(tag));
    }
    IEnumerator HE156_1(string tag)
    {
        if (player.GetHEbuf(156) && enemy.GetHEbuf(156))
        {
            //それぞれから二つ来ていると同期が難しいので、片方を切り捨てる。Enemyであることに意味はない
            //片方のコルーチンで両方ともの処理をするので、処理の問題はない.
            if (tag == "Enemy") yield break;
            if (Random.value<0.5f)
            {
                StartCoroutine(HE156_2(player.tag));
                yield return new WaitUntil(() => !hand_processed);
                StartCoroutine(HE156_2(enemy.tag));
                yield return new WaitUntil(() => !hand_processed);
            }
            else
            {
                StartCoroutine(HE156_2(enemy.tag));
                yield return new WaitUntil(() => !hand_processed);
                StartCoroutine(HE156_2(player.tag));
                yield return new WaitUntil(() => !hand_processed);
            }
        }
        else if (player.GetHEbuf(156))
        {
            StartCoroutine(HE156_2(player.tag));
            yield return new WaitUntil(() => !hand_processed);
        }
        else if (enemy.GetHEbuf(156))
        {
            StartCoroutine(HE156_2(enemy.tag));
            yield return new WaitUntil(() => !hand_processed);
        }
    }
    IEnumerator HE156_2(string tag)
    {
        hand_processed = true;
        if (tag.Contains("Player"))
        {
            if (player.HandCardNum() > 0)
            {
                int num = player.HandCardNum();
                player.Hand2Deck("all");
                yield return new WaitUntil(() => player.synch);
                SelectSetting(num);
                SelPanelSet(player.tag, "Deck");
                yield return new WaitUntil(() => selected);
                player.PlayerUIReset();
                for (int i = 0; i < num; i++)
                {
                    player.DT2Hand(selectName[i], "Deck");
                    yield return new WaitUntil(() => player.synch);
                }
                player.DeckShuffle();
                yield return new WaitUntil(() => player.synch);
            }
            player.SetHEbuf(156, false);
        }
        else if (tag.Contains("Enemy"))
        {
            if (enemy.HandCardNum() > 0)
            {
                int num = enemy.HandCardNum();
                enemy.Hand2Deck("all");
                yield return new WaitUntil(() => enemy.synch);
                List<int> edeck = enemy.GetDeck();
                edeck = edeck.OrderBy(i => System.Guid.NewGuid()).ToList();
                for (int i = 0; i < num; i++)
                {
                    enemy.DT2Hand(num2arcana(edeck[i]), "Deck");
                    yield return new WaitUntil(() => enemy.synch);
                }
                enemy.DeckShuffle();
                yield return new WaitUntil(() => enemy.synch);
            }
            enemy.SetHEbuf(156, false);
        }
        hand_processed = false;
    }
    IEnumerator HE2016(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "神の審判";
            PHtext.color = Color.blue;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
            int num1 = cardlist[2].Number > cardlist[3].Number ? cardlist[2].Number : cardlist[3].Number;
            int num2 = cardlist[2].Number > cardlist[3].Number ? cardlist[3].Number : cardlist[2].Number;
            if (num1 == 20 && num2 == 16)
            {
                hand_effect = false;
                yield break;
            }
            if (HE_buffer[0] == 0)
            {
                if (HE_buffer[1] == 1)
                {
                    for (int i = 0; i < HE_buffer.Length; i++) HE_buffer[i] = -1;
                    enemy.HPChange(-4);
                }
                else enemy.HPChange(2);
            }
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "神の審判";
            EHtext.color = Color.blue;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
            int num1 = cardlist[0].Number > cardlist[1].Number ? cardlist[0].Number : cardlist[1].Number;
            int num2 = cardlist[0].Number > cardlist[1].Number ? cardlist[1].Number : cardlist[0].Number;
            if (num1 == 20 && num2 == 16)
            {
                hand_effect = false;
                yield break;
            }
            if (HE_buffer[0] == 1)
            {
                if (HE_buffer[1] == 0)
                {
                    for (int i = 0; i < HE_buffer.Length; i++) HE_buffer[i] = -1;
                    player.HPChange(-4);
                }
                else player.HPChange(2);
            }
        }
        hand_effect = false;
    }
    IEnumerator HE115(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "正義の法";
            PHtext.color = Color.blue;
            player.AD = true;
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "正義の法";
            EHtext.color = Color.blue;
            enemy.AD = true;
        }
        hand_effect = false;
    }
    IEnumerator HE94(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "約束された勝利の剣";
            PHtext.color = Color.red;
            Card emperor = cardlist[0].Number < cardlist[1].Number ? cardlist[0] : cardlist[1];
            if (emperor.Number == 4) emperor.Attack *= 3;
            HEsetter(94, tag);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "約束された勝利の剣";
            EHtext.color = Color.red;
            Card emperor = cardlist[2].Number < cardlist[3].Number ? cardlist[2] : cardlist[3];
            if (emperor.Number == 4) emperor.Attack *= 3;
            HEsetter(94, tag);
        }
        hand_effect = false;
    }
    IEnumerator HE1817(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "スターリー・ナイツ";
            PHtext.color = Color.blue;
            APlus[0] += 1;
            DPlus[0] += 2;
            player.HPChange(2);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "スターリー・ナイツ";
            EHtext.color = Color.blue;
            APlus[1] += 1;
            DPlus[1] += 2;
            player.HPChange(2);
        }
        hand_effect = false;
    }
    IEnumerator HE227(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "荒野を拓くもの";
            PHtext.color = Color.red;
            APlus[0] += 2;
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "荒野を拓くもの";
            EHtext.color = Color.red;
            APlus[1] += 2;
        }
        hand_effect = false;
    }
    IEnumerator HE215(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "世界の真理";
            PHtext.color = Color.cyan;
            if (HEtable[cardlist[2].Number, cardlist[3].Number])
            {
                int num1 = cardlist[2].Number > cardlist[3].Number ? cardlist[2].Number : cardlist[3].Number;
                int num2 = cardlist[2].Number > cardlist[3].Number ? cardlist[3].Number : cardlist[2].Number;
                if (!(num1 == 21 && num2 == 5))
                {
                    for (int i = 0; i < HE_buffer.Length; i++) HE_buffer[i] = -1;

                    SelectSetting(1);
                    CanselSet();
                    AgreeSet();
                    yield return new WaitUntil(() => selected || selectCansel);
                    manager.state = State.Battle;
                    player.PlayerUIReset();
                    if (!selectCansel)
                    {
                        hand_effect = false;
                        HEmanager(num1, num2, player.tag);
                        yield break;
                    }
                }
            }
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "世界の真理";
            EHtext.color = Color.cyan;
            if (HEtable[cardlist[0].Number, cardlist[1].Number])
            {
                int num1 = cardlist[0].Number > cardlist[1].Number ? cardlist[0].Number : cardlist[1].Number;
                int num2 = cardlist[0].Number > cardlist[1].Number ? cardlist[1].Number : cardlist[0].Number;
                if (!(num1 == 21 && num2 == 5))
                {
                    for (int i = 0; i < HE_buffer.Length; i++) HE_buffer[i] = -1;
                    //効果を使いたくないやつのリスト書こう
                    if (!(num1 == 18 && num2 == 16) || !(num1 == 17 && num2 == 10) ||
                        !(num1 == 15 && num2 == 8) || !(num1 == 14 && num2 == 11 && enemy.HP > player.HP) || !(num1 == 22 && num2 == 12) ||
                        !(num1 == 9 && num1 == 1) || !(num1 == 4 && num2 == 3) || !(num1 == 13 && num2 == 6))
                    {
                        hand_effect = false;
                        HEmanager(num1, num2, player.tag);
                        yield break;
                    }
                }
            }
        }
        hand_effect = false;
    }
    IEnumerator HE204(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "断罪の皇帝";
            PHtext.color = Color.cyan;
            int num1 = cardlist[2].Number > cardlist[3].Number ? cardlist[2].Number : cardlist[3].Number;
            int num2 = cardlist[2].Number > cardlist[3].Number ? cardlist[3].Number : cardlist[2].Number;
            if (num1 == 20 && num2 == 4)
            {
                hand_effect = false;
                yield break;
            }
            for (int i = 0; i < 2; i++)
            {
                Transform card = enemy.Field[i].GetChild(0);
                if (arcana2num(card.name) < 17)
                {
                    card.GetComponent<SpriteRenderer>().sprite = manager.arcana[0];
                    card.name = "Citizen";
                    fieldreset = true;
                }
            }
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "断罪の皇帝";
            EHtext.color = Color.cyan;
            int num1 = cardlist[0].Number > cardlist[1].Number ? cardlist[0].Number : cardlist[1].Number;
            int num2 = cardlist[0].Number > cardlist[1].Number ? cardlist[1].Number : cardlist[0].Number;
            if (num1 == 20 && num2 == 4)
            {
                hand_effect = false;
                yield break;
            }
            for (int i = 0; i < 2; i++)
            {
                Transform card = player.Field[i].GetChild(0);
                if (arcana2num(card.name) < 17)
                {
                    card.GetComponent<SpriteRenderer>().sprite = manager.arcana[0];
                    card.name = "Citizen";
                    fieldreset = true;
                }
            }
        }
        if (fieldreset)
        {
            HEtable[20, 4] = false;
            HEtable[4, 20] = false;
        }
        hand_effect = false;
    }
    IEnumerator HE1816(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "狂乱の坩堝";
            PHtext.color = Color.cyan;
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "狂乱の坩堝";
            EHtext.color = Color.cyan;
        }
        HEtable[18, 16] = false;
        HEtable[16, 18] = false;
        fieldreset = true;
        player.CardTrash();
        enemy.CardTrash();
        yield return new WaitUntil(() => (enemy.synch && player.synch));
        List<string> plist = player.Trash.GetListTrashCard();
        List<string> elist = enemy.Trash.GetListTrashCard();
        
        plist = plist.OrderBy(i => System.Guid.NewGuid()).ToList();
        elist = elist.OrderBy(i => System.Guid.NewGuid()).ToList();
        
        for (int i = 0; i < 2; i++)
        {
            GameObject pobj = Instantiate((GameObject)Resources.Load("Prefab/Card"));
            GameObject eobj = Instantiate((GameObject)Resources.Load("Prefab/Card"));
            pobj.name = plist[i];
            eobj.name = elist[i];
            pobj.tag = player.tag;
            eobj.tag = enemy.tag;
            pobj.GetComponent<SpriteRenderer>().sprite = manager.arcana[arcana2num(plist[i])];
            eobj.GetComponent<SpriteRenderer>().sprite = manager.arcana[arcana2num(elist[i])];
            pobj.transform.position = player.Trash.transform.position;
            eobj.transform.position = enemy.Trash.transform.position;
            player.Fieldset(pobj.transform);
            enemy.Fieldset(eobj.transform);
            player.Trash.Salvage(plist[i]);
            enemy.Trash.Salvage(elist[i]);
            yield return new WaitUntil(() => player.set && enemy.set);
            pobj.transform.localRotation = Quaternion.identity;
            eobj.transform.localRotation = Quaternion.identity;
        }
        hand_effect = false;
    }
    IEnumerator HE106(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "恋人は惹かれ合う";
            PHtext.color = Color.cyan;
            if (cardlist[2].Number == 6 || cardlist[3].Number == 6)
            {
                hand_effect = false;
                yield break;
            }
            SelectSetting(1);
            FSelSet();
            yield return new WaitUntil(() => selected);
            manager.state = State.Battle;
            player.PlayerUIReset();
            Transform slc = enemy.Field[int.Parse(selectName[0])].GetChild(0);
            slc.GetComponent<SpriteRenderer>().sprite = manager.arcana[6];
            slc.name = "The Lovers";
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "恋人は惹かれ合う";
            EHtext.color = Color.cyan;
            if (cardlist[0].Number == 6||cardlist[1].Number==6)
            {
                hand_effect = false;
                yield break;
            }
            int num = cardlist[0].Number > cardlist[1].Number ? 0 : 1;
            Transform slc = player.Field[num].GetChild(0);
            slc.GetComponent<SpriteRenderer>().sprite = manager.arcana[6];
            slc.name = "The Lovers";
        }
        HEtable[10, 6] = false;
        HEtable[6, 10] = false;
        fieldreset = true;
        hand_effect = false;
    }
    public void HE186_CharacterBack(string tag)
    {
        StartCoroutine(HE186_1(tag));
    }
    IEnumerator HE186_1(string tag)
    {
        if (player.GetHEbuf(186) && enemy.GetHEbuf(186))
        {
            //それぞれから二つ来ていると同期が難しいので、片方を切り捨てる。Enemyであることに意味はない
            //片方のコルーチンで両方ともの処理をするので、処理の問題はない.
            if (tag == "Enemy") yield break;
            if (Random.value<0.5f)
            {
                StartCoroutine(HE186_2(player.tag));
                yield return new WaitUntil(() => !hand_processed);
                StartCoroutine(HE186_2(enemy.tag));
                yield return new WaitUntil(() => !hand_processed);
            }
            else
            {
                StartCoroutine(HE186_2(enemy.tag));
                yield return new WaitUntil(() => !hand_processed);
                StartCoroutine(HE186_2(player.tag));
                yield return new WaitUntil(() => !hand_processed);
            }
        }
        else if (enemy.GetHEbuf(186))
        {
            StartCoroutine(HE186_2(player.tag));
            yield return new WaitUntil(() => !hand_processed);
        }
        else if (player.GetHEbuf(186))
        {
            StartCoroutine(HE186_2(enemy.tag));
            yield return new WaitUntil(() => !hand_processed);
        }
    }
    IEnumerator HE186_2(string tag)
    {
        hand_processed = true;
        if (tag.Contains("Player"))
        {
            if (enemy.HandCardNum() > 0)
            {
                List<GameObject> clist = new List<GameObject>();
                for (int i = 0; i < enemy.Hand.Length; i++)
                {
                    if (enemy.Hand[i].childCount != 0)
                    {
                        clist.Add(enemy.Hand[i].GetChild(0).gameObject);
                    }
                }
                enemy.Flip(clist);
                yield return new WaitUntil(() => enemy.flipb);
                SelectSetting(1);
                HFSelSet();
                yield return new WaitUntil(() => selected);
                player.PlayerUIReset();
                enemy.Fieldset(enemy.Hand[int.Parse(selectName[0])].GetChild(0));
                yield return new WaitUntil(() => enemy.set);
                clist = new List<GameObject>();
                for (int i = 0; i < enemy.Hand.Length; i++)
                {
                    if (enemy.Hand[i].childCount != 0)
                    {
                        clist.Add(enemy.Hand[i].GetChild(0).gameObject);
                    }
                }
                enemy.Flipura(clist);
                yield return new WaitUntil(() => enemy.flipb);
            }
            enemy.SetHEbuf(186, false);
        }
        else if (tag.Contains("Enemy"))
        {
            if (player.HandCardNum() > 0)
            {
                int[] random = new int[player.Hand.Length];
                for (int i = 0; i < random.Length; i++) random[i] = i;
                random = random.OrderBy(i => System.Guid.NewGuid()).ToArray();
                for (int i = 0; i < random.Length; i++)
                {
                    if (player.Hand[random[i]].childCount != 0)
                    {
                        if (!((player.Hand[random[i]].GetChild(0).name.Contains("Fool")
                     || player.Hand[random[i]].GetChild(0).name.Contains("World")) && manager.GetBattleTurn() < 5))
                        {
                            player.Fieldset(player.Hand[random[i]].GetChild(0));
                            yield return new WaitUntil(() => player.set);
                            break;
                        }
                    }

                }
            }
            player.SetHEbuf(186, false);
        }
        hand_processed = false;
    }
    IEnumerator HE186(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "月への憧憬";
            PHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
            enemy.SetHEbuf(186, true);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "月への憧憬";
            EHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
            player.SetHEbuf(186, true);
        }
        hand_effect = false;
    }
    IEnumerator HE2110(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "胎動する世界";
            PHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
            int num0 = cardlist[2].Number > cardlist[3].Number ? cardlist[2].Number : cardlist[3].Number;
            int num1 = cardlist[2].Number > cardlist[3].Number ? cardlist[3].Number : cardlist[2].Number;
            if (num0 == 21 && num1 == 10)
            {
                hand_effect = false;
                yield break;
            }
            else if ((num0 == 22 && num1 == 15) || (num0 == 14 && num1 == 9))
            {
                for (int i = 0; i < HE_buffer.Length; i++) HE_buffer[i] = -1;
            }
            //プレイヤー選択
            int nump = player.HandCardNum();
            int nume = enemy.HandCardNum();
            player.Hand2Deck("all");
            enemy.Hand2Deck("all");
            yield return new WaitUntil(() => player.synch && enemy.synch);
            
            SelectSetting(nump);
            SelPanelSet(player.tag, "Deck");
            yield return new WaitUntil(() => selected);
            manager.state = State.Battle;
            player.PlayerUIReset();
            for (int i = 0; i < nump; i++)
            {
                player.DT2Hand(selectName[i], "Deck");
                yield return new WaitUntil(() => player.synch);
            }
            player.DeckShuffle();
            yield return new WaitUntil(() => player.synch);
            //エネミー選択
            SelectSetting(nume);
            SelPanelSet(enemy.tag, "Deck");
            yield return new WaitUntil(() => selected);
            manager.state = State.Battle;
            player.PlayerUIReset();
            for (int i = 0; i < nume; i++)
            {
                enemy.DT2Hand(selectName[i], "Deck");
                yield return new WaitUntil(() => enemy.synch);
            }
            enemy.DeckShuffle();
            yield return new WaitUntil(() => enemy.synch);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "胎動する世界";
            EHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
            int num0 = cardlist[0].Number > cardlist[1].Number ? cardlist[0].Number : cardlist[1].Number;
            int num1 = cardlist[0].Number > cardlist[1].Number ? cardlist[1].Number : cardlist[0].Number;
            if (num0 == 21 && num1 == 10)
            {
                hand_effect = false;
                yield break;
            }
            else if ((num0 == 22 && num1 == 15) || (num0 == 14 && num1 == 9))
            {
                for (int i = 0; i < HE_buffer.Length; i++) HE_buffer[i] = -1;
            }
            //ランダム
            int nump = player.HandCardNum();
            int nume = enemy.HandCardNum();
            enemy.Hand2Deck("all");
            player.Hand2Deck("all");
            yield return new WaitUntil(() => player.synch&&enemy.synch);
            List<int> edeck = enemy.GetDeck();
            edeck = edeck.OrderBy(i => System.Guid.NewGuid()).ToList();
            for (int i = 0; i < nume; i++)
            {
                enemy.DT2Hand(num2arcana(edeck[i]), "Deck");
                yield return new WaitUntil(() => enemy.synch);
            }
            enemy.DeckShuffle();
            yield return new WaitUntil(() => enemy.synch);

            List<int> pdeck = player.GetDeck();
            pdeck = pdeck.OrderBy(i => System.Guid.NewGuid()).ToList();
            for(int i = 0; i < nump; i++)
            {
                player.DT2Hand(num2arcana(pdeck[i]), "Deck");
                yield return new WaitUntil(() => player.synch);
            }
            player.DeckShuffle();
            yield return new WaitUntil(() => player.synch);
        }
        for (int i = 0; i < 4; i++)
        {
            if (cardlist[i].Number == 1 || cardlist[i].Number == 10 || cardlist[i].Number == 15) cardlist[i].Effect = false;
        }
        hand_effect = false;
    }
    IEnumerator HE182(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "欺瞞の女教皇";
            PHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
            enemy.SetHEbuf(182, true);
            HEsetter(182, tag);
            if (enemy.HandCardNum() > 0)
            {
                List<GameObject> clist = new List<GameObject>();
                for (int i = 0; i < enemy.Hand.Length; i++)
                {
                    if (enemy.Hand[i].childCount != 0)
                    {
                        clist.Add(enemy.Hand[i].GetChild(0).gameObject);
                    }
                }
                enemy.Flip(clist);
                yield return new WaitUntil(() => enemy.flipb);
            }
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "欺瞞の女教皇";
            EHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
            player.SetHEbuf(182, true);
            HEsetter(182, tag);
        }
        hand_effect = false;
    }
    IEnumerator HE1710(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "運命の軌跡";
            PHtext.color = Color.cyan;
            HEsetter(1710, tag);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "運命の軌跡";
            EHtext.color = Color.cyan;
            HEsetter(1710, tag);
        }
        hand_effect = false;
    }
    IEnumerator HE158(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "鮮烈なる力";
            PHtext.color = Color.red;
            int counter = player.Trash.GetListTrashCard().Count;
            Card devil = cardlist[0].Number > cardlist[1].Number ? cardlist[0] : cardlist[1];
            if (devil.Number != 15)
            {
                hand_effect = false;
                yield break;
            } 
            if (counter >= 4)
            {
                devil.Attack *= 2;
                devil.Defense *= 2;
            }
            if (counter >= 14)
            {
                devil.Attack *= 2;
                devil.Defense *= 2;
            }
            if (counter >= 20)
            {
                for (int i = 2; i < 4; i++) cardlist[i].Effect = false;
                for (int i = 0; i < HE_buffer.Length; i++) HE_buffer[i] = -1;
            }
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "鮮烈なる力";
            EHtext.color = Color.red;
            int counter = enemy.Trash.GetListTrashCard().Count;
            Card devil = cardlist[2].Number > cardlist[3].Number ? cardlist[2] : cardlist[3];
            if (devil.Number != 15)
            {
                hand_effect = false;
                yield break;
            }
            if (counter >= 4)
            {
                devil.Attack *= 2;
                devil.Defense *= 2;
            }
            if (counter >= 14)
            {
                devil.Attack *= 2;
                devil.Defense *= 2;
            }
            if (counter >= 20)
            {
                for (int i = 0; i < 2; i++) cardlist[i].Effect = false;
                for (int i = 0; i < HE_buffer.Length; i++) HE_buffer[i] = -1;
            }
        }
        hand_effect = false;
    }
    IEnumerator HE149(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "止水の隠者";
            PHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
            if (enemy.HandCardNum() > 0)
            {
                int[] random = new int[enemy.Hand.Length];
                for (int i = 0; i < random.Length; i++) random[i] = i;
                random = random.OrderBy(i => System.Guid.NewGuid()).ToArray();
                int counter = 0;
                for (int i = 0; i < random.Length; i++)
                {
                    if (enemy.Hand[random[i]].childCount != 0)
                    {
                        enemy.Flip(new List<GameObject> { enemy.Hand[random[i]].GetChild(0).gameObject });
                        yield return new WaitUntil(() => enemy.flipb);
                        enemy.Hand2Trash(random[i].ToString());
                        yield return new WaitUntil(() => enemy.synch);
                        if (++counter == 2) break;
                    }
                }
                SelectSetting(2);
                SelPanelSet(enemy.tag, "Trash");
                yield return new WaitUntil(() => selected);
                manager.state = State.Battle;
                player.PlayerUIReset();
                for (int i = 0; i < selectCount; i++)
                {
                    enemy.DT2Hand(selectName[i], "Trash");
                    yield return new WaitUntil(() => enemy.synch);
                }
            }
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "止水の隠者";
            EHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
            if (player.HandCardNum() > 0)
            {
                int[] random = new int[enemy.Hand.Length];
                for (int i = 0; i < random.Length; i++) random[i] = i;
                random = random.OrderBy(i => System.Guid.NewGuid()).ToArray();
                int counter1 = 0;
                for (int i = 0; i < player.Hand.Length; i++)
                {
                    if (player.Hand[i].childCount != 0)
                    {
                        player.Hand2Trash(i.ToString());
                        yield return new WaitUntil(() => player.synch);
                        if (++counter1 == 2) break;
                    }
                }
                List<string> list = player.Trash.GetListTrashCard();
                list = list.OrderBy(i => System.Guid.NewGuid()).ToList();
                List<string> export = new List<string>();
                int counter = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (arcana2num(list[i]) < 17)
                    {
                        player.DT2Hand(list[i], "Trash");
                        export.Add(list[i]);
                        yield return new WaitUntil(() => player.synch);
                        if (++counter == 2) break;
                    }
                }
                if (counter != 2)
                {
                    for (int i = 0; i < export.Count; i++) list.Remove(export[i]); 
                    for (int i = 0; i < list.Count; i++)
                    {
                        player.DT2Hand(list[i], "Trash");
                        yield return new WaitUntil(() => player.synch);
                        if (++counter == 2) break;
                    }
                }
            }
        }
        hand_effect = false;
    }
    IEnumerator HE121(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "魔術の深奥";
            PHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
            HEsetter(121, tag);
            enemy.SetHEbuf(121,true);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "魔術の深奥";
            EHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
            HEsetter(121, tag);
            player.SetHEbuf(121, true);
        }
        hand_effect = false;
    }
    IEnumerator HE2212(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "忘却の愚者";
            PHtext.color = Color.cyan;
            HEsetter(2212, tag);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "忘却の愚者";
            EHtext.color = Color.cyan;
            HEsetter(2212, tag);
        }
        hand_effect = false;
    }
    IEnumerator HE91(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "可能性の開花";
            PHtext.color = Color.cyan;
            HEsetter(91, tag);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "可能性の開花";
            EHtext.color = Color.cyan;
            HEsetter(91, tag);
        }
        hand_effect = false;
    }
    IEnumerator HE2215(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "混沌導く奇術師";
            PHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "混沌導く奇術師";
            EHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
        }
        //プレイヤーとエネミーの手札枚数は同じ想定
        if (enemy.HandCardNum() > 0 && player.HandCardNum() > 0)
        {
            List<Transform> handp = new List<Transform>();
            List<Transform> hande = new List<Transform>();
            for (int i = 0; i < player.Hand.Length; i++)
            {
                if (player.Hand[i].childCount != 0) handp.Add(player.Hand[i].GetChild(0));
                if (enemy.Hand[i].childCount != 0) hande.Add(enemy.Hand[i].GetChild(0));
            }
            for (int i = 0; i < handp.Count; i++) handp[i].parent = null;
            for (int i = 0; i < hande.Count; i++) hande[i].parent = null;
            for (int i = 0; i < handp.Count; i++)
            {
                player.Handset(hande[i]);
                enemy.Handset(handp[i]);
                yield return new WaitUntil(() => player.set && enemy.set);
                player.Flip(new List<GameObject> { hande[i].gameObject });
                enemy.Flipura(new List<GameObject> { handp[i].gameObject });
                yield return new WaitUntil(() => player.flipb && enemy.flipb);
            }
        }
        hand_effect = false;
    }
    IEnumerator HE2221(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            
            PHtext.text = "旅人の夢";
            PHtext.color = Color.blue;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
            
            enemy.SetHEbuf(2221,true);
            if (enemy.HandCardNum() > 0)
            {
                List<GameObject> clist = new List<GameObject>();
                for (int i = 0; i < enemy.Hand.Length; i++)
                {
                    if (enemy.Hand[i].childCount != 0)
                    {
                        clist.Add(enemy.Hand[i].GetChild(0).gameObject);
                    }
                }
                enemy.Flip(clist);
                yield return new WaitUntil(() => enemy.flipb);
            }
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "旅人の夢";
            EHtext.color = Color.blue;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
            player.SetHEbuf(2221, true);
            if (player.HandCardNum() > 0)
            {
                List<GameObject> clist = new List<GameObject>();
                for (int i = 0; i < player.Hand.Length; i++)
                {
                    if (player.Hand[i].childCount != 0)
                    {
                        clist.Add(player.Hand[i].GetChild(0).gameObject);
                    }
                }
                player.Flip(clist);
                yield return new WaitUntil(() => player.flipb);
            }
        }
        hand_effect = false;
    }
    IEnumerator HE2019(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "方舟にかかる虹";
            PHtext.color = Color.cyan;
            HEsetter(2019, tag);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "方舟にかかる虹";
            EHtext.color = Color.cyan;
            HEsetter(2019, tag);
        }
        hand_effect = false;
    }
    IEnumerator HE195(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "破天荒なる教皇";
            PHtext.color = Color.red;
            HEsetter(2019, tag);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "破天荒なる教皇";
            EHtext.color = Color.red;
            HEsetter(2019, tag);
        }
        hand_effect = false;
    }
    IEnumerator HE1613(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "崩壊の足音";
            PHtext.color = Color.cyan;
            HEsetter(1613, tag);
            for (int i = 0; i < 2; i++)
            {
                if (cardlist[i].Number == 16 || cardlist[i].Number == 13) cardlist[i].Effect = false;
            }
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "崩壊の足音";
            EHtext.color = Color.cyan;
            HEsetter(1613, tag);
            for (int i = 2; i < 4; i++)
            {
                if (cardlist[i].Number == 16 || cardlist[i].Number == 13) cardlist[i].Effect = false;
            }
        }
        hand_effect = false;
    }
    IEnumerator HE1512(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "暗闇の先にあるもの";
            PHtext.color = Color.cyan;
            HEsetter(1512, tag);
            player.HPChange(-3);
            for (int i = 0; i < 2; i++) cardlist[i].Effect = false;
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "暗闇の先にあるもの";
            EHtext.color = Color.cyan;
            HEsetter(1512, tag);
            enemy.HPChange(-3);
            for (int i = 0; i < 2; i++) cardlist[i+2].Effect = false;
        }
        hand_effect = false;
    }
    IEnumerator HE136(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "嘆きの恋人";
            PHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
            int num0 = cardlist[2].Number > cardlist[3].Number ? cardlist[2].Number : cardlist[3].Number;
            int num1 = cardlist[2].Number > cardlist[3].Number ? cardlist[3].Number : cardlist[2].Number;
            Card death = cardlist[0].Number > cardlist[1].Number ? cardlist[0] : cardlist[1];
            Card lovers = cardlist[0].Number > cardlist[1].Number ? cardlist[1] : cardlist[0];
            if (num0 == 6 || num1 == 6)
            {
                enemy.HPChange(3);
                if(lovers.Number==6) lovers.Effect = false;
            }
            else
            {
                enemy.HPChange(-3);
                if(death.Number==13) death.Effect = false;
            }
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "嘆きの恋人";
            EHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
            int num0 = cardlist[0].Number > cardlist[1].Number ? cardlist[0].Number : cardlist[1].Number;
            int num1 = cardlist[0].Number > cardlist[1].Number ? cardlist[1].Number : cardlist[0].Number;
            Card death = cardlist[2].Number > cardlist[3].Number ? cardlist[2] : cardlist[3];
            Card lovers = cardlist[2].Number > cardlist[3].Number ? cardlist[3] : cardlist[2];
            if (num0 == 6 || num1 == 6)
            {
                player.HPChange(3);
                if (lovers.Number == 6) lovers.Effect = false;
            }
            else
            {
                player.HPChange(-3);
                if (death.Number == 13) death.Effect = false;
            }
        }
        hand_effect = false;
    }
    IEnumerator HE113(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "義憤の女帝";
            PHtext.color = Color.blue;
            HEsetter(113, tag);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "義憤の女帝";
            EHtext.color = Color.blue;
            HEsetter(113, tag);
        }
        counterCoef = 2;
        hand_effect = false;
    }
    IEnumerator HE118(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "力こそ正義";
            PHtext.color = Color.red;
            IgnoreD[0] = true;
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "力こそ正義";
            EHtext.color = Color.red;
            IgnoreD[1] = true;
        }
        hand_effect = false;
    }
    IEnumerator HE177(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "怒涛の戦車";
            PHtext.color = Color.red;
            Card star = cardlist[0].Number > cardlist[1].Number ? cardlist[0] : cardlist[1];
            if (star.Number == 17)
            {
                star.Attack += star.Defense;
                star.Defense = 0;
            }
            HEsetter(177, tag);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "怒涛の戦車";
            EHtext.color = Color.red;
            Card star = cardlist[2].Number > cardlist[3].Number ? cardlist[2] : cardlist[3];
            if (star.Number == 17)
            {
                star.Attack += star.Defense;
                star.Defense = 0;
            }
            HEsetter(177, tag);
        }
        hand_effect = false;
    }
    IEnumerator HE161(string tag)
    {
        StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "禍殃なる魔術師";
            PHtext.color = Color.red;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
            enemy.HPChange(-HCount[1]);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "禍殃なる魔術師";
            EHtext.color = Color.red;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
            player.HPChange(-HCount[0]);
        }
        hand_effect = false;
    }
    IEnumerator HE205(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "法王の裁き";
            PHtext.color = Color.blue;
            for(int i = 2; i < 4; i++)
            {
                cardlist[i].Effect = false;
            }
            int num1 = cardlist[2].Number > cardlist[3].Number ? cardlist[2].Number : cardlist[3].Number;
            int num2 = cardlist[2].Number > cardlist[3].Number ? cardlist[3].Number : cardlist[2].Number;
            if (!(num1 == 20 && num2 == 5)) for (int i = 0; i < HE_buffer.Length; i++) HE_buffer[i] = -1;
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "法王の裁き";
            EHtext.color = Color.blue;
            for (int i = 0; i < 2; i++)
            {
                cardlist[i].Effect = false;
            }
            int num1 = cardlist[0].Number > cardlist[1].Number ? cardlist[0].Number : cardlist[1].Number;
            int num2 = cardlist[0].Number > cardlist[1].Number ? cardlist[1].Number : cardlist[0].Number;
            if (!(num1 == 20 && num2 == 5)) for (int i = 0; i < HE_buffer.Length; i++) HE_buffer[i] = -1;
        }
        hand_effect = false;
    }
    IEnumerator HE1411(string tag)
    {
       StartCoroutine(HECommons(tag));
        if (tag.Contains("Player"))
        {
            PHtext.text = "生命循環";
            PHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                hand_effect = false;
                yield break;
            }
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "生命循環";
            EHtext.color = Color.cyan;
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                hand_effect = false;
                yield break;
            }
        }
        if (!(player.AD || enemy.AD)){
            int after = (player.HP + enemy.HP) / 2;
            player.HPChange(after - player.HP);
            enemy.HPChange(after - enemy.HP);
        }
        hand_effect = false;
    }
    IEnumerator HE74(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "王へ至る道";
            PHtext.color = Color.red;
            APlus[0] += 2;
            HEsetter(74, tag);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "王へ至る道";
            EHtext.color = Color.red;
            APlus[1] += 2;
            HEsetter(74, tag);
        }
        hand_effect = false;
    }
    IEnumerator HE98(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "森の賢者";
            PHtext.color = Color.blue;
            Card harmit = cardlist[0].Number > cardlist[1].Number ? cardlist[0] : cardlist[1];
            if(harmit.Number==9) harmit.Defense *= 2;
            for (int i = 0; i < 2; i++) cardlist[i + 2].Effect = false;
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "森の賢者";
            EHtext.color = Color.blue;
            Card harmit = cardlist[2].Number > cardlist[3].Number ? cardlist[2] : cardlist[3];
            if (harmit.Number == 9) harmit.Defense *= 2;
            for (int i = 0; i < 2; i++) cardlist[i].Effect = false;
        }
        hand_effect = false;
    }
    IEnumerator HE137(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "戦乱の英雄";
            PHtext.color = Color.red;
            AnchDef[0] += 2;
            DPlus[0] += 2;
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "戦乱の英雄";
            EHtext.color = Color.red;
            AnchDef[1] += 2;
            DPlus[1] += 2;
        }
        hand_effect = false;
    }
    IEnumerator HE172(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "星の導き";
            PHtext.color = Color.blue;
            player.AD = true;
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "星の導き";
            EHtext.color = Color.blue;
            enemy.AD = true;
        }
        hand_effect = false;
    }
    IEnumerator HE1914(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "生命満ちる大地";
            PHtext.color = Color.blue;
            player.HPChange(3);
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "生命満ちる大地";
            EHtext.color = Color.blue;
            enemy.HPChange(3);
        }
        hand_effect = false;
    }
    IEnumerator HE32(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "聖女の抱擁";
            PHtext.color = Color.blue;
            player.AD=true;
        }
        else if (tag.Contains("Enemy"))
        {
            EHtext.text = "聖女の抱擁";
            EHtext.color = Color.blue;
            enemy.AD = true;
        }
        hand_effect = false;
    }
    IEnumerator HE43(string tag)
    {
        StartCoroutine(HECommons(tag));
        yield return new WaitUntil(() => hand_effect);
        if (tag.Contains("Player"))
        {
            PHtext.text = "理想の王政";
            PHtext.color = Color.red;
            for(int i = 0; i < 2; i++)
            {
                if (cardlist[i].Number == 3 || cardlist[i].Number == 4)
                {
                    cardlist[i].Attack *= 3;
                    cardlist[i].Defense *= 3;
                }
                
            }
        }else if (tag.Contains("Enemy"))
        {
            EHtext.text = "理想の王政";
            EHtext.color = Color.red;
            for (int i = 2; i < 4; i++)
            {
                if (cardlist[i].Number == 3 || cardlist[i].Number == 4)
                {
                    cardlist[i].Attack *= 3;
                    cardlist[i].Defense *= 3;
                }
            }
        }
        yield return new WaitForSeconds(1.0f);
        hand_effect = false;
    }
    IEnumerator HECommons(string tag)
    {
        hand_effect = true;
        GameObject[] effeobj = new GameObject[2];
        GameObject eff = (GameObject)Resources.Load("Prefab/CircleEff");
        if (tag.Contains("Player"))
        {
            effeobj[0] = Instantiate(eff, player.Field[0].position, Quaternion.identity);
            effeobj[1] = Instantiate(eff, player.Field[1].position, Quaternion.identity);
            PHtext.enabled = true;
            HCount[0]++;
        }
        else if (tag.Contains("Enemy"))
        {
            effeobj[0] = Instantiate(eff, enemy.Field[0].position, Quaternion.identity);
            effeobj[1] = Instantiate(eff, enemy.Field[1].position, Quaternion.identity);
            EHtext.enabled = true;
            HCount[1]++;
        }
        yield return new WaitUntil(() => !hand_effect);
        yield return new WaitForSeconds(1.2f);
        for (int i = 0; i < 2; i++) Destroy(effeobj[i]);
        PHtext.enabled = false;
        EHtext.enabled = false;
        hand_processed = false;
    }
    //カード効果
    private void Cardeffect(int i,string tag)
    {
        effect_processed = true;
        if (i == 22) Fool(tag);
        else if (i == 1) StartCoroutine(Magician(tag));
        else if (i == 6) Lovers(tag);
        else if (i == 10) StartCoroutine(WheelofFortune(tag));
        else if (i == 12) HangedMan(tag);
        else if (i == 13) Death(tag);
        else if (i == 15) StartCoroutine(Devil(tag));
        else if (i == 16) Tower(tag);
        else if (i == 17) Star(tag);
        else if (i == 18) Moon(tag);
        else if (i == 19) Sun(tag);
        else if (i == 20) Judgement(tag);
        else if (i == 21) StartCoroutine(World(tag));
    }
    private void Fool(string tag)
    {
        if (tag.Contains("Player")) FoolFlag[0] = true;
        else if (tag.Contains("Enemy")) FoolFlag[1] = true;
        for (int i = 0; i < cardlist.Length; i++)
        {
            if (cardlist[i].Number != 22)
            {
                cardlist[i].Attack = 0;
                cardlist[i].Defense = 0;
                cardlist[i].Effect = false;
            }
        }
        effect_processed = false;
    }
    IEnumerator Magician(string tag)
    {
        if (tag.Contains("Player") && player.HandCardNum()>0)
        {
            if ((bool)HEgetter(121, tag))
            {
                SelectSetting(player.HandCardNum());
                CanselSet();
            }
            else if ((bool)HEgetter(91, tag)) SelectSetting(2);
            else SelectSetting(1);
            HSelSet(tag);
            yield return new WaitUntil(() => selected || selectCansel);
            manager.state = State.Battle;
            player.PlayerUIReset();
            if (selectName.Count == 0)
            {
                effect_processed = false;
                yield break;
            }
            for (int i = 0; i < selectName.Count; i++)
            {
                player.Hand2Deck(selectName[i]);
                yield return new WaitUntil(() => player.synch);
            }
            if ((bool)HEgetter(91, tag))
            {
                SelectSetting(2);
                SelPanelSet(player.tag, "Deck");
                yield return new WaitUntil(() => selected);
                manager.state = State.Battle;
                player.PlayerUIReset();
                for(int i = 0; i < selectName.Count; i++)
                {
                    player.DT2Hand(selectName[i], "Deck");
                    yield return new WaitUntil(() => player.synch);
                }
                player.DeckShuffle();
                yield return new WaitUntil(() => player.synch);
            }
            else
            {
                player.DeckShuffle();
                yield return new WaitUntil(() => player.synch);
                player.Draw(selectName.Count);
                yield return new WaitUntil(() => player.synch);
            }          
        }
        else if(tag.Contains("Enemy") && enemy.HandCardNum() > 0)
        {
            int count = 1;
            if ((bool)HEgetter(121, tag)) count = 3;
            else if ((bool)HEgetter(91, tag)) count = 2;
            List<string> list = new List<string>();
            for(int i = 0; i < enemy.Hand.Length; i++)
            {
                if (enemy.Hand[i].childCount != 0) list.Add(i.ToString());
            }
            list=list.OrderBy(i => System.Guid.NewGuid()).ToList();

            for (int i = 0; i < count; i++)
            {
                enemy.Hand2Deck(list[i]);
                yield return new WaitUntil(() => enemy.synch);
            }
            if ((bool)HEgetter(91, tag))
            {
                List<int> edeck = enemy.GetDeck();
                edeck = edeck.OrderBy(i => System.Guid.NewGuid()).ToList();
                for (int i = 0; i < count; i++)
                {
                    enemy.DT2Hand(num2arcana(edeck[i]), "Deck");
                    yield return new WaitUntil(() => enemy.synch);
                }
                enemy.DeckShuffle();
                yield return new WaitUntil(() => enemy.synch);
            }
            else
            {
                enemy.DeckShuffle();
                yield return new WaitUntil(() => enemy.synch);
                enemy.Draw(count);
                yield return new WaitUntil(() => enemy.synch);  
            }
        }
        effect_processed = false;
    }
    private void Lovers(string tag)
    {
        if (tag.Contains("Player")) player.HPChange(2);
        else if (tag.Contains("Enemy")) enemy.HPChange(2);
        effect_processed = false;
    }
    IEnumerator WheelofFortune(string tag)
    {
        if (tag.Contains("Player") && player.HandCardNum() > 0)
        {
            int num = player.HandCardNum();
            player.Hand2Deck("all");
            yield return new WaitUntil(() => player.synch);
            if ((bool)HEgetter(1710, tag))
            {
                SelectSetting(num);
                SelPanelSet(player.tag, "Deck");
                yield return new WaitUntil(() => selected);
                manager.state = State.Battle;
                player.PlayerUIReset();
                for(int i = 0; i < num; i++)
                {
                    player.DT2Hand(selectName[i], "Deck");
                    yield return new WaitUntil(() => player.synch);
                }
                player.DeckShuffle();
                yield return new WaitUntil(() => player.synch);
            }
            else
            {
                player.DeckShuffle();
                yield return new WaitUntil(() => player.synch);
                player.Draw(num);
                yield return new WaitUntil(() => player.synch);
            }
        }
        else if (tag.Contains("Enemy") && enemy.HandCardNum() > 0)
        {
            int num = enemy.HandCardNum();
            enemy.Hand2Deck("all");
            yield return new WaitUntil(() => enemy.synch);
            if ((bool)HEgetter(1710, tag))
            {
                List<int> edeck = enemy.GetDeck();
                edeck = edeck.OrderBy(i => System.Guid.NewGuid()).ToList();
                for (int i = 0; i < num; i++)
                {
                    enemy.DT2Hand(num2arcana(edeck[i]), "Deck");
                    yield return new WaitUntil(() => enemy.synch);
                }
                enemy.DeckShuffle();
                yield return new WaitUntil(() => enemy.synch);
            }
            else
            {
                enemy.DeckShuffle();
                yield return new WaitUntil(() => enemy.synch);
                enemy.Draw(num);
                yield return new WaitUntil(() => enemy.synch);
            }
        }
        effect_processed = false;
    }
    private void HangedMan(string tag)
    {
        if (tag.Contains("Player")) hanged[0] = 1;
        else if (tag.Contains("Enemy")) hanged[1] = 1;
        effect_processed = false;
    }
    private void Death(string tag)
    {
        if (tag.Contains("Player")) player.HPChange(-2);
        else if (tag.Contains("Enemy")) enemy.HPChange(-2);
        effect_processed = false;
    }
    IEnumerator Devil(string tag)
    {
        if(player.HandCardNum() > 0 &&enemy.HandCardNum() > 0)
        {
            if (tag == "Player")
            {
                CSkoisi(tag);
                yield return new WaitUntil(() => enemy.CSbk);
                if (enemy.koisiF)
                {
                    enemy.koisiF = false;
                    effect_processed = false;
                    yield break;
                }
            }
            else if (tag == "Enemy")
            {
                CSkoisi(tag);
                yield return new WaitUntil(() => player.CSbk);
                if (player.koisiF)
                {
                    player.koisiF = false;
                    effect_processed = false;
                    yield break;
                }
            }
            int nump = player.HandCardNum();
            int nume = enemy.HandCardNum();
            player.Hand2Deck("all");
            enemy.Hand2Deck("all");
            yield return new WaitUntil(() => player.synch && enemy.synch);
            player.DeckShuffle();
            enemy.DeckShuffle();
            yield return new WaitUntil(() => player.synch && enemy.synch);
            player.Draw(nump);
            enemy.Draw(nume);
            yield return new WaitUntil(() => player.synch && enemy.synch);
            
        }
        effect_processed = false;
    }
    private void Tower(string tag)
    {
        if (tag.Contains("Player")) player.HPChange(-2);
        else if (tag.Contains("Enemy")) enemy.HPChange(-2);
        effect_processed = false;
    }
    private void Star(string tag)
    {
        if (tag.Contains("Player")) player.HPChange(2);
        else if (tag.Contains("Enemy")) enemy.HPChange(2);
        effect_processed = false;
    }
    private void Moon(string tag)
    {
        if (tag.Contains("Player"))
        {
            for (int i = 2; i < 4; i++)
            {
                if (cardlist[i].Number != 21 && cardlist[i].Number != 22 && cardlist[i].Number != 18) cardlist[i].Effect = false;
            }
        }
        else if (tag.Contains("Enemy"))
        {
            for (int i = 0; i < 2; i++)
            {
                if (cardlist[i].Number != 21 && cardlist[i].Number != 22 && cardlist[i].Number != 18) cardlist[i].Effect = false;
            }
        }
        effect_processed = false;
    }
    private void Sun(string tag)
    {
        if (tag.Contains("Player"))
        {
            if ((bool)HEgetter(2019, tag)) AnchDef[0] += 6;
            else AnchDef[0] += 3;
        }
        else if (tag.Contains("Enemy"))
        {
            if ((bool)HEgetter(2019, tag)) AnchDef[1] += 6;
            else AnchDef[1] += 3;
        }
        effect_processed = false;
    }
    private void Judgement(string tag)
    {
        if (tag.Contains("Player"))
        {
            if ((bool)HEgetter(2019, tag)) player.HPChange(8);
            else player.HPChange(4);
        }
        else if (tag.Contains("Enemy"))
        {
            if ((bool)HEgetter(2019, tag)) enemy.HPChange(8);
            else enemy.HPChange(4);
        }
        effect_processed = false;
    }
    IEnumerator World(string tag)
    {
        if (tag.Contains("Player"))
        {
            CSkoisi(tag);
            yield return new WaitUntil(() => enemy.CSbk);
            if (enemy.koisiF)
            {
                enemy.koisiF = false;
                effect_processed = false;
                yield break;
            }
            enemy.HPChange(-7);
            player.HPChange(3);
        }
        else if (tag.Contains("Enemy"))
        {
            CSkoisi(tag);
            yield return new WaitUntil(() => player.CSbk);
            if (player.koisiF)
            {
                player.koisiF = false;
                effect_processed = false;
                yield break;
            }
            player.HPChange(-7);
            enemy.HPChange(3);
        }
        effect_processed = false;
    }
    //CSシリーズ
    private bool[] CSFlag = new bool[2];//キャラクタースキルの仕様の有無
    public bool CSflag(string tag)
    {
        if (tag == "Player") return CSFlag[0];
        else if (tag == "Enemy") return CSFlag[1];
        return false;
    }
    //sunの処理前、暗闇の後にいれたので破天荒の処理をきちんとすれば、守備力と攻撃力をプラスするだけで解決できると思う。
    public void CSmarisa(string tag)
    {
        if (tag == "Player") APlus[0] += 4;
        else if (tag == "Enemy") APlus[1] += 4;
    }
    public void CSreimu(string tag)
    {
        if (tag == "Player"&& !(bool)HEgetter(195, "Enemy")) DPlus[0] += 4;
        else if (tag == "Enemy"&& !(bool)HEgetter(195, "Player")) DPlus[1] += 4;
    }
    public void CSfran(string tag)
    {
        if (tag.Contains("Player")) enemy.HPChange(-3);
        else if (tag.Contains("Enemy")) player.HPChange(-3);
    }
    public void CSsatori(string tag) { StartCoroutine(cSsatori(tag)); }
    IEnumerator cSsatori(string tag)
    {
        if (tag.Contains("Player"))
        {
            CSFlag[0] = false;
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < enemy.Hand.Length; i++)
            {
                if (enemy.Hand[i].childCount != 0)
                {
                    list.Add(enemy.Hand[i].GetChild(0).gameObject);
                }
            }
            enemy.Flip(list);
            yield return new WaitUntil(() => enemy.flipb);
            enemy.satoriF = manager.GetBattleTurn() + 2;
            CSFlag[0] = true;
        }
        else if (tag.Contains("Enemy"))
        {
            CSFlag[1] = false;
            player.satoriF = manager.GetBattleTurn() + 2;
            CSFlag[1] = true;
        }
    }
    public void CSyoumu(string tag)
    {
        if (tag == "Player") youmu[0] = true;
        else if (tag == "Enemy") youmu[1] = true;
    }
    public void CSkoisi(string tag)
    {
        if (tag == "Player")
        {
            enemy.koisiF = false;
            enemy.CSkillProcess(CSkillType.EffectA);
        }
        else if (tag == "Enemy")
        {
            player.koisiF = false;
            player.CSkillProcess(CSkillType.EffectA);
        }
    }
    //Player と　Enemyの中継役のメソッド
    public object GetCharacterData(string type, string tag)
    {
        if (type == "CSbk")
        {
            if (tag == "Player") return enemy.CSbk;
            else if (tag == "Enemy") return player.CSbk;
        }
        if (type == "koisiF")
        {
            if (tag == "Player") return enemy.koisiF;
            else if (tag == "Enemy") return player.koisiF;
        }
        return null;
    }
    public bool youmuflag()
    {
        return youmu[0];
    }
    //Think系はエネミーしか使わない
    public bool reimuThink()
    {
        if (manager.GetBattleTurn() == 10) return true;
        if (!IgnoreD[0] && !(bool)HEgetter(195, "Player") && !(AnchDef[0] > advalue[3] + 4) && e_dmg < 0)
        {
            if (!enemy.AD && !(bool)HEgetter(113, "Enemy"))
            {
                int damege = -e_dmg;
                if (APlus[0] > 0) damege += APlus[0];

                if (AnchDef[0] > advalue[3]) damege += AnchDef[0];
                if (damege > 0 && hanged[0] == 2) damege += 2;
                if ((bool)HEgetter(74, "Player") && damege > 4) return true;
                else if ((bool)HEgetter(74, "Player") && damege > 3 && Random.value > 0.80f) return true;

                if (enemy.HP <= damege) return true;
                if (damege > 4 && Random.value > 0.2f)
                {
                    return true;
                }
            }
        }

        return false;
    }
    public bool marisaThink()
    {
        if (manager.GetBattleTurn() == 10) return true;
        if (!player.AD && !(bool)HEgetter(113, "Player"))
        {
            int damege = advalue[1] + DPlus[0] - AnchDef[1] - advalue[2] - APlus[1];
            if (damege >= 4) return false;
            if (IgnoreD[1] || (bool)HEgetter(195, "Enemy"))
            {
                if (player.HP < -p_dmg + 4) return true;
                else if (Random.value > 0.3f) return true;
                return false;
            }
            if ((bool)HEgetter(74, "Enemy"))
            {
                if (advalue[1] + DPlus[0] - AnchDef[1] < advalue[2] + APlus[1]) return true;
                else if (advalue[1] + DPlus[0] - AnchDef[1] < advalue[2] + APlus[1] + 2 && Random.value > 0.6f) return true;
            }
            if (hanged[1] == 2 && damege < 0) damege -= 2;
            if (player.HP < -(damege - 4)) return true;
            if (Random.value > 0.95f) return true;
        }
        return false;
    }
    public bool youmuThink()
    {
        Card[] array = new Card[2];
        for (int i = 0; i < 2; i++) array[i] = arcana2Card(enemy.GetFieldCard(i), player.tag);
        int num1 = array[0].Number > array[1].Number ? array[0].Number : array[1].Number;
        int num2 = array[0].Number > array[1].Number ? array[1].Number : array[0].Number;
        if (num1 == 22 || (num1 == 18 || num2 == 18)) return false;
        if ((num1 == 21 && num2 == 5) || (num1 == 20 && num2 == 4) || (num1 == 20 && num2 == 5)
            || (num1 == 18 && num2 == 16) || (num1 == 10 && num2 == 6)) return false;
        float baseRate = manager.GetBattleTurn() < 5 ? 0.5f : 0.5f + 0.1f * (manager.GetBattleTurn() - 5);
        if (player.GetHEbuf(182))
        {
            Card[] array1 = new Card[2];
            for (int i = 0; i < 2; i++) array[i] = arcana2Card(player.GetFieldCard(i), player.tag);
            int nump1 = array1[0].Number > array1[1].Number ? array1[0].Number : array1[1].Number;
            int nump2 = array1[0].Number > array1[1].Number ? array1[1].Number : array1[0].Number;
            if (HEtable[nump1, nump2] && Random.value < baseRate) return true;
            if (nump1 == 22 || num1 == 21) return true;
            if (num2 >= 17 && Random.value < baseRate + 0.1f) return true;
            if (num1 >= 17 && Random.value < baseRate - 0.3f) return true;
            return false;
        }
        for (int i = 0; i < 2; i++)
        {
            if (player.Field[i].childCount != 0)
            {
                if (player.Field[i].GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite != manager.cardback)
                {
                    Card card = arcana2Card(player.GetFieldCard(i), player.tag);
                    if (card.Number == 22 || card.Number == 21) return true;
                    if (card.Number >= 17 && Random.value < baseRate - 0.2f) return true;
                }
            }
        }
        if (manager.GetBattleTurn() > 5)
        {
            if (baseRate > Random.value) return true;
        }
        return false;
    }
    public bool sakuyaThink()
    {
        bool worldf = true;
        List<string> tra = enemy.Trash.GetListTrashCard();
        foreach (string na in tra)
        {
            if (na.Contains("World"))
            {
                worldf = false;
                break;
            }
        }
        if (worldf)
        {
            Card[] arrayE = new Card[2];
            for (int i = 0; i < 2; i++) arrayE[i] = arcana2Card(enemy.GetFieldCard(i), enemy.tag);
            int num1e = arrayE[0].Number > arrayE[1].Number ? arrayE[0].Number : arrayE[1].Number;
            int num2e = arrayE[0].Number > arrayE[1].Number ? arrayE[1].Number : arrayE[0].Number;
            Card[] arrayP = new Card[2];
            for (int i = 0; i < 2; i++) arrayP[i] = arcana2Card(player.GetFieldCard(i), player.tag);
            int num1p = arrayP[0].Number > arrayP[1].Number ? arrayP[0].Number : arrayP[1].Number;
            int num2p = arrayP[0].Number > arrayP[1].Number ? arrayP[1].Number : arrayP[0].Number;
            if (num1e == 21 && ((num1p == 22 && num2p != 15) || youmu[0] || (num1p == 20 && num2p == 5) || (num1p == 18 && num2p == 16) ||
                (num1p == 17 && num1p == 2) || (num1p == 3 && num2p == 2) || (num1p == 10 && num2p == 6))) return true;
            if (num1p == 14 && num2p == 9)
            {
                for (int i = 0; i < enemy.Hand.Length; i++)
                {
                    if (enemy.Hand[i].childCount != 0)
                    {
                        if (enemy.Hand[i].GetChild(0).name == "World" && manager.GetBattleTurn() > 5)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("見えている状態での最善手を探す。");
        }
        return false;
    }
    //その他
    public bool isArie(Card card)
    {
        if (card.Number >= 17) return true;
        else return false;
    }
    private void adtextSet()
    {
        for(int i = 0; i < 4; i++)
        {
            adtext[i] = Instantiate((GameObject)Resources.Load("Prefab/Text/pa_Text"));
            adtext[i].name = i.ToString();
            if (i < 2) adtext[i].transform.SetParent(player.Field[i], false);
            else adtext[i].transform.SetParent(enemy.Field[i-2], false);
            if (i % 2 == 0)
            {
                adtext[i].transform.localPosition = new Vector3(1.0f, 0.0f, 0.0f);
                adtext[i].GetComponent<Text>().color = Color.red;
            }
            else
            {
                adtext[i].transform.localPosition = new Vector3(-1.0f, 0.0f, 0.0f);
                adtext[i].GetComponent<Text>().color = Color.blue;
            }
            adtext[i].transform.SetParent(UIPanel.transform, true);
        }
        adtextTe();
    }
    private void adtextTe()
    {
        for(int i=0;i<4;i++) adtext[i].GetComponent<Text>().text = advalue[i].ToString();
    }
    private void effector(int i)
    {
        Vector3 effectpos = Vector3.zero;
        if (i == 0) effectpos = player.Field[0].transform.position;
        else if (i == 1) effectpos = player.Field[1].transform.position;
        else if (i == 2) effectpos = enemy.Field[0].transform.position;
        else if (i == 3) effectpos = enemy.Field[1].transform.position;
        effectpos.z -= 0.01f;
        GameObject eff = (GameObject)Resources.Load("Prefab/CircleEff");
        effect = Instantiate(eff, effectpos, Quaternion.identity);
    }
    public void SelectedAction(string str, GameObject selector)
    {
        if (str == "No")
        {
            selectCansel = true;
            return;
        }
        for (int i = 0; i < selectName.Count; i++)
        {
            if (selectName[i] == str) return;
        }
        selectName.Add(str);
        selector.SetActive(false);
        if (selectName.Count == selectCount) selected = true;
    }
    private void SelectSetting(int num)
    {
        selected = false;
        selectCansel = false;
        selectCount = num;
        selectName = new List<string>();
        manager.state = State.Select;
    }
    
    public void HSelSet(string tag)
    {
        Transform[] handpos = player.Hand;
        Vector3 pos = new Vector3(0f, -1.05f, 0f);
        if (tag == "Enemy")
        {
            handpos = enemy.Hand;
            pos = new Vector3(0f, 1.05f, 0f);
        }
        for (int i = 0; i < handpos.Length; i++)
        {
            if (handpos[i].transform.childCount != 0)
            {
                GameObject htd = Instantiate((GameObject)Resources.Load("Prefab/Button/cardButton"));
                htd.name = "htd";
                htd.transform.SetParent(handpos[i], false);
                htd.transform.localPosition = pos;
                htd.transform.SetParent(manager.UIpanel, true);
                htd.transform.localRotation = Quaternion.identity;
                htd.transform.localScale = new Vector3(1f, 1f, 1f);
                htd.transform.GetChild(0).GetComponent<Text>().text = "SELECT";
                string temp = i.ToString();
                htd.GetComponent<Button>().onClick.AddListener(() => { SelectedAction(temp, htd); });
                htd.GetComponent<Button>().onClick.AddListener(() => { player.SelectedAction(temp, htd); });
                player.buffer.Add(htd);
            }
        }
    }
    public void HFSelSet()
    {
        Transform[] handp = enemy.Hand;
        Vector3 pos = new Vector3(0f, 1f, 0f);
        for (int i = 0; i < handp.Length; i++)
        {
            if (handp[i].childCount != 0)
            {
                if (!((handp[i].GetChild(0).name.Contains("Fool")
                    || handp[i].GetChild(0).name.Contains("World")) && manager.GetBattleTurn() < 5))
                {
                    GameObject htd = Instantiate((GameObject)Resources.Load("Prefab/Button/cardButton"));
                    htd.name = "htd";
                    htd.transform.SetParent(handp[i], false);
                    htd.transform.localPosition = pos;
                    htd.transform.SetParent(manager.UIpanel, true);
                    htd.transform.localRotation = Quaternion.identity;
                    htd.transform.localScale = new Vector3(1f, 1f, 1f);
                    htd.transform.GetChild(0).GetComponent<Text>().text = "SELECT";
                    string temp = i.ToString();
                    htd.GetComponent<Button>().onClick.AddListener(() => { SelectedAction(temp, htd); });
                    htd.GetComponent<Button>().onClick.AddListener(() => { player.SelectedAction(temp, htd); });
                    player.buffer.Add(htd);
                }
            }
        }
    }
    public void SelPanelSet(string tag, string selname)
    {
        GameObject selpane = Instantiate((GameObject)Resources.Load("Prefab/MenuBar/SelectivePanel"));
        SelectivePanel selp = selpane.GetComponent<SelectivePanel>();
        if (tag == "Player")
        {
            if (selname == "Trash")
            {
                selp.SetTrash(player.Trash.GetListTrashCard());
                selpane.transform.SetParent(player.Trash.transform, false);
            }
            else if (selname == "Deck")
            {
                selp.SetDeck(player.GetDeck());
                selpane.transform.SetParent(player.Deckzone, false);
            }
        }
        else if (tag == "Enemy")
        {
            if (selname == "Trash")
            {
                selp.SetTrash(enemy.Trash.GetListTrashCard());
                selpane.transform.SetParent(enemy.Trash.transform, false);
            }
            else if (selname == "Deck")
            {
                selp.SetDeck(enemy.GetDeck());
                selpane.transform.SetParent(enemy.Deckzone, false);
            }
        }
        selpane.transform.localPosition = new Vector3(0f, 0f, 0f);
        selp.panel0 = player.cardPanel;
        selp.ListSet();
        selpane.name = "tmenu";
        selpane.transform.SetParent(manager.UIpanel, true);
        selpane.transform.localRotation = Quaternion.identity;
        selpane.transform.localScale = new Vector3(1f, 1f, 1f);
        player.buffer.Add(selpane);
    }
    public void AgreeSet()
    {
        GameObject ag = Instantiate((GameObject)Resources.Load("Prefab/Button/cardButton"));
        ag.name = "ag";
        ag.transform.SetParent(manager.UIpanel, false);
        ag.transform.localPosition = new Vector3(0f, -70f, 0f);
        ag.transform.GetChild(0).GetComponent<Text>().text = "OK";
        ag.GetComponent<Button>().onClick.AddListener(() => { SelectedAction("Yes", ag); });
        ag.GetComponent<Button>().onClick.AddListener(() => { player.SelectedAction("Yes", ag); });
        player.buffer.Add(ag);
    }
    public void CanselSet()
    {
        GameObject cansel = Instantiate((GameObject)Resources.Load("Prefab/Button/cardButton"));
        cansel.name = "cansel";
        cansel.transform.SetParent(manager.UIpanel, false);
        cansel.transform.localPosition = new Vector3(0f, -134f, 0f);
        cansel.transform.GetChild(0).GetComponent<Text>().text = "CANSEL";
        cansel.GetComponent<Button>().onClick.AddListener(() => { player.SelectedAction("No", cansel); });
        cansel.GetComponent<Button>().onClick.AddListener(() => { SelectedAction("No", cansel); });
        player.buffer.Add(cansel);
    }
    public void FSelSet()
    {
        Vector3[] fool = { new Vector3(258f, 85f, 0f), new Vector3(-258f, 85f, 0f) };
        for (int i = 0; i < 2; i++)
        {
            GameObject f = Instantiate((GameObject)Resources.Load("Prefab/Button/cardButton"));
            f.name = "select";
            f.transform.SetParent(manager.UIpanel, false);
            f.transform.localPosition = fool[i];
            f.transform.GetChild(0).GetComponent<Text>().text = "SELECT";
            string temp = i.ToString();
            f.GetComponent<Button>().onClick.AddListener(() => { player.SelectedAction(temp, f); });
            f.GetComponent<Button>().onClick.AddListener(() => { SelectedAction(temp, f); });
            player.buffer.Add(f);
        }
    }
    public void DamegePop(int value, string tag)
    {
        GameObject txd = Instantiate((GameObject)Resources.Load("Prefab/Text/Damege"));
        txd.name = "pop" + Random.value.ToString();
        txd.transform.SetParent(manager.UIpanel, false);
        if (tag == "Player") txd.transform.localPosition = new Vector3(-380f, -300f, 0f);
        else if (tag == "Enemy") txd.transform.localPosition = new Vector3(380f, 300f, 0f);
        txd.GetComponent<Text>().text = value.ToString();
        if (value < 0) txd.GetComponent<Text>().color = Color.red;
        else if (value > 0) txd.GetComponent<Text>().color = Color.green;
        else if (value == 0) txd.GetComponent<Text>().color = Color.black;
        txd.GetComponent<TextPop>().Play();
    }
    private bool ListExist<T>(IEnumerable<T> list, T search)
    {
        foreach (var item in list)
        {
            if (item.Equals(search)) return true;
        }
        return false;
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
    public static int arcana2num(string name)
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
    public int CurrentTimeSeed()
    {
        System.DateTime dt = System.DateTime.Now;
        return dt.Day + dt.Hour + dt.Minute + dt.Second + dt.Millisecond;
    }
}

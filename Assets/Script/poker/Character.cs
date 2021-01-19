using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

/*エネミーとプレイヤーに継承させるクラス
 * かなりごちゃってる
 */

public abstract class Character:MonoBehaviour
{
    protected const int handmax = 6;
    const int fieldmax = 2;
    const float hpsrate = 0.19f;
    const float hpprate = 0.131f;
    const float gaugelimitp = -4.06f;

    public Transform[] Hand=new Transform[handmax];
    public Transform[] Field=new Transform[fieldmax];
    public Trashzone Trash;
    public Transform Deckzone;
    public CardManager manager;
    public Transform[] hpgauge=new Transform[2];
    public Text hptext;
    public Image graphic; //Character
    protected Animation anim;

    public BattleTurn battle;
    public int Decktop { get; set; }
    public int FieldCount { get; set; }
    public int HP { get; set; }
    public bool AD { get; set; }
    private bool[] HEbuf { get; set; }
    private bool locked = false;
    protected List<int> Deck = new List<int> { 22, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19,20,21  };

    public bool flipb { get; set; } //これは同期用にいると思う
    public bool synch { get; set; } //同期用
    public bool set { get; set; }//割り込みでやるやつ
    public bool CSb { get; set; }
    public bool CSbk { get; set; }
    public bool mariganb { get; set; }
    public bool CSFlag { get; set; }
    public CutIn cutin;
    protected List<string> selectName = new List<string>();
    protected int selectCount = 0;
    protected bool selected;
    protected bool selectCansel = false;
    public int satoriF { get; set; }
    public bool koisiF { get; set; }
    const float h_offset = 1.6f;
    const float f_offset = 4f;
    Dictionary<string, Color> color = new Dictionary<string, Color>()
    {
        { "Green" ,new Color(0.0f,0.8f,0.0f,0.8f)},
        {"Yellow",new Color(0.8f,0.8f,0.0f,0.8f) },
        {"Red",new Color(0.8f,0.0f,0.0f,0.8f) },
    };

    private void Awake()
    {
        HEbuf = new bool[8];
        Decktop = 0;
        FieldCount = 0;
        AD = false;
        HP = 20;
        
        anim = GetComponent<Animation>();
        for (int i = 0; i < handmax; i++)
        {
            GameObject zone = Instantiate((GameObject)Resources.Load("Prefab/HandZone"));
            zone.tag = tag;
            zone.name = "HandZone" + i.ToString();
            zone.transform.parent = transform;
            Vector3 pos = new Vector3(-4f + i * h_offset, -4f, 0f);
            zone.transform.localRotation = Quaternion.identity;
            zone.transform.localPosition = pos;
            zone.GetComponent<zoneScript>().number = i.ToString();
            Hand[i] = zone.transform;
        }
        for(int i = 0; i < fieldmax; i++)
        {
            GameObject zone = Instantiate((GameObject)Resources.Load("Prefab/FieldZone"));
            zone.tag = tag;
            zone.name = "FieldZone" + i.ToString();
            zone.transform.parent = transform;
            foreach (Transform child in zone.transform)
            {
                if (child.GetComponent<BoxCollider2D>() != null)
                {
                    child.tag = tag;
                    child.name = "Field" + i.ToString();
                    child.GetComponent<zoneScript>().number = i.ToString();
                    Field[i] = child;
                }
            }
            Vector3 pos = new Vector3(-2f + i * f_offset, -1.1f, 0f);
            zone.transform.localRotation = Quaternion.identity;
            zone.transform.localPosition = pos;
        }
    }
    //名前の変更
    public void Rename()
    {
        hptext.text = "HP : " + HP.ToString();
    }
        //カード操作
    /*デッキをshuffle
     * synchで同期を取る
     */
    public void DeckShuffle() { StartCoroutine("deckShuffle"); }
    IEnumerator deckShuffle()
    {
        synch = false;
        Deck = Deck.OrderBy(i => System.Guid.NewGuid()).ToList();
        GameObject[] list = new GameObject[4];
        //ここからshuffleのanimation製作
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;
        for (int i = 0; i < 4; i++)
        {
            list[i] = Instantiate((GameObject)Resources.Load("Prefab/Card"),Deckzone.position,Quaternion.identity,Deckzone);
            list[i].name = "Card" + i.ToString();
            list[i].AddComponent<Animation>();
            list[i].transform.localRotation = Quaternion.identity;
            Keyframe key0 = new Keyframe(0.0f, 0.0f);
            Keyframe key2 = new Keyframe(0.5f, 0.0f);
            if (i == 0)
            {
                Keyframe key1 = new Keyframe(0.25f, 3.5f);
                AnimationCurve curvey = new AnimationCurve(key0, key1, key2);
                string path = GetHierarchyPath(list[i].transform);
                clip.SetCurve(path, typeof(Transform), "localPosition.y", curvey);
            } else if (i == 1)
            {
                Keyframe key1 = new Keyframe(0.25f, 2.0f);
                AnimationCurve curvex = new AnimationCurve(key0, key1, key2);
                string path = GetHierarchyPath(list[i].transform);
                clip.SetCurve(path, typeof(Transform), "localPosition.x", curvex);
            } else if (i == 2)
            {
                Keyframe key1 = new Keyframe(0.25f, -2.0f);
                AnimationCurve curvex = new AnimationCurve(key0, key1, key2);
                string path = GetHierarchyPath(list[i].transform);
                clip.SetCurve(path, typeof(Transform), "localPosition.x", curvex);
            } else if (i == 3)
            {
                Keyframe key1 = new Keyframe(0.25f, -3.5f);
                AnimationCurve curvey = new AnimationCurve(key0, key1, key2);
                string path = GetHierarchyPath(list[i].transform);
                clip.SetCurve(path, typeof(Transform), "localPosition.y", curvey);
            }
        }
        anim.AddClip(clip, clip.name);
        anim.Play(clip.name);
        yield return new WaitUntil(() => !anim.isPlaying);
        for (int i = 0; i < list.Length; i++) Destroy(list[i]);
        synch = true;
    }

    /*手札をshuffle
     * カードの位置を親を変えてそれぞれ移動
     * 同期はsynch
     * 基本的にターン開始時に呼び出される
     */
    public void HandShuffle() { StartCoroutine("handShuffle"); }
    IEnumerator handShuffle()
    {
        synch = false;
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;
        int[] after = new int[handmax];
        Transform[] card = new Transform[handmax];
        for (int i = 0; i < handmax; i++) {
            after[i] = i;
            if (Hand[i].childCount != 0) card[i] = Hand[i].GetChild(0);
            else card[i] = null;
        }
        after = after.OrderBy(i => System.Guid.NewGuid()).ToArray();

        for(int i = 0; i < handmax; i++)
        {
            if (card[i] != null)
            {
                card[i].parent = Hand[after[i]];
                Keyframe key0 = new Keyframe(0.0f, card[i].localPosition.x);
                Keyframe key1 = new Keyframe(0.25f, card[i].localPosition.x - Hand[i].localPosition.x);
                Keyframe key2 = new Keyframe(0.5f, 0.0f);
                AnimationCurve curvex = new AnimationCurve(key0, key1, key2);
                string path = GetHierarchyPath(card[i]);
                clip.SetCurve(path, typeof(Transform), "localPosition.x", curvex);
            }
        }
        anim.AddClip(clip, clip.name);
        anim.Play(clip.name);
        yield return new WaitUntil(() => !anim.isPlaying);
        synch = true;
    }

    /*ドロー
     * numでドロー枚数
     * 同期はsynch
     */
    public void Draw(int num) { StartCoroutine(draw(num)); }
    IEnumerator draw(int num)
    {
        AnimationClip clip = new AnimationClip();
        List<GameObject> clist = new List<GameObject>();
        clip.legacy = true;
        synch = false;
        //さとりの効果終了
        if (satoriF - manager.GetBattleTurn() == 0 && manager.GetBattleTurn() > 0)
        {
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < Hand.Length; i++)
            {
                if (Hand[i].childCount != 0)
                {
                    list.Add(Hand[i].GetChild(0).gameObject);
                }
            }
            Flipura(list);
            yield return new WaitUntil(() => flipb);
        }
        if (Deck.Count >= num&& num!=0)
        {
           
            for (int j = 0; j < num; j++)
            {
                for (int i = 0; i < Hand.Length; i++)
                {
                    if (Hand[i].childCount == 0)
                    {
                        GameObject obj
                            = Instantiate((GameObject)Resources.Load("Prefab/Card"), Deckzone.position, Quaternion.identity, Hand[i]);
                        obj.tag = this.tag;
                        obj.name = num2arcana(Deck[Deck.Count - 1]);
                        obj.transform.localRotation = Quaternion.identity;
                        clist.Add(obj);
                        AnimationCurve curvex = AnimationCurve.EaseInOut(0.0f, obj.transform.localPosition.x, 0.2f, 0.0f);
                        AnimationCurve curvey = AnimationCurve.EaseInOut(0.0f, obj.transform.localPosition.y, 0.2f, 0.0f);
                        string path = GetHierarchyPath(obj.transform);
                        clip.SetCurve(path, typeof(Transform), "localPosition.x", curvex);
                        clip.SetCurve(path, typeof(Transform), "localPosition.y", curvey);
                        Deck.RemoveAt(Deck.Count - 1);
                        break;
                    }
                }
            }
            anim.AddClip(clip, clip.name);
            anim.Play(clip.name);
            yield return new WaitUntil(() => !anim.isPlaying);
            if (tag.Contains("Player") || GetHEbuf(2221) || GetHEbuf(182) || (satoriF - manager.GetBattleTurn()) > 0)
            {
                Flip(clist);
                yield return new WaitUntil(() => flipb);
            }
        }
        if (locked)
        {
            locked = false;
            for (int i = 0; i < Field.Length; i++) Field[i].GetComponent<zoneScript>().Locked = false;
            for (int i = 0; i < Hand.Length; i++) Hand[i].GetComponent<zoneScript>().Locked = false;
        }
        if (manager.state == State.Set && GetHEbuf(156))
        {
            battle.HE156_CharacterBack(tag);
            yield return new WaitUntil(() => !GetHEbuf(156));
        }
        if (manager.state == State.Set && GetHEbuf(186))
        {
            battle.HE186_CharacterBack(tag);
            yield return new WaitUntil(() => !GetHEbuf(186));
        }
        synch = true;
    }

    /*Fieldのcardをtrash
     * 基本的にターンの終わりに呼び出される
     * 同期はsynch
     */
    public void CardTrash() { StartCoroutine("cardTrash"); }
    IEnumerator cardTrash()
    {
        synch = false;
        List<Transform> list = new List<Transform>();
        for (int i = 0; i < Field.Length; i++)
        {
            if (Field[i].childCount != 0)
            {
                list.Add(Field[i].GetChild(0));
            }
        }
        CardMovementD(list.ToArray(), Trash.transform);
        yield return new WaitUntil(() => !anim.isPlaying);
        for (int i = 0; i < list.Count; i++) Trash.AddTrash(list[i]);
        synch = true;
    }
    
    /*引数を手札に加える
     * 敵のカードを取るのに使う
     * 同期はset
     */
    public void Handset(Transform card) { StartCoroutine(handSet(card)); }
    IEnumerator handSet(Transform card)
    {
        set = false;
        for (int i = 0; i < Hand.Length; i++)
        {
            if (Hand[i].transform.childCount == 0)
            {
                CardMovement(card, Hand[i]);
                break;
            }
        }
        yield return new WaitUntil(() => !anim.isPlaying);
        card.localRotation = Quaternion.identity;
        card.gameObject.tag = tag;
        if (GetHEbuf(2221) || GetHEbuf(182) || (satoriF - manager.GetBattleTurn()) > 0)
        {
            Flip(new List<GameObject> { card.gameObject });
            yield return new WaitUntil(() => flipb);
        }
        set = true;
    }
    /*引数を場にだす
     * 敵のカードをだsのに使う
     * 同期はset
     */
    public void Fieldset(Transform card) { StartCoroutine(fieldSet(card)); }
    IEnumerator fieldSet(Transform card)
    {
        set = false;
        for (int i = 0; i < Field.Length; i++)
        {
            if (Field[i].transform.childCount == 0)
            {
                CardMovement(card, Field[i]);
                Field[i].GetComponent<zoneScript>().Locked = true;
                FieldCount++;
                locked = true;
                break;
            }
        }
        yield return new WaitUntil(() => !anim.isPlaying);
        set = true;
    }
    
    /*引数のものをひっくり返す
     * 同期はflipb
     * これを外で使っているかが気になる
     */
    public void Flip(List<GameObject> cardlist) {StartCoroutine(flip(cardlist)); }
    IEnumerator flip(List<GameObject> cardlist)
    {
        flipb = false;
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;
        for(int i = 0; i < cardlist.Count; i++)
        {
            Keyframe key0 = new Keyframe(0.0f, cardlist[i].transform.localScale.x);
            Keyframe key1 = new Keyframe(0.1f, 0.0f);
            Keyframe key2 = new Keyframe(0.2f, cardlist[i].transform.localScale.x);
            
            AnimationCurve curves = new AnimationCurve(key0, key1, key2);
            AnimationCurve curvesy = AnimationCurve.EaseInOut(0.0f, cardlist[i].transform.localScale.y, 0.2f, cardlist[i].transform.localScale.y);
            AnimationCurve curvesz = AnimationCurve.EaseInOut(0.0f, cardlist[i].transform.localScale.z, 0.2f, cardlist[i].transform.localScale.z);
            string path = GetHierarchyPath(cardlist[i].transform);
            clip.SetCurve(path, typeof(Transform), "localScale.x", curves);
            clip.SetCurve(path, typeof(Transform), "localScale.y", curvesy);
            clip.SetCurve(path, typeof(Transform), "localScale.z", curvesz);

            AnimationEvent animEvent = new AnimationEvent();
            animEvent.functionName = "cardChange";
            animEvent.objectReferenceParameter = cardlist[i];
            animEvent.time = 0.1f;
            clip.AddEvent(animEvent);
        }
        anim.AddClip(clip, clip.name);
        anim.Play(clip.name);
        yield return new WaitUntil(() => !anim.isPlaying);
        flipb = true;
    }
    //flipでeventで使う
    private void cardChange(GameObject obj)
    {
        obj.GetComponent<SpriteRenderer>().sprite = manager.arcana[arcana2num(obj.name)];
    }
    public void Flipura(List<GameObject> cardlist) { StartCoroutine(flipura(cardlist)); }
    IEnumerator flipura(List<GameObject> cardlist)
    {
        flipb = false;
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;
        for (int i = 0; i < cardlist.Count; i++)
        {
            Keyframe key0 = new Keyframe(0.0f, cardlist[i].transform.localScale.x);
            Keyframe key1 = new Keyframe(0.1f, 0.0f);
            Keyframe key2 = new Keyframe(0.2f, cardlist[i].transform.localScale.x);

            AnimationCurve curves = new AnimationCurve(key0, key1, key2);
            AnimationCurve curvesy = AnimationCurve.EaseInOut(0.0f, cardlist[i].transform.localScale.y, 0.2f, cardlist[i].transform.localScale.y);
            AnimationCurve curvesz = AnimationCurve.EaseInOut(0.0f, cardlist[i].transform.localScale.z, 0.2f, cardlist[i].transform.localScale.z);
            string path = GetHierarchyPath(cardlist[i].transform);
            clip.SetCurve(path, typeof(Transform), "localScale.x", curves);
            clip.SetCurve(path, typeof(Transform), "localScale.y", curvesy);
            clip.SetCurve(path, typeof(Transform), "localScale.z", curvesz);

            AnimationEvent animEvent = new AnimationEvent();
            animEvent.functionName = "cardChangeura";
            animEvent.objectReferenceParameter = cardlist[i];
            animEvent.time = 0.1f;
            clip.AddEvent(animEvent);
        }
        anim.AddClip(clip, clip.name);
        anim.Play(clip.name);
        yield return new WaitUntil(() => !anim.isPlaying);
        
        flipb = true;
    }
    //flipでeventで使う
    private void cardChangeura(GameObject obj)
    {
        obj.GetComponent<SpriteRenderer>().sprite = manager.cardback;
    }
    //カードを動かす(list)
    public void CardMovementD(Transform[] card,Transform target)
    {
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;
        for(int i = 0; i < card.Length; i++)
        {
            card[i].parent = target;
            AnimationCurve curvex = AnimationCurve.EaseInOut(0.0f, card[i].localPosition.x, 0.2f, 0.0f);
            AnimationCurve curvey = AnimationCurve.EaseInOut(0.0f, card[i].localPosition.y, 0.2f, 0.0f);
            string path = GetHierarchyPath(card[i].transform);
            clip.SetCurve(path, typeof(Transform), "localPosition.x", curvex);
            clip.SetCurve(path, typeof(Transform), "localPosition.y", curvey);
        }
        anim.AddClip(clip, clip.name);
        anim.Play(clip.name);
    }
    //カードを動かす(単品)
    public void CardMovement(Transform card, Transform target)
    {
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;
        card.parent = target;
        AnimationCurve curvex = AnimationCurve.EaseInOut(0.0f, card.localPosition.x, 0.2f, 0.0f);
        AnimationCurve curvey = AnimationCurve.EaseInOut(0.0f, card.localPosition.y, 0.2f, 0.0f);
        string path = GetHierarchyPath(card.transform);
        clip.SetCurve(path, typeof(Transform), "localPosition.x", curvex);
        clip.SetCurve(path, typeof(Transform), "localPosition.y", curvey);
        anim.AddClip(clip, clip.name);
        anim.Play(clip.name);
    }
    
    /*
     * DamegeF,TurnS,BattleF,BattleS,EffectA //ダメ計前　霊夢魔理沙　バトル前　妖夢　バトル開始　昨夜　ターン開始時　フラン早苗レミリアさとり幽々子　効果発動後　こいし
     * キャラクターのスキルをタイミングによって分割、それぞれのタイミングで別の場所から呼び出される。
     */
    public void CSkillProcess(CSkillType type)
    {
        if (manager.level == Level.hard || manager.level == Level.expert)
        {
            if (type == CSkillType.EffectA && GetHEbuf(192))
            {
                koisiF = true;
                SetHEbuf(192, false);
                CSbk = true;
                CSb = true;
                return;
            }
        }
        if (manager.level == Level.normal||manager.level==Level.expert)
        {
            if (type == CSkillType.EffectA)
            {
                CSbk = false;
                int nFrame = 1; // フレーム数(1なら直接呼び出したメソッド)
                System.Diagnostics.StackFrame objStackFrame = new System.Diagnostics.StackFrame(nFrame);
                string className = objStackFrame.GetMethod().ReflectedType.FullName;
                StartCoroutine(cSkillProcessE(className));
            }
            else
            {
                CSb = false;
                StartCoroutine(cSKillProcess(type));
            }
        }
        else
        {
            CSbk = true;
            CSb = true;
        }

    }
    //プレイヤーとエネミーで分けるやつ
    protected abstract void SelectInit();
    protected abstract IEnumerator cSkillProcessE(string method);
    protected abstract IEnumerator cSKillProcess(CSkillType type);
    public void Marigan() { StartCoroutine("marigan"); }
    protected abstract IEnumerator marigan();

    //input outputメソッド
    /*HPを変化させる
     * 正なら回復、負ならダメージ
     */
    public void HPChange(int value)
    {
        if (AD && value < 0) return;
        int temp = HP;
        HP += value;
        //ゲージの調整
        List<Transform> changelist = new List<Transform>();
        List<float> coef = new List<float>();
        if (!(temp <= 0 && HP <= 0))
        {
            if (temp > 10 && HP < 10)
            {
                changelist.Add(hpgauge[1]);
                changelist.Add(hpgauge[0]);
                coef.Add((float)(10 - temp));
                coef.Add((float)(HP - 10));
            }
            else if (temp <= 10 && HP > 10)
            {
                changelist.Add(hpgauge[1]);
                changelist.Add(hpgauge[0]);
                coef.Add((float)(HP - 10));
                coef.Add((float)(10 - temp));
            }
            else if (temp > 10 && HP >= 10)
            {
                changelist.Add(hpgauge[1]);
                coef.Add((float)value);
            }
            else if (temp <= 10 && HP <= 10)
            {
                changelist.Add(hpgauge[0]);
                coef.Add((float)value);
            }
            for (int i = 0; i < changelist.Count; i++)
            {
                Vector3 pos = changelist[i].localPosition;
                Vector3 scale = changelist[i].localScale;
                pos.y += coef[i] * hpprate;
                scale.y += coef[i] * hpsrate;
                if (pos.y < gaugelimitp) pos.y = gaugelimitp;
                if (scale.y < 0) scale.y = 0;
                changelist[i].localPosition = pos;
                changelist[i].localScale = scale;
            }
        }

        //ゲージ色の変更
        Color gaugeColor = hpgauge[0].gameObject.GetComponent<SpriteRenderer>().color;
        if (HP > 12 && temp <= 12)
        {
            //graphic.sprite = Resources.Load<Sprite>("Character/" + CName + "/" + CName);
            hptext.color = color["Green"];
            gaugeColor = color["Green"];
        }
        else if (HP <= 12 && HP > 5 && !(temp <= 12 && temp > 5))
        {
            //graphic.sprite = Resources.Load<Sprite>("Character/" + CName + "/" + CName + "1");
            hptext.color = color["Yellow"];
            gaugeColor = color["Yellow"];
        }
        else if (HP <= 5 && temp > 5)
        {
            //graphic.sprite = Resources.Load<Sprite>("Character/" + CName + "/" + CName + "2");
            hptext.color = color["Red"];
            gaugeColor = color["Red"];
        }
        for (int i = 0; i < hpgauge.Length; i++) hpgauge[i].gameObject.GetComponent<SpriteRenderer>().color = gaugeColor;

        //damegeText　PopUp
        battle.DamegePop(value, tag);
        Rename();
    }
    public bool GetHEbuf(int he)
    {
        if (he == 2221) return HEbuf[0];
        else if (he == 182) return HEbuf[1];
        else if (he == 186) return HEbuf[2];
        else if (he == 121) return HEbuf[3];
        else if (he == 156) return HEbuf[4];
        else if (he == 1312) return HEbuf[5];
        else if (he == 192) return HEbuf[6];
        return false;
    }
    public void SetHEbuf(int he, bool b)
    {
        if (he == 2221) HEbuf[0] = b;
        else if (he == 182) HEbuf[1] = b;
        else if (he == 186) HEbuf[2] = b;
        else if (he == 121) HEbuf[3] = b;
        else if (he == 156) HEbuf[4] = b;
        else if (he == 1312) HEbuf[5] = b;
        else if (he == 192) HEbuf[6] = b;
    }
    public void ChangeCard(int[] change, int after)
    {
        for (int i = 0; i < change.Length; i++)
        {
            int l = ListExistnum<int>(Deck, change[i]);
            if (l != -1) Deck[l] = after;
        }
    }
    public void Decksearch(int num)
    {
        Deck.Remove(num);
    }
    public List<int> GetDeck()
    {
        return Deck;
    }
    public string GetFieldCard(int i)
    {
        if (i >= 2) return null;
        return Field[i].transform.GetChild(0).gameObject.name;
    }
    
    //手札のの枚数
    public int HandCardNum()
    {
        int count = 0;
        for (int i = 0; i < Hand.Length; i++)
        {
            if (Hand[i].childCount != 0) count++;
        }
        return count;
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

    //utilityメソッド
    protected bool ListExist<T>(IEnumerable list, T search)
    {
        foreach (var item in list)
        {
            if (item.Equals(search)) return true;
        }
        return false;
    }
    protected int ListExistnum<T>(IEnumerable list, T search)
    {
        int count = 0;
        foreach (var item in list)
        {
            if (item.Equals(search)) return count;
            count++;
        }
        return -1;
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
    public static string GetHierarchyPath(Transform self)
    {
        string path = self.gameObject.name;
        Transform parent = self.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        path = "/" + path;
        return path;
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
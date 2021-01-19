using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//カードデータクラスの一覧
public enum cardType
{
    offensive,defensive,support,
}
public enum State
{
    Select, Battle, Set, Wait
}
/*
 * easy カード効果のみ
 * normal カード効果　キャラクタースキル
 * hard カード効果　役
 * expert カード効果　役　キャラクタースキル
 */
public enum Level
{
    easy, normal, hard,expert
}
/*ダメ計前　霊夢魔理沙
 * バトル前　妖夢
 * バトル開始　昨夜　タ
 * ーン開始時　フラン早苗レミリアさとり幽々子
 * 効果発動後　こいし
 */
public enum CSkillType
{
    DamegeF,TurnS,BattleF,BattleS,EffectA 
}

abstract public class Card
{
    public string Name { get; set; }
    public int Number { get; set; }
    public cardType Type { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public bool Effect { get; set; }
    public string Etext { get; set; }
    public List<int> hand=new List<int>();
    public string tag { get; set; }
    public string[] Htext_sentence;
    public int[] color_num;

    //０なら黒　１ならアクセントカラー bool型で強調するかを選択
    public void TextChange(int num,bool b)
    {
        if (Htext_sentence.Length < num || num < 0) return;
        color_num[num] = b ? 1 : 0;
    }
}
public class Citizen : Card
{
    public Citizen(string str)
    {
        Name = "Citizen";
        Number = 0;
        Type = cardType.defensive;
        Attack = 0;
        Defense = 0;
        Effect = false;
        Etext = "";
        tag = str;
        hand = new List<int> {};
        Htext_sentence = new string[0];
    }
    
}
public class The_Fool : Card
{
    public The_Fool(string str)
    {
        Name = "The Fool";
        Number = 22;
        Type = cardType.support;
        Attack = 0;
        Defense = 0;
        Effect = true;
        tag = str;
        Etext = "このカードは5ターン経過後でなければ、場に出せない。" +Environment.NewLine+
            "このターン、「The_Fool」以外の全てのカードの攻撃力・守備力を0にし、効果を無効にする。" + Environment.NewLine +
            "また、このターンの終了時に、相手の場のカードを1枚を自分の手札に加えることができる。その場合、自分の手札のカードを1枚トラッシュする。";
        Etext = Etext.Replace(" ", "\u00A0");
        hand = new List<int> { 7,12, 15, 21 };
        color_num = new int[4];
        Htext_sentence = new string[4];
        Htext_sentence[0] = "【荒野を拓くもの】" + Environment.NewLine + " \"7.The Chariot\"" + Environment.NewLine + "攻撃力の合計を+2する。";
        Htext_sentence[1] = "【忘却の愚者】" + Environment.NewLine + " \"12.The Hanged Man\"" + Environment.NewLine + "「The Fool」の効果で相手の墓地のカードも選ぶことができる。";
        Htext_sentence[2] = "【混沌導く奇術師】" + Environment.NewLine + " \"15.The Devil\"" + Environment.NewLine + "自分と相手の手札を入れ替える。";
        Htext_sentence[3]= "【旅人の夢】" + Environment.NewLine + " \"21.The World\"" + Environment.NewLine + "以降のターンでは、相手は手札と場のカードを全て公開した状態でバトルを行う。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
    

}
public class The_Magician : Card
{
    public The_Magician(string str)
    {
        Name = "The Magician";
        Number = 1;
        Type = cardType.support;
        Attack = 0;
        Defense = 1;
        Effect = true;
        tag = str;
        Etext = "自分の手札を1枚デッキに戻してシャッフルし、デッキから1枚ドローする。";
        Etext = Etext.Replace(" ", "\u00A0");
        hand =new List<int> { 4,8,9, 12, 16 };
        color_num = new int[5];
        Htext_sentence = new string[5];
        Htext_sentence[0] = "【チャンスメイキング】" + Environment.NewLine + " \"4.The Emperor\"" + Environment.NewLine + "相手の場のカードを手札に戻し、手札からランダムに場に出す。この時、カードを場に出せない制限を無視する。";
        Htext_sentence[1] = "【ベクトル操作】" + Environment.NewLine + " \"8.Strength\"" + Environment.NewLine + "このターン、自分がバトルによるダメージを受ける場合、相手にダメージを反射する。";
        Htext_sentence[2] = "【可能性の開花】" + Environment.NewLine + " \"9.The Hermit\"" + Environment.NewLine + "「The Magician」の効果を「手札を2枚戻し、デッキから2枚選んで手札に加える」に変更する。";
        Htext_sentence[3] = "【魔術の深奥】" + Environment.NewLine + " \"12.The Hanged Man\"" + Environment.NewLine + "次のターン、相手はカードを1枚、公開した状態で場に出さなければならない。" + Environment.NewLine +
            "また、「The Magician」の効果を「手札を好きな枚数戻して、戻した枚数分ドローする」に変更する。";
        Htext_sentence[4]= "【禍殃なる魔術師】" + Environment.NewLine + " \"16.The Tower\"" + Environment.NewLine + "相手がこれまでに発動した役の回数分、相手のHPを減少させる。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
        
    }

}
public class The_High_Priestess : Card
{
    public The_High_Priestess(string str)
    {
        Name="Tha High Priestess";
        Number = 2;
        Type = cardType.defensive;
        Attack = 0;
        Defense = 1;
        Effect = false;
        Etext = "";
        tag = str;
        hand=new List<int> { 3,5, 17, 18 ,19};
        color_num = new int[5];
        Htext_sentence = new string[5];
        Htext_sentence[0] = "【聖女の抱擁】" + Environment.NewLine + " \"3.The Empress\"" + Environment.NewLine + "このターン、自分のHPは減少しない。";
        Htext_sentence[1] = "【救済するもの】" + Environment.NewLine + " \"5.The Hierophant\"" + Environment.NewLine + "自分のHPを4点回復する。";
        Htext_sentence[2] = "【星の導き】" + Environment.NewLine + " \"17.The Star\"" + Environment.NewLine + "このターン、自分のHPは減少しない。";
        Htext_sentence[3] = "【欺瞞の女教皇】" + Environment.NewLine + " \"18.The Moon\"" + Environment.NewLine + "次のターン終了時まで、相手の手札を公開した状態でバトルを行う。";
        Htext_sentence[4] = "【雨除けの巫女】" + Environment.NewLine + " \"19.The Sun\"" + Environment.NewLine + "これ以降に一度、自分を対象にする効果を無効にする。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class The_Empress : Card
{
    public The_Empress(string str)
    {
        Name = "The Empress";
        Number = 3;
        Type = cardType.defensive;
        Attack = 0;
        Defense = 2;
        Effect = false;
        Etext = "";
        tag = str;
        hand=new List<int> { 2, 4,7,10, 11 };
        color_num = new int[5];
        Htext_sentence = new string[5];
        Htext_sentence[0] = "【聖女の抱擁】" + Environment.NewLine + " \"2.The High Priestess\"" + Environment.NewLine + "このターン、自分のHPは減少しない。";
        Htext_sentence[1] = "【理想の王政】" + Environment.NewLine + " \"4.The Emperor\"" + Environment.NewLine + "「The Empress」と「The Emperor」の攻撃力・守備力を3倍にする。";
        Htext_sentence[2] = "【アマゾネスクイーン】" + Environment.NewLine + " \"7.The Chariot\"" + Environment.NewLine + "相手の守備力を2減少させ、自分の守備力を２増加する。";
        Htext_sentence[3] = "【傾国の美女】" + Environment.NewLine + " \"10.Wheel of Fortune\"" + Environment.NewLine + "この効果の発動以降、相手の「The Emperor」・「The Lovers」・「The Chariot」を平民(攻守０,効果なし,役なし)にする";
        Htext_sentence[4] = "【義憤の女帝】" + Environment.NewLine + " \"11.Justice\"" + Environment.NewLine + "このターン、自分がバトルによるダメージを受ける場合、相手に２倍のダメージを反射する";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class The_Emperor : Card
{
    public The_Emperor(string str)
    {
        Name = "The Emperor";
        Number = 4;
        Type = cardType.offensive;
        Attack = 2;
        Defense = 0;
        Effect = false;
        Etext = "";
        tag = str;
        hand=new List<int> {1, 3, 7, 9,20 };
        color_num = new int[5];
        Htext_sentence = new string[5];
        Htext_sentence[0] = "【チャンスメイキング】" + Environment.NewLine + " \"1.The Magician\"" + Environment.NewLine + "相手の場のカードを手札に戻し、手札からランダムに場に出す。この時、カードを場に出せない制限を無視する。";
        Htext_sentence[1] = "【理想の王政】" + Environment.NewLine + " \"3.The Empress\"" + Environment.NewLine + "「The Empress」と「The Emperor」の攻撃力・守備力を3倍にする。";
        Htext_sentence[2] = "【王へ至る道】" + Environment.NewLine + " \"7.Chariot\"" + Environment.NewLine + "このターン、攻撃力の合計を2増加させ、相手に与えたダメージの半分回復する。";
        Htext_sentence[3] = "【約束された勝利の剣】" + Environment.NewLine + " \"9.The Hermit\"" + Environment.NewLine + "「The Emperor」の攻撃力を3倍にし、相手に与えたダメージの半分回復する。";
        Htext_sentence[4] = "【断罪の皇帝】" + Environment.NewLine + " \"20.Judgement\"" + Environment.NewLine + "相手の場の「The Fool」を除く16番以下のカードを平民(攻守0,効果なし,役なし)にする。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class The_Hierophant : Card
{
    public The_Hierophant(string str)
    {
        Name = "The Hierophant";
        Number = 5;
        Type = cardType.defensive;
        Attack = 0;
        Defense = 3;
        Effect = false;
        Etext = "";
        tag = str;
        hand=new List<int> { 2,11,19, 20, 21 };
        color_num = new int[5];
        Htext_sentence = new string[5];
        Htext_sentence[0] = "【救済するもの】" + Environment.NewLine + " \"2.The High Priestess\"" + Environment.NewLine + "自分のHPを4点回復する。";
        Htext_sentence[1] = "【正義の法】" + Environment.NewLine + " \"11.Justice\"" + Environment.NewLine + "このターン、自分のHPは減少しない。";
        Htext_sentence[2] = "【破天荒なる教皇】" + Environment.NewLine + " \"19.The Sun\"" + Environment.NewLine + "相手のダメージ反射、ダメージ無効、守備力の合計を増加させる効果を無効にする。";
        Htext_sentence[3] = "【法王の裁き】" + Environment.NewLine + " \"20.Judgement\"" + Environment.NewLine + "相手の場のカードの効果と役を無効にする。";
        Htext_sentence[4] = "【世界の真理】" + Environment.NewLine + " \"21.The World\"" + Environment.NewLine + "このターン、相手が役を発動する場合、それを無効にし、自分の役として発動する。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class The_Lovers : Card
{
    public The_Lovers(string str)
    {
        Name = "The Lovers";
        Number = 6;
        Type = cardType.support;
        Attack = 0;
        Defense = 1;
        Effect = true;
        Etext = "自分のHPを2点回復する。";
        Etext = Etext.Replace(" ", "\u00A0");
        tag = str;
        hand=new List<int> { 9,10, 13,15, 18 };
        color_num = new int[5];
        Htext_sentence = new string[5];
        Htext_sentence[0] = "【導き手】" + Environment.NewLine + " \"9.The Hermit\"" + Environment.NewLine + "このターン、自分のHPは減少しない。";
        Htext_sentence[1] = "【恋人は惹かれ合う】" + Environment.NewLine + " \"10.Wheel of Fortune\"" + Environment.NewLine + "相手の場のカードを1枚選んで、そのカードを「The Lovers」にしてバトルを行う。相手の場に「The Lovers」がある場合、この効果は無効になる。";
        Htext_sentence[2] = "【嘆きの恋人】" + Environment.NewLine + " \"13.Death\"" + Environment.NewLine + "相手の場に「The Lovers」がない場合、相手のHPを3点減少させ、自分の「Death」の効果を無効にする。「The Lovers」がある場合、相手のHPを3点回復し、自分の「The Lovers」の効果を無効にする。";
        Htext_sentence[3] = "【禁断の果実】" + Environment.NewLine + " \"15.The Devil\"" + Environment.NewLine + "自分のHPを3点減少する。次のターンのドロー後、手札をすべてデッキに戻して、戻した枚数分デッキから選んで手札に加える。";
        Htext_sentence[4] = "【月への憧憬】" + Environment.NewLine + " \"18.The Moon\"" + Environment.NewLine + "次のターンの開始時に、相手の手札を確認し、1枚選んで公開した状態で場に出す。相手はこのカードを戻すことができない。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
    
}
public class The_Chariot : Card
{
    public The_Chariot(string str)
    {
        Name = "The Chariot";
        Number = 7;
        Type = cardType.offensive;
        Attack = 3;
        Defense = 0;
        Effect = false;
        Etext = "";
        tag = str;
        hand=new List<int> { 0,3,4, 13, 17 };
        color_num = new int[5];
        Htext_sentence = new string[5];
        Htext_sentence[0] = "【荒野を拓くもの】" + Environment.NewLine + " \"0.The Fool\"" + Environment.NewLine + "攻撃力の合計を+2する。";
        Htext_sentence[1] = "【アマゾネスクイーン】" + Environment.NewLine + " \"3.The Empress\"" + Environment.NewLine + "相手の守備力を2減少させ、自分の守備力を２増加する。";
        Htext_sentence[2] = "【王へ至る道】" + Environment.NewLine + " \"4.The Emperor\"" + Environment.NewLine + "このターン、攻撃力の合計を2増加させ、相手に与えたダメージの半分回復する。";
        Htext_sentence[3] = "【戦乱の英雄】" + Environment.NewLine + " \"13.Death\"" + Environment.NewLine + "相手の守備力の合計を2減少させ、自分の守備力の合計を2増加する。";
        Htext_sentence[4] = "【怒涛の戦車】" + Environment.NewLine + " \"17.The Star\"" + Environment.NewLine + "「The Star」の守備力を0にし、攻撃力を2増加する。また、このターン終了時、自分が受けたダメージの半分回復する。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class Strength : Card
{
    public Strength(string str)
    {
        Name = "Strength";
        Number = 8;
        Type = cardType.offensive;
        Attack = 3;
        Defense = 0;
        Effect = false;
        Etext = "";
        tag = str;
        hand=new List<int> { 1,9,10, 11, 15 };
        color_num = new int[5];
        Htext_sentence = new string[5];
        Htext_sentence[0] = "【ベクトル操作】" + Environment.NewLine + " \"1.The Magician\"" + Environment.NewLine + "このターン、自分がバトルによるダメージを受ける場合、相手にダメージを反射する。";
        Htext_sentence[1] = "【森の賢者】" + Environment.NewLine + " \"9.The Hermit\"" + Environment.NewLine + "このターン、「The Hermit」の守備力を２倍にする。また、相手のカードの効果を無効にする。";
        Htext_sentence[2] = "【先手必勝】" + Environment.NewLine + " \"10.Wheel of Fortune\"" + Environment.NewLine + "相手のHPを3点減少させ、自分のHPを3点回復する。この効果は無効にされず、最初のターンにのみ発動できる。";
        Htext_sentence[3] = "【力こそ正義】" + Environment.NewLine + " \"11.Justice\"" + Environment.NewLine + "このターン、相手の守備力を無視してダメージを与える。";
        Htext_sentence[4] = "【鮮烈なる力】" + Environment.NewLine + " \"15.The Devil\"" + Environment.NewLine + "自分のトラッシュの枚数に応じて以下の効果を発動する。" + Environment.NewLine +
            "4枚以上の場合、「The Devil」の攻撃力・守備力を２倍にする" + Environment.NewLine +
            "14枚以上の場合、さらに「The Devil」の攻撃力・守備力が2倍にする。" + Environment.NewLine +
            "20枚以上の場合、この効果は無効にされず、相手の場のカードの効果と役を無効にする。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class The_Hermit : Card
{
    public The_Hermit(string str)
    {
        Name = "The Hermit";
        Number = 9;
        Type = cardType.defensive;
        Attack = 0;
        Defense = 3;
        Effect = false;
        Etext = "";
        tag = str;
        hand=new List<int> { 1,4,6, 8, 14 };
        color_num = new int[5];
        Htext_sentence = new string[5];
        Htext_sentence[0] = "【可能性の開花】" + Environment.NewLine + " \"1.The Magician\"" + Environment.NewLine + "「The Magician」の効果を「手札を2枚戻し、デッキから2枚選んで手札に加える」に変更する。";
        Htext_sentence[1] = "【約束された勝利の剣】" + Environment.NewLine + " \"4.The Emperor\"" + Environment.NewLine + "「The Emperor」の攻撃力を3倍にし、相手に与えたダメージの半分回復する。";
        Htext_sentence[2] = "【導き手】" + Environment.NewLine + " \"6.The Lovers\"" + Environment.NewLine + "このターン、自分のHPは減少しない。";
        Htext_sentence[3] = "【森の賢者】" + Environment.NewLine + " \"8.Strength\"" + Environment.NewLine + "このターン、「The Hermit」の守備力を２倍にする。また、相手のカードの効果を無効にする。";
        Htext_sentence[4] = "【止水の隠者】" + Environment.NewLine + " \"14.Temperance\"" + Environment.NewLine + "相手の手札をランダムに2枚トラッシュし、相手のトラッシュから2枚を選んで手札に加えさせる。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class Wheel_of_Fortune : Card
{
    public Wheel_of_Fortune(string str)
    {
        Name = "Wheel of Fortune";
        Number = 10;
        Type = cardType.support;
        Attack = 0;
        Defense = 1;
        Effect = true;
        Etext = "自分の手札を全てデッキに戻してシャッフルし、戻した枚数分ドローする。";
        Etext = Etext.Replace(" ", "\u00A0");
        tag = str;
        hand=new List<int> { 3,6,8, 17, 21 };
        color_num = new int[5];
        Htext_sentence = new string[5];
        Htext_sentence[0] = "【傾国の美女】" + Environment.NewLine + " \"3.The Empress\"" + Environment.NewLine + "この効果の発動以降、相手の「The Emperor」・「The Lovers」・「The Chariot」を平民(攻守０,効果なし,役なし)にする";
        Htext_sentence[1] = "【恋人は惹かれ合う】" + Environment.NewLine + " \"6.The Lovers\"" + Environment.NewLine + "相手の場のカードを1枚選んで、そのカードを「The Lovers」にしてバトルを行う。相手の場に「The Lovers」がある場合、この効果は無効になる。";
        Htext_sentence[2] = "【先手必勝】" + Environment.NewLine + " \"8.Strength\"" + Environment.NewLine + "相手のHPを3点減少させ、自分のHPを3点回復する。この効果は無効にされず、最初のターンにのみ発動できる。";
        Htext_sentence[3] = "【運命の軌跡】" + Environment.NewLine + " \"17.The Star\"" + Environment.NewLine + "「Wheel of Fortune」の効果でのドローの代わりに、デッキから選んで手札に加えることができる。";
        Htext_sentence[4] = "【胎動する世界】" + Environment.NewLine + " \"21.The World\"" + Environment.NewLine + "お互いの手札をすべてデッキに戻す。その後、自分のデッキから4枚選んで手札に加え、相手のデッキから4枚を選んで手札に加えさせる。" + Environment.NewLine +
            "このターン、お互いに手札を操作する効果を使用できない。この効果はお互いが発動する場合、お互いに無効になる。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class Justice : Card
{
    public Justice(string str)
    {
        Name = "Justice";
        Number = 11;
        Type = cardType.offensive;
        Attack = 3;
        Defense = 0;
        Effect = false;
        Etext = "";
        tag = str;
        hand=new List<int> {3,5, 8, 14 };
        color_num = new int[4];
        Htext_sentence = new string[4];
        Htext_sentence[0] = "【義憤の女帝】" + Environment.NewLine + " \"3.The Empress\"" + Environment.NewLine + "このターン、自分がバトルによるダメージを受ける場合、相手に２倍のダメージを反射する";
        Htext_sentence[1] = "【正義の法】" + Environment.NewLine + " \"5.The Hierophant\"" + Environment.NewLine + "このターン、自分のHPは減少しない。";
        Htext_sentence[2] = "【力こそ正義】" + Environment.NewLine + " \"8.Strength\"" + Environment.NewLine + "このターン、相手の守備力を無視してダメージを与える。";
        Htext_sentence[3] = "【生命循環】" + Environment.NewLine + " \"14.Temperance\"" + Environment.NewLine + "お互いのHPを、お互いのHPの合計の半分にする。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class The_Hanged_Man : Card
{
    public The_Hanged_Man(string str)
    {
        Name = "The Hanged Man";
        Number = 12;
        Type = cardType.support;
        Attack = 1;
        Defense = 1;
        Effect = true;
        Etext = "このターン、自分が受けるダメージが1点増加するが、" +
            "次のターン、相手に与えるダメージが2点増加する。";
        Etext = Etext.Replace(" ", "\u00A0");
        tag = str;
        hand=new List<int> { 22, 1,13, 15 };
        color_num = new int[4];
        Htext_sentence = new string[4];
        Htext_sentence[0] = "【忘却の愚者】" + Environment.NewLine + " \"0.The Fool\"" + Environment.NewLine + "「The Fool」の効果で相手の墓地のカードも対象に選ぶことができる。";
        Htext_sentence[1] = "【魔術の深奥】" + Environment.NewLine + " \"1.The Magician\"" + Environment.NewLine + "次のターン、相手はカードを1枚、公開して場に出さなければならない。" + Environment.NewLine +
            "また、「The Magician」の効果を「手札を好きな枚数戻して、戻した枚数分ドローする」に変更する。";
        Htext_sentence[2] = "【ラグナロク】" + Environment.NewLine + " \"13.Death\"" + Environment.NewLine + "自分のHPを2点減少する。このターン以降、9番以下の自分のカードはすべて、攻撃力・守備力が+1される。";
        Htext_sentence[3] = "【暗闇の先にあるもの】" + Environment.NewLine + " \"15.The Devil\"" + Environment.NewLine + "自分のHPを3点減少する。このターン、自分の場のカードの効果は無効になる。次のターン、自分の場のカードの攻撃力・守備力の合計をそれぞれ２倍にする。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }

}
public class Death : Card
{
    public Death(string str)
    {
        Name = "Death";
        Number = 13;
        Type = cardType.offensive;
        Attack = 4;
        Defense = 0;
        Effect = true;
        Etext = "自分のHPを2点減少する。";
        Etext = Etext.Replace(" ", "\u00A0");
        tag = str;
        hand=new List<int> { 6, 7, 12,16 };
        color_num = new int[4];
        Htext_sentence = new string[4];
        Htext_sentence[0] = "【嘆きの恋人】" + Environment.NewLine + " \"6.The Lovers\"" + Environment.NewLine + "相手の場に「The Lovers」がない場合、相手のHPを3点減少させ、自分の「Death」の効果を無効にする。「The Lovers」がある場合、相手のHPを3点回復し、自分の「The Lovers」の効果を無効にする。";
        Htext_sentence[1] = "【戦乱の英雄】" + Environment.NewLine + " \"7.The Chariot\"" + Environment.NewLine + "相手の守備力の合計を2減少させ、自分の守備力の合計を2増加する。";
        Htext_sentence[2] = "【ラグナロク】" + Environment.NewLine + " \"12.The Hanged Mang\"" + Environment.NewLine + "自分のHPを2点減少する。このターン以降、9番以下の自分のカードはすべて、攻撃力・守備力が+1される。";
        Htext_sentence[3] = "【崩壊の足音】" + Environment.NewLine + " \"16.The Tower\"" + Environment.NewLine + "2ターンの間、ターン終了時に相手のHPを2点減少させる。" + Environment.NewLine +
            "またこのターン、「Death」・「The Tower」の効果を無効にする。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }

}
public class Temperance : Card
{
    public Temperance(string str)
    {
        Name = "Temperance";
        Number = 14;
        Type = cardType.support;
        Attack = 2;
        Defense = 2;
        Effect = false;
        Etext = "";
        tag = str;
        hand=new List<int> { 9, 11, 19 ,21};
        color_num = new int[4];
        Htext_sentence = new string[4];
        Htext_sentence[0] = "【止水の隠者】" + Environment.NewLine + " \"9.The Hermit\"" + Environment.NewLine + "相手の手札をランダムに2枚トラッシュし、相手のトラッシュから2枚を選んで手札に加えさせる。";
        Htext_sentence[1] = "【生命循環】" + Environment.NewLine + " \"11.Justice\"" + Environment.NewLine + "お互いのHPを、お互いのHPの合計の半分にする。";
        Htext_sentence[2] = "【生命満ちる大地】" + Environment.NewLine + " \"19.The Sun\"" + Environment.NewLine + "自分のHPを3点回復する。";
        Htext_sentence[3] = "【理想郷】" + Environment.NewLine + " \"21.The World\"" + Environment.NewLine + "相手のHPを自分のHPと同じにする。この効果は、現在のターン数が10未満の場合のみ発動する。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class The_Devil : Card
{
    public The_Devil(string str)
    {
        Name = "The Devil";
        Number = 15;
        Type = cardType.support;
        Attack = 2;
        Defense = 1;
        Effect = true;
        Etext = "お互いの手札を全てデッキに戻してシャッフルし、戻した枚数分ドローする。";
        Etext = Etext.Replace(" ", "\u00A0");
        tag = str;
        hand=new List<int> { 22, 6,8, 12 };
        color_num = new int[4];
        Htext_sentence = new string[4];
        Htext_sentence[0] = "【混沌導く奇術師】" + Environment.NewLine + " \"0.The Fool\"" + Environment.NewLine + "自分と相手の手札を入れ替える。";
        Htext_sentence[1] = "【禁断の果実】" + Environment.NewLine + " \"6.The Lovers\"" + Environment.NewLine + "自分のHPを3点減少する。次のターンのドロー後、手札をすべてデッキに戻して、戻した枚数分デッキから選んで手札に加える。";
        Htext_sentence[2] = "【鮮烈なる力】" + Environment.NewLine + " \"8.Strength\"" + Environment.NewLine + "自分のトラッシュの枚数に応じて以下の効果を発動する。" + Environment.NewLine +
            "4枚以上の場合、「The Devil」の攻撃力・守備力を2倍にする。" + Environment.NewLine +
            "14枚以上の場合、さらに「The Devil」の攻撃力・守備力を2倍にする。" + Environment.NewLine +
            "20枚以上の場合、この効果は無効にされず、相手の場のカードの効果と役を無効にする。";
        Htext_sentence[3] = "【暗闇の先にあるもの】" + Environment.NewLine + " \"12.The Hanged Man\"" + Environment.NewLine + "自分のHPを3点減少する。このターン、自分の場のカードの効果は無効になる。次のターン、自分の場のカードの攻撃力・守備力の合計をそれぞれ２倍にする。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }

}
public class The_Tower : Card
{
    public The_Tower(string str)
    {
        Name = "The Tower";
        Number = 16;
        Type = cardType.defensive;
        Attack = 1;
        Defense = 4;
        Effect = true;
        Etext = "自分のHPを2点減少する。";
        Etext = Etext.Replace(" ", "\u00A0");
        tag = str;
        hand=new List<int> { 1, 13, 18,20 };
        color_num = new int[4];
        Htext_sentence = new string[4];
        Htext_sentence[0] = "【禍殃なる魔術師】" + Environment.NewLine + " \"1.The Magician\"" + Environment.NewLine + "相手がこれまでに発動した役の回数分、相手のHPを減少させる。";
        Htext_sentence[1] = "【崩壊の足音】" + Environment.NewLine + " \"13.Death\"" + Environment.NewLine + "2ターンの間、ターン終了時に相手のHPを2点減少させる。" + Environment.NewLine +
            "またこのターン、自分の「Death」・「The Tower」の効果を無効にする。";
        Htext_sentence[2] = "【狂乱の坩堝】" + Environment.NewLine + " \"18.The Moon\"" + Environment.NewLine + "お互いの場のカードを全てトラッシュする。その後、それぞれのトラッシュのカードを2枚ランダムに場に出し、バトルを行う。";
        Htext_sentence[3] = "【神の審判】" + Environment.NewLine + " \"20.Judgement\"" + Environment.NewLine + "このターン、相手が役を発動する場合、それを無効にして、相手のHPを4点減少させる。役を発動いない場合、相手のHPを2点回復する。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }

}
public class The_Star : Card
{
    public The_Star(string str)
    {
        Name = "The Star";
        Number = 17;
        Type = cardType.offensive;
        Attack = 4;
        Defense = 2;
        Effect = true;
        Etext = "自分のHPを2点回復する。";
        Etext = Etext.Replace(" ", "\u00A0");
        tag = str;
        hand=new List<int> { 2, 7, 10 ,18};
        color_num = new int[4];
        Htext_sentence = new string[4];
        Htext_sentence[0] = "【星の導き】" + Environment.NewLine + " \"2.The High Priestess\"" + Environment.NewLine + "このターン、自分のHPは減少しない。";
        Htext_sentence[1] = "【怒涛の戦車】" + Environment.NewLine + " \"7.The Chariot\"" + Environment.NewLine + "「The Star」の守備力を0にし、攻撃力を2増加する。また、このターン終了時、自分が受けたダメージの半分回復する。";
        Htext_sentence[2] = "【運命の軌跡】" + Environment.NewLine + " \"10.Wheel of Fortune\"" + Environment.NewLine + "「Wheel of Fortune」の効果でのドローの代わりに、デッキから選んで手札に加えることができる。";
        Htext_sentence[3] = "【スターリー・ナイツ】" + Environment.NewLine + " \"18.The Moon\"" + Environment.NewLine + "自分のHPを２点回復し、攻撃力の合計を+1、守備力の合計を+2する。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class The_Moon : Card
{
    public The_Moon(string str)
    {
        Name = "The Moon";
        Number = 18;
        Type = cardType.support;
        Attack = 0;
        Defense = 2;
        Effect = true;
        Etext = "このターン、相手の場の「The World」・「The Fool」・「The Moon」以外のカードの効果を無効にする。";
        Etext = Etext.Replace(" ", "\u00A0");
        tag = str;
        hand=new List<int> { 2, 6, 16 ,17};
        color_num = new int[4];
        Htext_sentence = new string[4];
        Htext_sentence[0] = "【欺瞞の女教皇】" + Environment.NewLine + " \"2.The High Priestess\"" + Environment.NewLine + "次のターン終了時まで、相手の手札を公開した状態でバトルを行う。";
        Htext_sentence[1] = "【月への憧憬】" + Environment.NewLine + " \"6.The Lovers\"" + Environment.NewLine + "次のターンの開始時に、相手の手札を確認し、1枚選んで公開した状態で場に出す。相手はこのカードを戻すことができない。";
        Htext_sentence[2] = "【狂乱の坩堝】" + Environment.NewLine + " \"16.The Tower\"" + Environment.NewLine + "お互いの場のカードを全てトラッシュする。その後、それぞれのトラッシュのカードを2枚ランダムに場に出し、バトルを行う。";
        Htext_sentence[3] = "【スターリー・ナイツ】" + Environment.NewLine + " \"17.The Star\"" + Environment.NewLine + "自分のHPを２点回復し、攻撃力の合計を+1、守備力の合計を+2する。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }

}
public class The_Sun : Card
{
    public The_Sun(string str)
    {
        Name = "The Sun";
        Number = 19;
        Type = cardType.offensive;
        Attack = 5;
        Defense = 0;
        Effect = true;
        Etext = "このターン、相手の守備力の合計を-3する。";
        Etext = Etext.Replace(" ", "\u00A0");
        tag = str;
        hand=new List<int> {2, 5, 14, 20 };
        color_num = new int[4];
        Htext_sentence = new string[4];
        Htext_sentence[0] = "【雨除けの巫女】" + Environment.NewLine + " \"2.The High Priestess\"" + Environment.NewLine + "これ以降に一度、自分を対象にする効果を無効にする。";
        Htext_sentence[1] = "【破天荒なる教皇】" + Environment.NewLine + " \"5.The Hierophant\"" + Environment.NewLine + "相手のダメージ反射、ダメージ無効、守備力の合計を増加させる効果を無効にする。";
        Htext_sentence[2] = "【生命満ちる大地】" + Environment.NewLine + " \"14.Temperance\"" + Environment.NewLine + "自分のHPを3点回復する。";
        Htext_sentence[3] = "【方舟にかかる虹】" + Environment.NewLine + " \"20.Judgement\"" + Environment.NewLine + "このターン、「The Sun」・「Judgement」の効果の値を倍にする。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class Judgement : Card
{
    public Judgement(string str)
    {
        Name = "Judgement";
        Number = 20;
        Type = cardType.defensive;
        Attack = 1;
        Defense = 5;
        Effect = true;
        Etext = "自分のHPを4点回復する。";
        Etext = Etext.Replace(" ", "\u00A0");
        tag = str;
        hand=new List<int> { 4, 5,16, 19 };
        color_num = new int[4];
        Htext_sentence = new string[4];
        Htext_sentence[0] = "【断罪の皇帝】" + Environment.NewLine + " \"4.The Emperor\"" + Environment.NewLine + "相手の場の「The Fool」を除く16番以下のカードを平民(攻守0,効果なし,役なし)にする。";
        Htext_sentence[1] = "【法王の裁き】" + Environment.NewLine + " \"5.The Hierophant\"" + Environment.NewLine + "相手の場のカードの効果と役を無効にする。";
        Htext_sentence[2] = "【神の審判】" + Environment.NewLine + " \"16.The Tower\"" + Environment.NewLine + "このターン、相手が役を発動する場合、それを無効にして、相手のHPを4点減少させる。役を発動いない場合、相手のHPを2点回復する。";
        Htext_sentence[3] = "【方舟にかかる虹】" + Environment.NewLine + " \"19.The Sun\"" + Environment.NewLine + "このターン、「The Sun」・「Judgement」の効果の値を倍にする。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }
}
public class The_World : Card
{
    public The_World(string str)
    {
        Name = "The World";
        Number = 21;
        Type = cardType.offensive;
        Attack = 0;
        Defense = 2;
        Effect = true;
        Etext = "このカードは5ターン経過後でなければ、場に出せない。" +
            "相手のHPを7点減少させ、自分のHPを3点回復する。";
        Etext = Etext.Replace(" ", "\u00A0");
        tag = str;
        hand=new List<int> { 22, 5, 10 ,14};
        color_num = new int[4];
        Htext_sentence = new string[4];
        Htext_sentence[0] = "【旅人の夢】" + Environment.NewLine + " \"0.The Fool\"" + Environment.NewLine + "以降のターンでは、相手は手札と場のカードを全て公開した状態でバトルを行う。";
        Htext_sentence[1] = "【世界の真理】" + Environment.NewLine + " \"5.The Hierophant\"" + Environment.NewLine + "このターン、相手が役を発動する場合、それを無効にし、自分の役として発動することができる。";
        Htext_sentence[2] = "【胎動する世界】" + Environment.NewLine + " \"10.Wheel of Fortune\"" + Environment.NewLine + "お互いの手札をすべてデッキに戻す。その後、自分のデッキから4枚選んで手札に加え、相手のデッキから4枚を選んで手札に加えさせる。" + Environment.NewLine +
            "このターン、お互いに手札を操作する効果を使用できない。この効果はお互いが発動する場合、お互いに無効になる。";
        Htext_sentence[3] = "【理想郷】" + Environment.NewLine + " \"14.Temperance\"" + Environment.NewLine + "相手のHPを自分のHPと同じにする。この効果は、現在のターン数が10未満の場合のみ発動する。";
        for (int i = 0; i < Htext_sentence.Length; i++) Htext_sentence[i].Replace(" ", "\u00A0");
    }

}



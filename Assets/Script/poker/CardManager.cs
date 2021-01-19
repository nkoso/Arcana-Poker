using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class CardManager : MonoBehaviour {
	const int handmax = 6;
	public Sprite[] arcana;
	public Sprite cardback;
	public Player player;
	public Enemy enemy;
	public GameObject gamePanel;	
	public Text turntext;
	public State state { get; set; }
	private BattleTurn battle;
	private int battleturn;
	private string[] CharaName=new string[2];
	private bool battleEndFlag;
    public Level level;
	private Level preLevel;
	public CardPanel cardpanel;
	private string[] spriname = new string[2];
	public Transform UIpanel;
	[SerializeField] Image background;
	[SerializeField] AudioSource bgm;

	public bool EnemyWait { get; set; }

	void Start ()
    {
		battle = GetComponent<BattleTurn>();
		gamePanel.SetActive(false);
		for (int i = 0; i < gamePanel.transform.childCount; i++) gamePanel.transform.GetChild(i).gameObject.SetActive(false);
		turntext.text = ("TURN : " + (battleturn + 1).ToString());
		cardpanel.LevelSet(level);
		StageSet();
		player.Rename();
		enemy.Rename();
		battleEndFlag = false;
		StartCoroutine("DuelStart");
	}
    IEnumerator DuelStart()
    {
		player.DeckShuffle();
		enemy.DeckShuffle();
		yield return new WaitUntil(() => player.synch && enemy.synch);
		battleturn = 0;
		StartCoroutine("TurnStart");
	}
	IEnumerator TurnStart()
	{
		turntext.text = ("TURN : " + (battleturn + 1).ToString());
		player.Trash.MenuOn();
		enemy.Trash.MenuOn();
        if (battleturn > 0)
        {
			player.CSkillProcess(CSkillType.TurnS);
			enemy.CSkillProcess(CSkillType.TurnS);
			yield return new WaitUntil(() => player.CSb && enemy.CSb);
		}
		if (player.HP <= 0 || enemy.HP <= 0)
		{
			GameEnd();
			yield break;
		}
		state = State.Set;
		player.Draw(handmax-player.HandCardNum());
		enemy.Draw(handmax - enemy.HandCardNum());
        //ドローの特別な奴も待機
		yield return new WaitUntil(()=>player.synch&&enemy.synch && !player.GetHEbuf(156) && !enemy.GetHEbuf(156));
		//マリガン
        if (battleturn == 0)
        {
			if (level == Level.expert || level == Level.hard) player.HandSupport();
			player.Marigan();
			enemy.Marigan();
			yield return new WaitUntil(() => player.mariganb && enemy.mariganb);
        }
		player.HandShuffle();
		enemy.HandShuffle();
		yield return new WaitUntil(() => player.synch && enemy.synch);
		if (level == Level.expert || level == Level.hard) player.HandSupport();
		state = State.Set;
		enemy.Hand2Field();

	}
	public void BattleStart() { StartCoroutine("battleStart"); }
    IEnumerator battleStart()
    {
		player.PlayerUIReset();
		player.Trash.MenuOff();
		enemy.Trash.MenuOff();
		if (enemy.Waiting)
        {
			state = State.Wait;
			EnemyWait = true;
			enemy.Waiting = false;
			yield return new WaitUntil(() => !EnemyWait);
        }
		player.FieldCount = 0;
		enemy.FieldCount = 0;
		if (level == Level.expert || level == Level.hard) player.SupportReset();
		battle.BattleStart();
	}
    //バトルが終わってエンド処理
    public void TurnEnd() { StartCoroutine("turnEnd"); }
    IEnumerator turnEnd()
    {
		player.CardTrash();
		enemy.CardTrash();
		yield return new WaitUntil(() => (enemy.synch&&player.synch));
		Debug.Log("Turn End" + battleturn);
		battleturn++;
		if (battleturn == 11　|| player.HP<=0 || enemy.HP<=0)
		{
			GameEnd();
		}
		else StartCoroutine("TurnStart");
	}
    //ゲーム終了
    public void GameEnd() { StartCoroutine("gameEnd"); }
	IEnumerator gameEnd()
    {
		battleEndFlag = true;
		gamePanel.SetActive(true);
		Animation anim = gamePanel.GetComponent<Animation>();

        foreach(Transform item in gamePanel.transform)
        {
			item.gameObject.SetActive(true);
			if (item.GetComponent<Text>() != null)
			{
				anim.Play("poker_panel");
				if (player.HP > enemy.HP) item.GetComponent<Text>().text = "YOU  WIN !";
				else if (player.HP < enemy.HP) item.GetComponent<Text>().text = "YOU  LOSE...";
				else if ((player.HP == enemy.HP) || (player.HP < 0 && enemy.HP < 0)) item.GetComponent<Text>().text = "DROW";
				yield return new WaitUntil(() => !anim.isPlaying);
			}
			if (item.name == "Restart")
			{
				item.transform.GetChild(0).GetComponent<Text>().text = "Restart";
			}
		}
		
		char[] del = { '1', '2' };
		string[] str = player.graphic.sprite.name.Split(del);
		string[] str1 = enemy.graphic.sprite.name.Split(del);
		CharaName[0] = str[0];
		CharaName[1] = str1[0];
		preLevel = level;
    }
    //エスケープ処理
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && !battleEndFlag) escape();
	}
    
	private void escape()
    {
		gamePanel.SetActive(true);
        foreach(Transform item in gamePanel.transform)
        {
			item.gameObject.SetActive(true);
			if (item.GetComponent<Text>() != null)
			{
				item.GetComponent<Text>().text = "Escape Menu";
				item.GetComponent<Text>().color = Color.red;
			}
			if (item.name == "Restart")
			{
				item.transform.GetChild(0).GetComponent<Text>().text = "Back";
			}
		}
	}
    public void Restart()
    {
        if (battleEndFlag)
        {
			Debug.Log("end");
			SceneManager.sceneLoaded += GameSceneLoaded;
			SceneManager.LoadScene("poker");
		}
        else
        {
			foreach (Transform item in gamePanel.transform) item.gameObject.SetActive(false);
			gamePanel.SetActive(false);
		}
    }
	private void GameSceneLoaded(Scene next, LoadSceneMode mode)
	{
		var gameManager = GameObject.FindWithTag("GameManager").GetComponent<CardManager>();
		gameManager.nameset(CharaName[0], CharaName[1]);
		gameManager.level = preLevel;
		SceneManager.sceneLoaded -= GameSceneLoaded;
	}
	public void nameset(string pname, string ename)
	{
		spriname[0] = pname;
		spriname[1] = ename;
		player.graphic.sprite = Resources.Load<Sprite>("Character/" + pname + "/" + pname);
		enemy.graphic.sprite = Resources.Load<Sprite>("Character/" + ename + "/" + ename);
	}
	public int GetBattleTurn() { return battleturn; }
	public void Exit() { SceneManager.LoadScene("StartGamen"); }
    public void Menu() { SceneManager.LoadScene("CharacterS"); }

    private void StageSet()
    {
		System.Random random = new System.Random(CurrentTimeSeed());
		int random_num = random.Next(0, 11);
		background.sprite = Resources.Load<Sprite>("Background/" + random_num.ToString());
		bgm.clip = Num2Audio(random_num);
		bgm.Play();
	}
    private AudioClip Num2Audio(int num)
    {
		if (num == 0) return Resources.Load<AudioClip>("Audio/SolCave021");
		else if (num == 1) return Resources.Load<AudioClip>("Audio/inheritTheLightTrail");
		else if (num == 2) return Resources.Load<AudioClip>("Audio/huwa_slow");
		else if (num == 3) return Resources.Load<AudioClip>("Audio/crystalForest");
		else if (num == 4) return Resources.Load<AudioClip>("Audio/sessionOntheSofa");
		else if (num == 5) return Resources.Load<AudioClip>("Audio/gunshotStraight");
		else if (num == 6) return Resources.Load<AudioClip>("Audio/snow_rabit");
		else if (num == 7) return Resources.Load<AudioClip>("Audio/hammockOfPiece");
		else if (num == 8) return Resources.Load<AudioClip>("Audio/TheSoundofWavesArvent");
		else if (num == 9) return Resources.Load<AudioClip>("Audio/Wmhinst097");
		else if (num == 10) return Resources.Load<AudioClip>("Audio/SpiritofKnowledge");
		return null;
    }
	public int CurrentTimeSeed()
	{
		DateTime dt = System.DateTime.Now;
		return dt.Day + dt.Hour + dt.Minute + dt.Second + dt.Millisecond;
	}
}

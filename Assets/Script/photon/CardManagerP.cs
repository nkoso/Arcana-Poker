using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class CardManagerP : Photon.MonoBehaviour {
	const int handmax = 6;
	public Sprite[] arcana;
	public Sprite cardback;
	[SerializeField] PlayerP player;
	[SerializeField] EnemyP enemy;
	public GameObject gamePanel;	
	public Text turntext;
	public State state { get; set; }
	private BattleTurnP battle;
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

	private PhotonView photonview;
	public Text[] synchtext=new Text[2];
	private bool[] Bsynch = new bool[2];
	public Text MenuT_Phtn;
	
    void Start () {
		
		battle = GetComponent<BattleTurnP>();
		photonview = GetComponent<PhotonView>();
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
		player.Trash.MenuOn();
		enemy.Trash.MenuOn();
		turntext.text = ("TURN : " + (battleturn + 1).ToString());
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
		player.Draw(handmax - player.HandCardNum());
		enemy.Draw(handmax - enemy.HandCardNum());
		//ドローの特別な奴も待機
		yield return new WaitUntil(() => player.synch && enemy.synch && !player.GetHEbuf(156) && !enemy.GetHEbuf(156));
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
		yield return new WaitUntil(() => player.synch&& enemy.synch);
		state = State.Set;
		if (level == Level.expert || level == Level.hard) player.HandSupport();
	}
	public void BattleStart() { StartCoroutine("battleStart"); }
	IEnumerator battleStart()
    {
		player.PlayerUIReset();
		player.Trash.MenuOff();
		enemy.Trash.MenuOff();
		if (level == Level.expert || level == Level.hard) player.SupportReset();
		state = State.Wait;
		photonview.RPC("RPCstart", PhotonTargets.Others);
		
		Bsynch[0] = true;
		yield return new WaitUntil(() => Bsynch[0]&&Bsynch[1]);
		Bsynch[0] = false;
		Bsynch[1] = false;
		player.PlayerUIReset();
		player.FieldCount = 0;
		enemy.FieldCount = 0;
		battle.BattleStart();
	}
    //スタートの同期
    [PunRPC]
    private void RPCstart() { Bsynch[1] = true; }

    public void TurnEnd() { StartCoroutine("turnEnd"); }
    IEnumerator turnEnd()
    {
		player.CardTrash();
		enemy.CardTrash();
		yield return new WaitUntil(() => (enemy.synch&&player.synch));
		Debug.Log("Turn End" + battleturn);
		battleturn++;
		if (battleturn==11　|| player.HP<=0 || enemy.HP<=0)
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
			if (item.GetComponent<Text>() != null && !item.name.Contains("Photon"))
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
	//エスケープ処理と選択状況
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape)&&!battleEndFlag) escape();
		synTextSet(synchtext[0], Bsynch[0]);
		synTextSet(synchtext[1], Bsynch[1]);
	}
	private void synTextSet(Text text, bool flg)
	{
		if (state != State.Battle)
		{
			if (flg) text.text = "選択完了";
			else text.text = "選択中";
		}
		else text.text = "";
	}
	private void escape()
    {
		gamePanel.SetActive(true);
        foreach(Transform item in gamePanel.transform)
        {
			item.gameObject.SetActive(true);
			if (item.GetComponent<Text>() != null && !item.name.Contains("Photon"))
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
	//ゲーム終了後のシーン遷移
	public void Restart() { StartCoroutine("restart"); }
	IEnumerator restart()
	{
		if (battleEndFlag)
		{
            //相手が退出している場合
			if (PhotonNetwork.room.PlayerCount != 2) Exit();

			MenuT_Phtn.text = "待機中";
			SynchCheckM();
			yield return new WaitUntil(() => RPCsynchM);
            //二人とも再開を押している場合
			if (PhotonNetwork.isMasterClient)
			{
				var properties = new ExitGames.Client.Photon.Hashtable();
				properties.Add("Battle", true);
				PhotonNetwork.room.SetCustomProperties(properties);
			}
		}
		else
		{
			foreach(Transform item in gamePanel.transform) item.gameObject.SetActive(false);
			gamePanel.SetActive(false);
		}
	}
    //同期してシーンリロード
	private void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable i_propertiesThatChanged)
	{
		{
			object value = null;
			if (i_propertiesThatChanged.TryGetValue("Battle", out value))
			{
				if ((bool)value)
				{
					SceneManager.sceneLoaded += GameSceneLoaded;
					SceneManager.LoadScene("photonpoker");
				}
			}
		}
	}
	private void GameSceneLoaded(Scene next, LoadSceneMode mode)
	{
		if (next.name == "photonpoker")
		{
			var gameManager = GameObject.FindWithTag("GameManager").GetComponent<CardManagerP>();
			gameManager.nameset(CharaName[0], CharaName[1]);
			gameManager.level = preLevel;
		}
		// イベントから削除
		SceneManager.sceneLoaded -= GameSceneLoaded;
	}
	public void nameset(string pname, string ename)
	{
		spriname[0] = pname;
		spriname[1] = ename;
		player.graphic.sprite = Resources.Load<Sprite>("Character/" + pname + "/" + pname);
		enemy.graphic.sprite = Resources.Load<Sprite>("Character/" + ename + "/" + ename);
	}
	public void Exit()
	{
		PhotonNetwork.Disconnect();
		SceneManager.LoadScene("StartGamen");
	}
	public void Menu()
	{
		PhotonNetwork.Disconnect();
		SceneManager.LoadScene("CharacterS");
	}
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
	//utility
	public int GetBattleTurn() { return battleturn; }
	public int CurrentTimeSeed()
	{
	    System.DateTime dt = System.DateTime.Now;
		return dt.Day + dt.Hour + dt.Minute + dt.Second + dt.Millisecond;
	}
	//photon通信メソッド
	//これはマスター側が制御する
	public void datasynch(string type, string tag)
	{
		if (tag == "Player")
		{
			string pdata = "";
            //ハンドシャッフルのランダムデータ
			if (type == "hand")
			{
				int[] random = new int[handmax];
				for (int i = 0; i < random.Length; i++) random[i] = i;
				random = random.OrderBy(i => System.Guid.NewGuid()).ToArray();
				foreach(int item in random) pdata = pdata + item.ToString() + ",";
			}
            //デッキシャッフルのランダムデータ
			else if (type == "deck")
			{
				List<int> pdeck = player.GetDeck();
				pdeck = pdeck.OrderBy(i => System.Guid.NewGuid()).ToList();
				foreach(int item in pdeck) pdata = pdata + item.ToString() + ",";
			}
            //相手に対戦相手のデータとして伝える
			photonview.RPC("RPCdataReceive", PhotonTargets.Others, pdata, "Enemy", type);
            //自分にプレイヤーのデータを作ったことを知らせる
			RPCdataReceive(pdata, "Player", type);
		}
		else if (tag == "Enemy")
		{
			string edata = "";
			//ハンドシャッフルのランダムデータ
			if (type == "hand")
			{
				int[] random = new int[handmax];
				for (int i = 0; i < random.Length; i++) random[i] = i;
				random = random.OrderBy(i => System.Guid.NewGuid()).ToArray();
				foreach (int item in random) edata = edata + item.ToString() + ",";
			}
			//デッキシャッフルのランダムデータ
			else if (type == "deck")
			{
				List<int> edeck = enemy.GetDeck();
				edeck = edeck.OrderBy(i => System.Guid.NewGuid()).ToList();
				foreach (int item in edeck) edata = edata + item.ToString() + ",";
			}
            //相手に相手のデータとして伝える
			photonview.RPC("RPCdataReceive", PhotonTargets.Others, edata, "Player", type);
			//自分にエネミーのデータを作ったことを知らせる
			RPCdataReceive(edata, "Enemy", type);
		}
	}
    //受け取ったデータ処理
	[PunRPC]
	private void RPCdataReceive(string data, string tag, string type)
	{
		if (tag == "Player")
		{
            //手札のシャッフルデータ
			if (type == "hand")
			{
				string[] phand = data.Split(',');
				int[] ppos = new int[handmax];
				for (int i = 0; i < handmax; i++) ppos[i] = int.Parse(phand[i]);
				player.bufferSet(ppos);
			}
            //デッキのシャッフルデータ
			else if (type == "deck")
			{
				string[] pdatas = data.Split(',');
				List<int> pdeck = new List<int>();
				for (int i = 0; i < pdatas.Length - 1; i++) pdeck.Add(int.Parse(pdatas[i]));
				player.bufferSet(pdeck.ToArray());
			}
		}
		else if (tag == "Enemy")
		{
			//手札のシャッフルデータ
			if (type == "hand")
			{
				string[] ehand = data.Split(',');
				int[] epos = new int[handmax];
				for (int i = 0; i < handmax; i++) epos[i] = int.Parse(ehand[i]);
				enemy.bufferSet(epos);
			}
			//デッキのシャッフルデータ
			else if (type == "deck")
			{
				string[] edatas = data.Split(',');
				List<int> edeck = new List<int>();
				for (int i = 0; i < edatas.Length - 1; i++) edeck.Add(int.Parse(edatas[i]));
				enemy.bufferSet(edeck.ToArray());
			}
		}
	}
	//キャラクターに来るPRCの橋渡しメソッド
	public void CharacterMediator(string type, string tag, string data)
	{
		Debug.Log(data);
		if (type == "Hand2Field")
		{
			enemy.Hand2Field(data);
		}
		else if (type == "Field2Hand")
		{
			enemy.Field2Hand(data);
		}
		else if (type == "Ack")
		{
			if (tag == "Player") enemy.ack = true;
			else if (tag == "Enemy") player.ack = true;
		}
		else if (type == "Select")
		{
			if (tag == "Player") enemy.rPCselectedAction(data);
			else if (tag == "Enemy") player.rPCselectedAction(data);
		}
	}
	//相手に切断された時
	void OnPhotonPlayerDisconnected()
	{
		MenuT_Phtn.text = "相手に切断されました。";
		if (!battleEndFlag)
		{
			enemy.HPChange(-enemy.HP);
			GameEnd();
		}
		else
		{
			StartCoroutine("disconne");
		}
	}
	IEnumerator disconne()
	{
		yield return new WaitForSeconds(1.5f);
		PhotonNetwork.Disconnect();
		SceneManager.LoadScene("CharacterS");
	}
    //ACK同期
	bool RPCsynchM { get; set; }
	public void SynchCheckM() { StartCoroutine("synchCheckM"); }
	IEnumerator synchCheckM()
	{
		RPCsynchM = false;
		if (PhotonNetwork.isMasterClient)
		{
			yield return new WaitUntil(() => ackM);
			ackM = false;
			photonview.RPC("RPCackM", PhotonTargets.Others);
		}
		else
		{
			photonview.RPC("RPCackM", PhotonTargets.Others);
			yield return new WaitUntil(() => ackM);
			ackM = false;
		}
		RPCsynchM = true;
	}
    //ACK信号
	private bool ackM { get; set; }
	[PunRPC]
	public void RPCackM(){ackM = true;}

	
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Maching : MonoBehaviour
{
    [SerializeField] Text connectionText;
    [SerializeField] Text State;
    [SerializeField] Text Mach;
    const int maxplayer = 2;
    //RoomOptions roomop;
    public Level level;
    public string charaName;
    PhotonView photonView;
    private string ename;
    bool synch=false;

    void Start()
    {
        PhotonNetwork.logLevel = PhotonLogLevel.Full;
        photonView = GetComponent<PhotonView>();
        // Photonに接続する(引数でゲームのバージョンを指定できる)
        PhotonNetwork.ConnectUsingSettings(null);
    }
    void Update()
    {
        connectionText.text = PhotonNetwork.connectionStateDetailed.ToString();
        State.text = "Maching......" + System.Environment.NewLine + "Level:" + level.ToString() + System.Environment.NewLine + "Character:" + japanName(charaName);
    }
    
    void OnJoinedLobby()
    {
        Debug.Log("ロビーに入りました。");
        // ルームに入室する
        RoomOptions roomop = new RoomOptions();
        roomop.MaxPlayers = maxplayer;
        roomop.IsOpen = true;
        roomop.IsVisible = true;
        roomop.CustomRoomPropertiesForLobby = new string[] { "lv" };
        roomop.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable { { "lv",(int)level } };
        Debug.Log(level.ToString());
        PhotonNetwork.JoinOrCreateRoom("Test"+level.ToString(), roomop, TypedLobby.Default);
    }
    
    void OnJoinedRoom()
    {
        Debug.Log("ルームへ入室しました。");
    }

    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        photonView.RPC("RPCmaching", PhotonTargets.Others, charaName);
    }
    
    [PunRPC]
    private void RPCmaching(string cname)
    {
        ename = cname;
        Mach.text = "Maching......" + System.Environment.NewLine + "Level:" + level.ToString() + System.Environment.NewLine + "Character:" + japanName(cname);
        //参加側が受け取った場合、マスターに返す マスターが動くと、ゲームを起動
        if (PhotonNetwork.isMasterClient)
        {
            //photonView.RPC("RPCstart", PhotonTargets.All);
            var properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("Battle", true);
            PhotonNetwork.room.SetCustomProperties(properties);
        }
        else
        {
            photonView.RPC("RPCmaching", PhotonTargets.Others, charaName);
        }
    }
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
            var manager = GameObject.FindWithTag("GameManager").GetComponent<CardManagerP>();
            manager.nameset(charaName, ename);
            manager.level = level;
        }
        SceneManager.sceneLoaded -= GameSceneLoaded;
    }
    public void CanselButton()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("CharacterS");
    }
    
    private string japanName(string name)
    {
        if (name.Contains("reimu")) return "霊夢";
        else if (name.Contains("marisa")) return "魔理沙";
        else if (name.Contains("sakuya")) return "咲夜";
        else if (name.Contains("youmu")) return "妖夢";
        else if (name.Contains("remiria")) return "レミリア";
        else if (name.Contains("fran")) return "フラン";
        else if (name.Contains("yuyuko")) return "幽々子";
        else if (name.Contains("sanae")) return "早苗";
        else if (name.Contains("satori")) return "さとり";
        else if (name.Contains("koisi")) return "こいし";
        else return "Name";
    }
}

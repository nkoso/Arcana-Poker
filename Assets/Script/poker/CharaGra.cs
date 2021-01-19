using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharaGra : MonoBehaviour
{
    private GameObject text;

    // Start is called before the first frame update
    void Start()
    {
        var trigger = GetComponent<EventTrigger>();
        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => {
            NameSet();
        });
        var entry1 = new EventTrigger.Entry();
        entry1.eventID = EventTriggerType.PointerExit;
        entry1.callback.AddListener((data) =>
        {
            NameReset();
        });
        trigger.triggers.Add(entry);
        trigger.triggers.Add(entry1);
    }
    public void NameSet()
    {
        text = Instantiate((GameObject)Resources.Load("Prefab/Text/CharaName"));
        text.transform.SetParent(transform, false);
        text.GetComponent<Text>().text = Gra2Name(GetComponent<Image>().sprite.name);
        text.transform.localPosition = new Vector3(170f, 0f, 0f);
        text.transform.localRotation = transform.rotation;
    }
    public void NameReset()
    {
        Destroy(text);
    }
    private string Gra2Name(string name)
    {
        if (name.Contains("reimu")) return "楽園の素敵な巫女";
        else if (name.Contains("marisa")) return "普通の魔法使い";
        else if (name.Contains("sakuya")) return "完全で瀟洒なメイド";
        else if (name.Contains("youmu")) return "半人半霊の庭師";
        else if (name.Contains("remiria")) return "永遠に紅い幼き月";
        else if (name.Contains("fran")) return "悪魔の妹";
        else if (name.Contains("yuyuko")) return "華胥の亡霊";
        else if (name.Contains("sanae")) return "祀られる風の人間";
        else if (name.Contains("satori")) return "怨霊も恐れ怯む少女";
        else if (name.Contains("koisi")) return "閉じた恋の瞳";
        else return "名前";
    }
    
}

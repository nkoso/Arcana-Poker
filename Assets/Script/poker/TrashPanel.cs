using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrashPanel : MonoBehaviour
{
    public Transform content;
    private GameObject panel0;

    private void Awake()
    {
        var trigger = GetComponent<EventTrigger>();
        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) => {
            Reset();
        });
        trigger.triggers.Add(entry);
    }
    public void ListSet(List<string> list,GameObject panel)
    {
        panel0 = panel;
        for (int i = 0; i < list.Count; i++)
        {
            GameObject node = Instantiate((GameObject)Resources.Load("Prefab/TrashNode"));
            node.transform.SetParent(content, false);
            node.GetComponent<TrashNode>().Set(list[i], panel0);
        }
    }
    public void Reset()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        Destroy(this.gameObject);
    }
}

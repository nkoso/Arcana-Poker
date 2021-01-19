using System.Collections.Generic;
using UnityEngine;

/*Setactiveして、ListSetする。
 * 使い終わったらReset
 */
public class SelectivePanel : MonoBehaviour
{
    private List<string> trash = new List<string>();
    private List<int> deck = new List<int>();
    public Transform content;
    public GameObject panel0;

    public void SetTrash(List<string> list)
    {
        trash = list;
    }
    public void SetDeck(List<int> list)
    {
        deck = list;
    }
    public void ListSet()
    {
        gameObject.SetActive(true);
        if (deck.Count>0)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                GameObject node = Instantiate((GameObject)Resources.Load("Prefab/SelectNode"));
                node.transform.SetParent(content, false);
                node.GetComponent<SelectNode>().Set(num2arcana(deck[i]), panel0);
            }

        }
        else if (trash.Count>0)
        {
            for (int i = 0; i < trash.Count; i++)
            {
                GameObject node = Instantiate((GameObject)Resources.Load("Prefab/SelectNode"));
                node.transform.SetParent(content, false);
                node.GetComponent<SelectNode>().Set(trash[i], panel0);
            }
        }
    }

    public void Reset()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        gameObject.SetActive(false);
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
}

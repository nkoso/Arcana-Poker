using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*デッキゾーンの表示管理
 */
public class Deckzone : MonoBehaviour
{
    private Character chara;
    private GameObject card;
    private CardManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindWithTag("GameManager").GetComponent<CardManager>();
        chara = transform.root.gameObject.GetComponent<Character>();
        card = new GameObject("Card");
        card.AddComponent<SpriteRenderer>().sprite = manager.cardback;
        card.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        card.transform.parent = transform;
        card.transform.rotation = card.transform.parent.rotation;
        Vector3 pos = Vector3.zero;
        card.transform.localPosition = pos;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (chara.GetDeck().Count>0) card.SetActive(true);
        else card.SetActive(false);
    }
    
}

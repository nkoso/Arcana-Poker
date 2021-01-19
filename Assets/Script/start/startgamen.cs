using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class startgamen : MonoBehaviour
{
    public GameObject panel;
    public void Battle()
    {
        StartCoroutine("battle");
    }
    IEnumerator battle()
    {
        panel.SetActive(true);
        Animation anim = panel.GetComponent<Animation>();
        anim.Play("seni");
        yield return new WaitUntil(() => !anim.isPlaying);
        SceneManager.LoadScene("CharacterS");
    }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
      UnityEngine.Application.Quit();
#endif
    }
}

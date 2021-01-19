using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
 * ダメージテキスト用スクリプト
 * play()で上に少し移動して消える。
 * テキストの中身は外から設定する。
 */
public class TextPop : MonoBehaviour
{
    Animation anim;
    private void Awake()
    {
        anim = GetComponent<Animation>();
    }
    public void Play()
    {
        StartCoroutine("play");
    }
    IEnumerator play()
    {
        GetComponent<Text>().enabled = true;
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;
        AnimationCurve curvey = AnimationCurve.Linear(0.0f, transform.localPosition.y, 1.0f, transform.localPosition.y + 18.0f);
        AnimationCurve curvex = AnimationCurve.Linear(0.0f, transform.localPosition.x, 1.0f, transform.localPosition.x);
        string path = GetHierarchyPath(transform);
        clip.SetCurve(path, typeof(Transform), "localPosition.y", curvey);
        clip.SetCurve(path, typeof(Transform), "localPosition.x", curvex);
        anim.AddClip(clip, clip.name);
        anim.Play(clip.name);
        yield return new WaitUntil(() => !anim.isPlaying);
        Destroy(this.gameObject);
    }
    private static string GetHierarchyPath(Transform self)
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
}

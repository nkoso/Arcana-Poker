using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 初期位置からtargetに動いて、消えるテキスト
 * textの中身は外から変える必要がある。
 */
public class adtextPop : MonoBehaviour
{
    Animation anim;
    public bool synch { get; set; }
    private void Start()
    {
        
       
    }
    public void Play()
    {
        StartCoroutine("play");
    }
    IEnumerator play()
    {
        synch = false;
        anim = GetComponent<Animation>();
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;
        //AnimationCurve curvey = AnimationCurve.Linear(0.0f, transform.localPosition.y, 0.5f, target.localPosition.y);
        AnimationCurve curvey = AnimationCurve.Linear(0.0f, transform.localPosition.y, 0.5f, 0.0f);
        AnimationCurve curvex = AnimationCurve.Linear(0.0f, transform.localPosition.x, 0.5f, transform.localPosition.x);
        string path = GetHierarchyPath(transform);
        clip.SetCurve(path, typeof(Transform), "localPosition.y", curvey);
        clip.SetCurve(path, typeof(Transform), "localPosition.x", curvex);
        anim.AddClip(clip, clip.name);
        anim.Play(clip.name);
        yield return new WaitUntil(() => !anim.isPlaying);
        synch = true;
        Destroy(gameObject);
    }
    string GetHierarchyPath(Transform self)
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

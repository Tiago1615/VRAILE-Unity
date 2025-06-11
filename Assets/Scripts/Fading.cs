using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Fading : MonoBehaviour
{
    public bool fadeOnStart = true;
    public int fadeDuration = 2;
    public Color fadeColor;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("Renderer not found on the object.");
            return;
        }
    }

    void Start()
    {
        if (fadeOnStart)
        {
            gameObject.SetActive(true);
            FadeIn(() => {gameObject.SetActive(false);});
        }
    }

    public void FadeIn(Action onComplete = null)
    {
        StartCoroutine(Fade(1, 0, onComplete));
    }

    public void FadeOut(Action onComplete = null)
    {
        StartCoroutine(Fade(0, 1, onComplete));
    }

    private IEnumerator Fade(float alphaIn, float alphaOut, Action onComplete)
    {
        float timer = 0;

        while (timer <= fadeDuration)
        {
            timer += Time.deltaTime;

            Color newColor = fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);
            rend.material.SetColor("_Color", newColor);

            yield return null;
        }

        Color finalColor = fadeColor;
        finalColor.a = alphaOut;
        rend.material.SetColor("_Color", finalColor);

        onComplete?.Invoke();
    }

}

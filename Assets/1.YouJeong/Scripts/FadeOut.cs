using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeOut : MonoBehaviour
{
    public Image fadeImage;           // 전체화면을 덮는 Image (검정색)
    public float fadeDuration = 1.5f; // 원하는 페이드 시간
    public Ease fadeEase = Ease.InOutQuad; // DOTween 이징

    void Start()
    {
    }

    public void Fade(System.Action onComplete = null)
    {
        fadeImage.DOFade(1f, fadeDuration)
                 .SetEase(fadeEase)
                 .OnStart(() => fadeImage.raycastTarget = true) // 클릭 차단
                 .OnComplete(() => onComplete?.Invoke());
    }

    public void startFade()
    {
        Fade(() => {
            Debug.Log("FadeOut Complete");
            // 여기서 씬 로딩
            SceneManager.LoadScene("SpaceShipLauncher");
        });

    }
}

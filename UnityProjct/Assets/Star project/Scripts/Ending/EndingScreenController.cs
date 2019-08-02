using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingScreenController : MonoBehaviour
{
    // フェード関係
    [Header("フェード関係")]
    [SerializeField] private GameObject fadeImageObj = null;
    [SerializeField] private GameObject fadeText = null;
    [SerializeField] private GameObject fadeChara = null;
    private Image fadeImage;
    [SerializeField] private Color fadeOutColor;
    [SerializeField] private float fadeOutTime;
    [SerializeField] private Color fadeInColor;
    [SerializeField] private float fadeInTime;

    void Start()
    {
        StartCoroutine(EndingStartEnumerator());
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("SelectOk"))
        {
            StartCoroutine(ExitTitleEnumerator());
        }
    }
    /// <summary>
    /// フェードインしてからこのシーンをスタート
    /// </summary>
    /// <returns></returns>
    private IEnumerator EndingStartEnumerator()
    {
        fadeImage = fadeImageObj.GetComponent<Image>();
        yield return FadeInEnumerator(fadeInTime);
    }
    /// <summary>
    /// タイトル遷移時にフェードを入れてから遷移
    /// </summary>
    /// <returns></returns>
    private IEnumerator ExitTitleEnumerator()
    {
        yield return FadeOutEnumerator(fadeOutColor, fadeInTime);
        SceneManager.LoadScene("TitleScene");
    }
    /// <summary>
    /// フェードイン
    /// </summary>
    /// <param name="period">フェード時間</param>
    /// <returns></returns>
    private IEnumerator FadeInEnumerator(float period)
    {
        ForceColor(fadeInColor);
        fadeImageObj.transform.SetAsLastSibling();
        yield return FadeEnumerator(fadeImage.color, Color.clear, period);
        FadeImageDisplay(false);
        FadeImagecharaDisplay(false);
    }
    /// <summary>
    /// フェードアウト
    /// </summary>
    /// <param name="color">最終カラー</param>
    /// <param name="period">フェード時間</param>
    /// <returns></returns>
    private IEnumerator FadeOutEnumerator(Color color, float period)
    {
        FadeImagecharaDisplay(true);
        FadeImageDisplay(true);
        fadeImageObj.transform.SetAsLastSibling();
        yield return FadeEnumerator(Color.clear, color, period);
    }
    /// <summary>
    /// フェード実体
    /// </summary>
    /// <param name="startColor">初期カラー</param>
    /// <param name="targetColor">最終カラー</param>
    /// <param name="period">フェード時間</param>
    /// <returns></returns>
    private IEnumerator FadeEnumerator(Color startColor, Color targetColor, float period)
    {
        float t = 0;
        while (t < period)
        {

            t += Time.deltaTime;
            Color color = Color.Lerp(startColor, targetColor, t / period);
            fadeImage.color = color;
            yield return null;
        }

        fadeImage.color = targetColor;
    }
    /// <summary>
    /// フェードカラー初期化用
    /// </summary>
    /// <param name="color">初期化カラー</param>
    private void ForceColor(Color color)
    {
        FadeImageDisplay(true);
        FadeImagecharaDisplay(true);
        fadeImageObj.transform.SetAsLastSibling();
        fadeImage.color = color;
    }
    /// <summary>
    /// フェード時フェードキャラクターを表示非表示します
    /// </summary>
    /// <param name="isFade">表示するかどうか</param>
    private void FadeImagecharaDisplay(bool isFade)
    {
        fadeText.SetActive(isFade);
        fadeChara.SetActive(isFade);
    }
    /// <summary>
    /// フェードImageの表示非表示
    /// </summary>
    /// <param name="isDysplay">表示非表示</param>
    private void FadeImageDisplay(bool isDysplay)
    {
        fadeImageObj.SetActive(isDysplay);
    }
}

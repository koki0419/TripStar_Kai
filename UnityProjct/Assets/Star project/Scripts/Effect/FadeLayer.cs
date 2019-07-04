using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面フェード用のクラス（外部参照してください）
/// </summary>
public class FadeLayer : MonoBehaviour
{
    //フェード用のImageを取得
    [SerializeField] private Image image = null;

    /// <summary>
    /// フェードするときに最初の開始カラーをセット
    /// </summary>
    public void ForceColor(Color color)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        image.color = color;
    }
    /// <summary>
    /// フェードイン用のコルーチン
    /// </summary>
    /// <param name="period">フェード時間</param>
    /// <returns></returns>
    public IEnumerator FadeInEnumerator(float period)
    {
        transform.SetAsLastSibling();
        yield return FadeEnumerator(image.color, Color.clear, period);
        gameObject.SetActive(false);
    }
    /// <summary>
    /// フェードアウト用のコルーチン
    /// </summary>
    /// <param name="color">フェードアウトする際の最終色</param>
    /// <param name="period">フェード時間</param>
    /// <returns></returns>
    public IEnumerator FadeOutEnumerator(Color color, float period)
    {
        transform.SetAsLastSibling();
        yield return FadeEnumerator(Color.clear, color, period);
    }
    /// <summary>
    /// フェード本体
    /// </summary>
    /// フェード終了までReturnされない
    /// <param name="startColor">フェードスタート色</param>
    /// <param name="targetColor">フェード終了色</param>
    /// <param name="period">フェード時間</param>
    /// <returns></returns>
    public IEnumerator FadeEnumerator(Color startColor, Color targetColor, float period)
    {
        float t = 0;
        while (t < period)
        {

            t += Time.deltaTime;
            Color color = Color.Lerp(startColor, targetColor, t / period);
            image.color = color;
            yield return null;
        }

        image.color = targetColor;
    }

}

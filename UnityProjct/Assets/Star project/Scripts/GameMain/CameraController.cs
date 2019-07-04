using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    //プレイヤーのTransformを取得
    [SerializeField] private Transform player = null;
    private float camaraPos = 6;
    private float camaraMove = 1.5f;
    public void Init()
    {
        var position = transform.position;
        position.x = player.position.x;
    }

    // Update is called once per frame
    public void MoveUpdate(float deltaTime)
    {
        var position = transform.position;
        position.x += camaraMove * deltaTime;
        position.y = camaraPos;
        transform.position = position;
    }
    /// <summary>
    /// カメラを振動させます
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="magnitude"></param>
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(DoShake(duration, magnitude));
    }
    private IEnumerator DoShake(float duration, float magnitude)
    {
        var pos = transform.localPosition;

        var elapsed = 0f;

        while (elapsed < duration)
        {
            var x = pos.x + Random.Range(-1f, 1f) * magnitude;
            var y = pos.y + Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, pos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = pos;
    }
}

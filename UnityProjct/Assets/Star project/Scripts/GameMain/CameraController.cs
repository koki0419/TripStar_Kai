using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    // プレイヤーのTransformを取得
    [SerializeField] private Transform player = null;
    private float cameraPos = 6;
    private float cameraMove = 2.0f;
    [SerializeField] private float zoomSpeed;
    // 初期位置
    private Vector3 initialPosition;
    // 移動するかどうか
    public bool IsMove
    {
        get; set;
    }
    // 初期化
    public void Init()
    {
        var position = transform.position;
        position.x = player.position.x;
        //初期位置を取得します。
        initialPosition = transform.position;
    }
    // カメラの移動
    public void MoveUpdate(float deltaTime, bool isCameraMove)
    {
        if (isCameraMove)
        {
            var position = transform.position;
            position.x += cameraMove * deltaTime;
            position.y = cameraPos;
            transform.position = position;
        }
        else
            return;
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
    // カメラをShake
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

    public void CameraZoomUp()
    {
        var playerPos = player.position;
        var vector3 = playerPos - transform.position;
        var nVector3 = vector3.normalized;
        if (transform.position.z <= -20.0f)
        {
            transform.position += nVector3 * zoomSpeed;
        }
    }

    public void CameraZoomDown()
    {
        var playerPos = player.position;
        var vector3 = playerPos - transform.position;
        var nVector3 = vector3.normalized;
        if (transform.position.z >= initialPosition.z)
        {
            transform.position -= nVector3 * zoomSpeed;
        }
    }
}

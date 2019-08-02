using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectPool))]
public class ObstacleSpawn : MonoBehaviour
{
    [SerializeField] private PlayerMove playerMove = null;
    [SerializeField] private GameObject playerObj = null;
    [SerializeField] private Camera tragetCamera = null;
    [Header("障害物プール生成数")]
    [SerializeField] private int spawnMax;
    public int SpawnMax
    {
        get { return spawnMax; }
    }
    [Header("画面内に最大何個障害物を表示するか")]
    [SerializeField] private int obstaclesDysplayCount;
    [Header("障害物プレハブ")]
    [SerializeField] private GameObject obstaclesPrefab = null;
    private ObjectPool pool;
    // 攻撃時のスピード
    private readonly float normalAttackMoveSpeed = 5000;
    // 戻るスピード
    private readonly float airAttackMoveSpeed = 1000;

    // ☆現在の表示数
    [HideInInspector]
    public int ActiveCount
    {
        set; get;
    }
    // ☆生成数（経過）→次生成する☆のインデックス
    private int spawnIndex = 0;
    [SerializeField] private EnemyData enemyData = null;
    // エネミーobjectPoolの初期化
    public void Init()
    {
        pool = GetComponent<ObjectPool>();
        pool.CreatePool(obstaclesPrefab, spawnMax);
        CreatObstacle();
    }
    // エネミーを生成する
    public void CreatObstacle()
    {
        if (spawnIndex < enemyData.enemyDatas.Count)
        {
            while (ActiveCount < obstaclesDysplayCount)
            {
                var obstacle = pool.GetObject();
                if (obstacle == null)
                {
                    break;
                }
                // 生成するときに「Rigidbody」がなければAddする
                if (obstacle.GetComponent<Rigidbody>() == null) { obstacle.AddComponent<Rigidbody>(); }
                // 初期化
                var setEnemyDatas = enemyData.enemyDatas[spawnIndex];
                var newObj_O = obstacle.GetComponent<ObstacleManager>();
                obstacle.transform.localPosition = setEnemyDatas.enemy_Position;
                newObj_O.SetObstacleDatas(this.tragetCamera, this.playerMove, this, setEnemyDatas.enemy_Hp, setEnemyDatas.enemy_AppearStarNum);
                newObj_O.Init();
                var newObj_E = obstacle.GetComponent<EnemyController>();
                newObj_E.SetEnemyDatas(setEnemyDatas.enemy_Type, setEnemyDatas.enemy_MoveVector, setEnemyDatas.enemy_MoveSpeed, this.normalAttackMoveSpeed,
                                       this.airAttackMoveSpeed, setEnemyDatas.enemy_AttackRugTime);
                newObj_E.Init(this.playerObj);
                spawnIndex++;
                ActiveCount++;
            }
        }
        else return;
    }
    /// <summary>
    /// オブジェクトを生成します
    /// </summary>
    public void ObstaclesSponUpdate()
    {
        CreatObstacle();
    }
}

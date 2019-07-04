using UnityEngine;

[RequireComponent(typeof(ObjectPool))]
public class ObstacleSpawn : MonoBehaviour
{
    //プレイヤースクリプト
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
    [SerializeField] private GameObject obstaclesPrefab;
    private ObjectPool pool;
    [Header("障害物生成場所と各障害物HPと壊した際の☆生成数")]
    [SerializeField] private int cleatEnemyNumMax = 0;//いくつエネミーを作るのか
    [SerializeField] private Vector3[] obataclesSponPosition;
    [SerializeField] private int[] obataclesHp;
    [SerializeField] private int[] spawnStarNum;

    /// <summary>
    /// エネミーのタイプ
    /// </summary>
    private enum EnemyTyp
    {
        None,
        NotMoveEnemy,//静動エネミー
        MoveEnemy,   //動くエネミー
        AirMoveEnemy,//空飛ぶエネミー
    }
    [SerializeField] private EnemyTyp[] enemyTyp;
    // 移動距離
    [SerializeField] private Vector3[] amountOfMovement;
    //移動スピード
    [Header("各状態の移動速度")]
    [SerializeField] private float[] searchMoveSpeed;
    //攻撃時のスピード
    private float lockOnMoveSpeed = 5000;
    //戻るスピード
    private float attackUpOnMoveSpeed = 1000;
    //[SerializeField] private float[] removeMoveSpeed;
    //攻撃時待機時間
    [SerializeField] private float[] defaultAttackTime;

    //☆現在の表示数
    [HideInInspector]
    public int activeCount
    {
        set; get;
    }
    //☆生成数（経過）→次生成する☆のインデックス
    private int spawnIndex = 0;

    //データファイル名
    [SerializeField] string fileName = "";
    //csvデータ
    private CsvlInport csvInport = new CsvlInport();

    //エネミーobjectPoolの初期化
    public void Init()
    {
        pool = GetComponent<ObjectPool>();
        pool.CreatePool(obstaclesPrefab, spawnMax);

        enemyTyp = new EnemyTyp[cleatEnemyNumMax];
        obataclesSponPosition = new Vector3[cleatEnemyNumMax];
        obataclesHp = new int[cleatEnemyNumMax];
        amountOfMovement = new Vector3[cleatEnemyNumMax];
        searchMoveSpeed = new float[cleatEnemyNumMax];
        defaultAttackTime = new float[cleatEnemyNumMax];
        spawnStarNum = new int[cleatEnemyNumMax];

        //データファイルを読み込み
        csvInport.DateRead(fileName);
        //ポジション＆ポイントデータを代入
        int index = 2;
        for (int i = 1; i < csvInport.csvDatas.Count-1; i++)
        {
            index = 2;
            switch (csvInport.csvDatas[i][index])
            {
                case "None":
                    enemyTyp[i - 1] = EnemyTyp.None;
                    break;
                case "NotMoveEnemy":
                    enemyTyp[i - 1] = EnemyTyp.NotMoveEnemy;
                    break;
                case "MoveEnemy":
                    enemyTyp[i - 1] = EnemyTyp.MoveEnemy;
                    break;
                case "AirMoveEnemy":
                    enemyTyp[i - 1] = EnemyTyp.AirMoveEnemy;
                    break;

            }
            index++;
            // 配置座標を取得
            obataclesSponPosition[i - 1].x = float.Parse(csvInport.csvDatas[i][index]);
            index++;
            obataclesSponPosition[i - 1].y = float.Parse(csvInport.csvDatas[i][index]);
            index++;
            obataclesSponPosition[i - 1].z = float.Parse(csvInport.csvDatas[i][index]);
            index++;
            // 移動方向と距離
            amountOfMovement[i - 1].x = float.Parse(csvInport.csvDatas[i][index]);
            index++;
            amountOfMovement[i - 1].y = 0;
            amountOfMovement[i - 1].z = 0;
            // HPを取得
            // obataclesHp[i - 1] = int.Parse(csvInport.csvDatas[i][index]);
            Debug.Log(csvInport.csvDatas[i][index]);
            index++;
            // 徘徊速さ
            searchMoveSpeed[i - 1] = float.Parse(csvInport.csvDatas[i][index]);
            index++;
            // 攻撃時待機時間
            defaultAttackTime[i - 1] = float.Parse(csvInport.csvDatas[i][index]);
            index++;
            //破壊時の☆出現数
            spawnStarNum[i - 1] = int.Parse(csvInport.csvDatas[i][index]);
            index++;
        }

        CreatObstacle();
    }
    //エネミーを生成する
    public void CreatObstacle()
    {
        if (spawnIndex < obataclesSponPosition.Length - 1)
        {
            while (activeCount < obstaclesDysplayCount)
            {
                var obstacle = pool.GetObject();
                if (obstacle != null)
                {
                    //生成するときに「Rigidbody」がなければAddする
                    if (obstacle.GetComponent<Rigidbody>() == null) { obstacle.AddComponent<Rigidbody>(); }
                    //初期化
                    obstacle.transform.localPosition = obataclesSponPosition[spawnIndex];
                    obstacle.GetComponent<ObstacleManager>().playerMove = this.playerMove;
                    obstacle.GetComponent<ObstacleManager>().tragetCamera = this.tragetCamera;
                    obstacle.GetComponent<ObstacleManager>().foundationHP = this.obataclesHp[spawnIndex];
                    obstacle.GetComponent<ObstacleManager>().spawnStarNum = this.spawnStarNum[spawnIndex];
                    obstacle.GetComponent<ObstacleManager>().obstacleSpawn = this;
                    obstacle.GetComponent<ObstacleManager>().Init();
                    switch (this.enemyTyp[spawnIndex])
                    {
                        case EnemyTyp.NotMoveEnemy:
                            obstacle.GetComponent<EnemyController>().enemyTyp = EnemyController.EnemyTyp.NotMoveEnemy;
                            break;
                        case EnemyTyp.MoveEnemy:
                            obstacle.GetComponent<EnemyController>().enemyTyp = EnemyController.EnemyTyp.MoveEnemy;
                            break;
                        case EnemyTyp.AirMoveEnemy:
                            obstacle.GetComponent<EnemyController>().enemyTyp = EnemyController.EnemyTyp.AirMoveEnemy;
                            break;
                    }

                    obstacle.GetComponent<EnemyController>().amountOfMovement = this.amountOfMovement[spawnIndex];
                    obstacle.GetComponent<EnemyController>().searchMoveSpeed = this.searchMoveSpeed[spawnIndex];
                    obstacle.GetComponent<EnemyController>().lockOnMoveSpeed = this.lockOnMoveSpeed;
                    obstacle.GetComponent<EnemyController>().attackUpOnMoveSpeed = this.attackUpOnMoveSpeed;
                    obstacle.GetComponent<EnemyController>().defaultAttackTime = this.defaultAttackTime[spawnIndex];
                    obstacle.GetComponent<EnemyController>().Init(this.playerObj);
                    spawnIndex++;
                    activeCount++;
                }
            }
        }
        else return;
    }

    public void ObstaclesSponUpdate()
    {
        CreatObstacle();
    }
}

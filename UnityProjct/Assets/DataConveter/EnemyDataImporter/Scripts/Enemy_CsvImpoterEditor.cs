using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;



[CustomEditor(typeof(EnemyCsvImporter))]
public class Enemy_CsvImpoterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 対象のScriptableObjectの参照をセットしました。
        // InspectorウィンドウでセットしたCSVファイルを参照することができます。
        var csvImpoter = target as EnemyCsvImporter;

        // パーツ要素を表示します。
        DrawDefaultInspector();

        // csvからエネミーのデータを作成します。
        if (GUILayout.Button("エネミーデータの作成"))
        {
            // Debug.Log("星データの作成ボタンが押された");
            // データの作成
            SetCsvDataToScriptableObject(csvImpoter);
        }
    }

    /// <summary>
    /// csvデータを編集します。
    /// </summary>
    void SetCsvDataToScriptableObject(EnemyCsvImporter csvImporter)
    {
        // ボタンを押したらパース実行
        if (csvImporter.csvFile == null)
        {
            Debug.LogWarning(csvImporter.name + " : 読み込むCSVファイルがセットされていません。");
            return;
        }

        // csvファイルをstring形式に変換
        string csvText = csvImporter.csvFile.text;

        // 各行ごとにパース（解析する→分割）
        string[] afterParse = csvText.Split('\n');

        // 行数をIDをしてファイルを作成
        // 「0003」のように0で埋めた4桁
        string fileName = "enemyData_ " + csvImporter.StageName.ToString() + ".asset";

        // ここでエラーが起こりやすい try catchを使用すると丁寧
        string path = "Assets/DataConveter/EnemyDataImporter/Resources/" + fileName;

        // EnemyDataのインスタンスをメモリ上に作成
        var enemyData = CreateInstance<EnemyData>();
        enemyData.enemyDatas = new List<EnemyDataEntiry>();
        // 使用するリストの要素数を追加します
        for (int i = 0; i < afterParse.Length-2; i++)
        {
            EnemyDataEntiry data = new EnemyDataEntiry();
            enemyData.enemyDatas.Add(data);
        }

        // ヘッダー行を除いてインポート
        for (int i = 1; i < afterParse.Length; i++)
        {
            // カンマ区切りでデータを取得します。
            string[] parseByComma = afterParse[i].Split(',');

            // columnに値を加算し、相対的に列をずらしています。
            // というのも、元のデータで列の追加があった場合、修正が簡単になるためです。
            int column = 0;

            // 先頭の行が空であれば読み込まない
            if (parseByComma[column] == "")
            {
                continue;
            }
            // エネミーのID
            enemyData.enemyDatas[i - 1].enemy_id = int.Parse(parseByComma[column]);
            column++;
            // エネミーの名前
            enemyData.enemyDatas[i - 1].enemy_Name = parseByComma[column];
            column++;
            // エネミーの種類
            enemyData.enemyDatas[i - 1].enemy_Type = parseByComma[column];
            column++;
            // エネミーの位置座標
            enemyData.enemyDatas[i - 1].enemy_Position.x = float.Parse(parseByComma[column]);
            column++;
            enemyData.enemyDatas[i - 1].enemy_Position.y = float.Parse(parseByComma[column]);
            column++;
            enemyData.enemyDatas[i - 1].enemy_Position.z = float.Parse(parseByComma[column]);
            column++;
            // エネミーの移動方向と距離
            enemyData.enemyDatas[i - 1].enemy_MoveVector.x = float.Parse(parseByComma[column]);
            column++;
            // エネミーのHP
            enemyData.enemyDatas[i - 1].enemy_Hp = int.Parse(parseByComma[column]);
            column++;
            // エネミーの移動速度
            enemyData.enemyDatas[i - 1].enemy_MoveSpeed = float.Parse(parseByComma[column]);
            column++;
            // エネミーの攻撃ラグ時間
            enemyData.enemyDatas[i - 1].enemy_AttackRugTime = float.Parse(parseByComma[column]);
            column++;
            // 破壊時出現星数
            enemyData.enemyDatas[i - 1].enemy_AppearStarNum = int.Parse(parseByComma[column]);
        }


        // インスタンス化したものをassetとして保存
        var asset = (EnemyData)AssetDatabase.LoadAssetAtPath(path, typeof(EnemyData));
        if (asset == null)
        {
            // 指定のパスにファイルが存在しない場合は新規作成
            AssetDatabase.CreateAsset(enemyData, path);
        }
        else
        {
            // 指定のパスに既に同名のファイルが存在する場合は更新
            EditorUtility.CopySerialized(enemyData, asset);
            AssetDatabase.SaveAssets();
        }
        // AssetDatabaseのリフレッシュを行うことで、
        // 作ったアセットがProjectウィンドウに表示されるようになります。
        AssetDatabase.Refresh();
        Debug.Log(csvImporter.name + " : エネミーデータの作成が完了しました。");
    }
}
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(StarCsvImporter))]
public class Star_CsvImpoterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //対象のScriptableObjectの参照をセットしました。
        //InspectorウィンドウでセットしたCSVファイルを参照することができます。
        var csvImpoter = target as StarCsvImporter;

        //パーツ要素を表示します。
        DrawDefaultInspector();

        //csvから星のデータを作成します。
        if (GUILayout.Button("星データの作成"))
        {
            // Debug.Log("星データの作成ボタンが押された");
            //データの作成
            SetCsvDataToScriptableObject(csvImpoter);
        }
    }

    /// <summary>
    /// csvデータを編集します。
    /// </summary>
    void SetCsvDataToScriptableObject(StarCsvImporter csvImporter)
    {
        //ボタンを押したらパース実行
        if (csvImporter.csvFile == null)
        {
            Debug.LogWarning(csvImporter.name + " : 読み込むCSVファイルがセットされていません。");
            return;
        }

        //csvファイルをstring形式に変換
        string csvText = csvImporter.csvFile.text;

        //各行ごとにパース（解析する→分割）
        string[] afterParse = csvText.Split('\n');

        //アセット名
        string fileName = "starData_ " + csvImporter.StageName.ToString() + ".asset";

        //ここでエラーが起こりやすい try catchを使用すると丁寧
        string path = "Assets/DataConveter/StarDataImporter/Resources/" + fileName;

        //StarDataのインスタンスをメモリ上に作成
        var starData = CreateInstance<StarData>();
        starData.starDatas = new List<StarDataEntiry>();
        //使用するリストの要素数を追加します
        for (int i = 0; i < afterParse.Length - 2; i++)
        {
            StarDataEntiry data = new StarDataEntiry();
            starData.starDatas.Add(data);
        }


        //ヘッダー行を除いてインポート
        for (int i = 1; i < afterParse.Length; i++)
        {
            //カンマ区切りでデータを取得します。
            string[] parseByComma = afterParse[i].Split(',');

            //columnに値を加算し、相対的に列をずらしています。
            //というのも、元のデータで列の追加があった場合、修正が簡単になるためです。
            int column = 0;

            //先頭の行が空であれば読み込まない
            if (parseByComma[column] == "")
            {
                continue;
            }

            // id
            starData.starDatas[i - 1].star_id = int.Parse(parseByComma[column]);
            column++;
            // 星の座標
            starData.starDatas[i - 1].star_Position.x = float.Parse(parseByComma[column]);
            column++;
            starData.starDatas[i - 1].star_Position.y = float.Parse(parseByComma[column]);
            column++;
            starData.starDatas[i - 1].star_Position.z = float.Parse(parseByComma[column]);
            column++;
            // 獲得ポイント
            starData.starDatas[i - 1].star_Point = int.Parse(parseByComma[column]);

            //インスタンス化したものをassetとして保存
            var asset = (StarData)AssetDatabase.LoadAssetAtPath(path, typeof(StarData));
            if (asset == null)
            {
                //指定のパスにファイルが存在しない場合は新規作成
                AssetDatabase.CreateAsset(starData, path);
            }
            else
            {
                //指定のパスに既に同名のファイルが存在する場合は更新
                EditorUtility.CopySerialized(starData, asset);
                AssetDatabase.SaveAssets();
            }
            //AssetDatabaseのリフレッシュを行うことで、
            //作ったアセットがProjectウィンドウに表示されるようになります。
            AssetDatabase.Refresh();
        }
        Debug.Log(csvImporter.name + " : 星データの作成が完了しました。");
    }
}
#endif
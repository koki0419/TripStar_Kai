using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
/*
 * ***使用方法***
 * ①csvファイルを読み込みたいオブジェクトにインスタンス化する
 * ②読み込みたいcsvをResourceフォルダに入れる
 * ③インスタンス化したオブジェクトから"csvファイル名"を呼び出す
 * ④※csvファイルはリスト型に格納されます→
 *                          呼び出しは二次元配列で呼び出してください
 */
public class CsvlInport
{

    //ファイルデータを格納する
    public List<string[]> csvDatas = new List<string[]>();
    //ファイル読み込み処理//ファイル名を記述
    public bool DateRead(string fileName)
    {
        // csvをロード
        TextAsset csv = Resources.Load(fileName) as TextAsset;
        StringReader reader = new StringReader(csv.text);
        while (reader.Peek() > -1)
        {
            // ','ごとに区切って配列へ格納
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }
        return true;
    }

}

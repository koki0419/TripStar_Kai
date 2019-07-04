using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ***インスタンス化したCsvImportの使い方***
 * ①インスタンス化
 * ②ファイル名を指定して「DateRead()」関数を呼び出す
 * ③二次元配列で呼び出せる
 */
public class GameDate : MonoBehaviour
{
    //データファイル名
    const string fileName = "starCSV";
    //csvデータ
    private CsvlInport excelInport = new CsvlInport();

    void Start()
    {
        excelInport.DateRead(fileName);

        for (int i = 0; i < excelInport.csvDatas.Count; i++)
        {
            for (int j = 0; j < excelInport.csvDatas[i].Length; j++)
            {
                Debug.Log("csvDatas[" + i + "][" + j + "] = " + excelInport.csvDatas[i][j]);
            }
        }
    }


    private void Update()
    {

    }
}

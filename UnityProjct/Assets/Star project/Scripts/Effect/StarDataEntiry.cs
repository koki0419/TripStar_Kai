using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarDataEntiry
{
    //星のID
    public int star_id;
    //生成ポジション
    public Vector3 star_Position;
    //星獲得ポイント
    public int star_Point;

    public void SetStarDatas(int id,float position_x,float position_y,float position_z,int point)
    {
        this.star_id = id;
        this.star_Position.x = position_x;
        this.star_Position.y = position_y;
        this.star_Position.z = position_z;
        this.star_Point = point;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//포톤 네트워크에서 시리얼라이즈로 xz좌표를 주고받는다.
public class PlayerCharactor : MonoBehaviour, IPunObservable
{
    public int x;
    public int z;

    /*
    public float Overtime_second { set; get; }
    public float Turntime_second { set; get; }
    public bool Overtime { set; get; }
    public int point { set; get; }
    */
    public float Overtime_second;
    public float Turntime_second;
    public bool Overtime;
    public int point;
    void setCoordinate((int,int)xz)
    {
        x = xz.Item1;
        z = xz.Item2;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(x);
            stream.SendNext(z);
            stream.SendNext(Overtime_second);
            stream.SendNext(Turntime_second);
            stream.SendNext(Overtime);
            stream.SendNext(point);
        }
        else
        {
            x = (int)stream.ReceiveNext();
            z = (int)stream.ReceiveNext();
            Overtime_second = (float)stream.ReceiveNext();
            Turntime_second = (float)stream.ReceiveNext();
            Overtime = (bool)stream.ReceiveNext();
            point = (int)stream.ReceiveNext();
        }
    }
}

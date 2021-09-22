using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class GameManager : GamePhaseStatus
{
    private GameObject PlanecubeMapManagerObj;
    private PlaneCubeMapManager PlanecubeMapManagerScript;
    private GameObject UIManagerObj;
    private GameObject MainCameraObj;

    private Queue<(int, int)> moveQueue;
    private Queue<(int, int, Direction)> installObstacleQueue;
    Queue<(int, int, Direction)> destroyObstacleQueue;

    private PhotonView photonView;
    public bool GameStarted = false;
    public int StartZ { set; get; }
    public int VictoryZ { set; get; }
    public int ObstacleAmount { set; get; }

    public GameObject PlayerCharactorObj { set; get; }
    public PlayerCharactor PlayerCharactorScript { set; get; }
    public GameObject OtherPlayerCharactorObj { set; get; }
    public PlayerCharactor OtherPlayerCharactorScript { set; get; }

    private int waitpaypoint = 0;

    public void addPointOne()
    {
        ObstacleAmount += 1;
    }

    void Start()
    {
        PlanecubeMapManagerObj = GameObject.Find("PlaneCubeMapManager");
        PlanecubeMapManagerScript = PlanecubeMapManagerObj.GetComponent<PlaneCubeMapManager>();
        UIManagerObj = GameObject.Find("UIManager");
        MainCameraObj = GameObject.Find("Main Camera");
        photonView = PhotonView.Get(this);
        moveQueue = new Queue<(int, int)>();
        installObstacleQueue = new Queue<(int, int,Direction)>();
        destroyObstacleQueue = new Queue<(int, int, Direction)>();
        VictoryZ = -1;
        //시간설정
        Overtime_second = Constants.DEFUALT_OVERTIME_SECOND;
        Turntime_second = Constants.DEFUALT_TURNTIME_SECOND;
        Overtime = false;
        PlayerCharactorScript = null;
        //처리 코루틴 시작
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine("BroadcastRoomGameStartMessage");
        }
    }

    IEnumerator BroadcastRoomGameStartMessage()
    {
        while(!GameStarted)
        {
            if ((PhotonNetwork.CurrentRoom.PlayerCount >= 2) && (!GameStarted))
            {
                startMasterClient();
                photonView.RPC("startNoMasterClient", RpcTarget.Others);
            }
            yield return null;
        }
    }

    //300초를 모두 사용하면 행동을 10초안에 결정해야한다.
    public float Overtime_second { set; get; }
    public float Turntime_second { set; get; }
    public bool Overtime { set; get; }
    //시간이 초과되면 강제로 넘긴다.

    void FixedUpdate()
    {
        //시간제한
        if (GamePhase.CommandPhase == GetGamePhase())
        {
            if (Overtime == false)
            {
                //Debug.Log(Time.deltaTime);
                Turntime_second -= Time.deltaTime;
                if (Turntime_second <= 0)
                {
                    Overtime = true;

                }
            }
            else
            {
                Overtime_second -= Time.deltaTime;
                if (Overtime_second <= 0)
                {
                    //강제진행
                    NextPhase();
                }
            }
        }

        if (PlayerCharactorScript != null)
        {
            PlayerCharactorScript.Overtime = Overtime;
            PlayerCharactorScript.Overtime_second = Overtime_second;
            PlayerCharactorScript.Turntime_second = Turntime_second;
            PlayerCharactorScript.point = ObstacleAmount;
        }
    }


    void Update()
    {
        if (GamePhase.SyncPhase == GetGamePhase())
        {
            photonView.RPC("OtherPlayerSyncReady", RpcTarget.Others);
            if (OtherPlayerSync)
            {
                NextPhase();
            }
        }
    }
    
    public override void StartPhase()
    {
        base.StartPhase();
        PlanecubeMapManagerObj.SendMessage("StartPhase");
        UIManagerObj.SendMessage("StartPhase");
        GameStarted = true;
        ObstacleAmount = Constants.DEFAULTPOINT;
    }
    public override void NextPhase()
    {
        //Debug.Log(GetGamePhase().ToString());
        base.NextPhase();
        PlanecubeMapManagerObj.SendMessage("NextPhase");
        UIManagerObj.SendMessage("NextPhase");

        //페이즈의 종료시 수행하는 작업
        switch (GetGamePhase())
        {
            case GamePhase.CommandPhase:
                UIManagerObj.SendMessage("SelectMoveView");
                //초읽기 초기화
                Overtime_second = Constants.DEFUALT_OVERTIME_SECOND;
                break;
            case GamePhase.SyncPhase:
                //가비지 컬렉션
                OtherPlayerSync = false;
                break;
            case GamePhase.EnforcementPhase:
                //삭제하는 장애물
                while (destroyObstacleQueue.Count > 0)
                {
                    PlanecubeMapManagerObj.SendMessage("DestroyObstacle", destroyObstacleQueue.Dequeue());
                }
                while (installObstacleQueue.Count>0)
                {
                    PlanecubeMapManagerObj.SendMessage("InstallObstacle", installObstacleQueue.Dequeue());
                }
                while(moveQueue.Count>0)
                {
                    PlanecubeMapManagerObj.SendMessage("MovePlayer", moveQueue.Dequeue());
                }
                installObstacleQueue.Clear();
                moveQueue.Clear();

                //조건확인
                Invoke("checkWinLoseDraw",2f);

                ObstacleAmount -= waitpaypoint;
                waitpaypoint = 0;
                NextPhase();
                break;
            default:
                break;
        }
    }

    public void checkWinLoseDraw()
    {
        if((VictoryZ== PlanecubeMapManagerScript.GetPlayerCoordinate().Item2)&
            (StartZ==OtherPlayerCharactorScript.z))
        {
            gotoCheckPhase();
            Invoke("gotoDrawPhase", 5f);
        }
        else if (VictoryZ == PlanecubeMapManagerScript.GetPlayerCoordinate().Item2)
        {
            photonView.RPC("gotoLosePhase", RpcTarget.Others);
            gotoWinPhase();
        }
    }

    public override void gotoCheckPhase()
    {
        base.gotoCheckPhase();
        UIManagerObj.SendMessage("gotoCheckPhase");
    }

    public override void gotoDrawPhase()
    {
        base.gotoDrawPhase();
        UIManagerObj.SendMessage("gotoDrawPhase");
        Invoke("OnLeftRoom", 5f);
    }

    public override void gotoWinPhase()
    {
        base.gotoWinPhase();
        UIManagerObj.SendMessage("gotoWinPhase");        
        Invoke("OnLeftRoom", 5f);
    }

    [PunRPC]
    public override void gotoLosePhase()
    {
        base.gotoLosePhase();
        UIManagerObj.SendMessage("gotoLosePhase");
        Invoke("OnLeftRoom", 5f);
    }

    public void OnLeftRoom()
    {
        PhotonNetwork.LeaveRoom();
        Invoke("gotoTitle", 5f);
    }

    public void gotoTitle()
    {
        SceneManager.LoadScene("GameTitle");
    }


    [PunRPC]
    public void insertCommandObstatcle(int x, int z, Direction direction)
    {
        installObstacleQueue.Enqueue((x, z, direction));
    }

    public void CancelCommandObstacle((int, int, Direction) obstacleData)
    {
        Debug.Log("Dummy Method");
        //사용하지 않는 메소드
    }
    

    public void commandObstacle((int, int) xz)
    {
        if (ObstacleAmount < 1)
        {
            UIManagerObj.SendMessage("SelectMoveView");
            UIManagerObj.SendMessage("showPlayerLog","포인트가 부족합니다.");
            return;
        }
        photonView.RPC("insertCommandObstatcle", RpcTarget.All, xz.Item1, xz.Item2, obstacleDirection);
        //ObstacleAmount -= 1;
        waitpaypoint = 1;
        NextPhase();
    }

    [PunRPC]
    public void insertCommandDestoryObstatcle(int x, int z, Direction direction)
    {
        destroyObstacleQueue.Enqueue((x, z, direction));
    }

    public void commandDestroyObstacleAndGo((int, int) xz)
    {
        if (ObstacleAmount < 2)
        {
            UIManagerObj.SendMessage("SelectMoveView");
            UIManagerObj.SendMessage("showPlayerLog", "포인트가 부족합니다.");
            return;
        }

        (int, int) PlayerCoordinate = PlanecubeMapManagerScript.GetPlayerCoordinate();
        (int, int) PlayerGoCoordinate = xz;
        int xValue = PlayerCoordinate.Item1 - PlayerGoCoordinate.Item1;
        int zValue = PlayerCoordinate.Item2 - PlayerGoCoordinate.Item2;

        //큐브의 입장에서는 반대이다.
        Direction direction=Direction.Top;
        if(xValue>0)
        {
            direction = Direction.Right;
        }
        else if(xValue<0)
        {
            direction = Direction.Left;
        }
        else if(zValue>0)
        {
            direction = Direction.Top;
        }
        else if(zValue<0)
        {
            direction = Direction.Bottom;
        }

        photonView.RPC("insertCommandDestoryObstatcle", RpcTarget.All, xz.Item1, xz.Item2, direction);
        moveQueue.Enqueue(xz);
        //ObstacleAmount -= 2;
        waitpaypoint = 2;
        NextPhase();
    }

    public void commandNextTurn()
    {
        NextPhase();
    }

    public void commandMove((int, int) xz)
    {
        moveQueue.Enqueue(xz);
        NextPhase();
    }

    bool OtherPlayerSync = false;    
    [PunRPC]
    public void OtherPlayerSyncReady()
    {
        OtherPlayerSync = true;
    }

    [PunRPC]
    public void startNoMasterClient()
    {
        StartPhase();
        int startX = Random.Range(0, 7+1);
           
        PlanecubeMapManagerObj.SendMessage("PutPlayerHourse", (startX, 7));
        VictoryZ = 0;
        StartZ = 7;
    }

    public void startMasterClient()
    {
        StartPhase();
        int startX = Random.Range(0, 7 + 1);
        PlanecubeMapManagerObj.SendMessage("PutPlayerHourse", (startX, 0));
        VictoryZ = 7;
        StartZ = 0;
    }

    //UI매니저에서 버튼이 클릭되었을때 처리하는 메소드들
    //단 장애물을 모두 소모한 상태에서는 명령을 취소시킨다.
    public override void SelectMoveView()
    {
        base.SelectMoveView();
        PlanecubeMapManagerObj.SendMessage("SelectMoveView");
        MainCameraObj.SendMessage("SelectMoveView");
    }

    public void ViewRefresh()
    {
        SelectMoveView();
    }

    public override void SelectObstacleView()
    {
        //명령 취소
        //사용자가 무브버튼을 눌른것과 동일하게 처리한다.
        if(ObstacleAmount<=0)
        {
            UIManagerObj.SendMessage("SelectMoveView");
            UIManagerObj.SendMessage("showPlayerLog", "포인트가 부족합니다.");
            return;
        }
        base.SelectObstacleView();
        PlanecubeMapManagerObj.SendMessage("SelectObstacleView");
        MainCameraObj.SendMessage("SelectObstacleView");
    }

    Direction obstacleDirection;
    public void SetObstacleDirection(Direction obstacleDirection)
    {
        //

        this.obstacleDirection = obstacleDirection;
    }
    public Direction GetObstacleDirecton()
    {
        return obstacleDirection;
    }
}

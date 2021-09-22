using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Threading;
using System.Threading.Tasks;

public class PlaneCubeMapManager : GamePhaseStatus
{
    private GameObject[,] PlaneCubeMatrix;
    private PlaneCube[,] PlaneCubeScriptMatrix;
    public Transform PlaneMapStartPos;
    public int xLength;
    public int zLength;

    public GameObject CameraObj;
    public GameObject PlaneCubePrefab;
    private GameObject ZeroPlaneCube;

    private GameObject PlayerCharactorObj=null;
    private PlayerCharactor PlayerCharactorScript = null;
    private GameObject OtherPlayerCharactorObj = null;
    private PlayerCharactor OtherPlayerCharactorScript = null;

    GameObject GameManagerObj;
    GameManager GameManagerScript;
    int MaxVertexNumber;

    List<PlaneCube> PlaneCubeScriptList = new List<PlaneCube>();
    PhotonView photonView;
    
    void Start()
    {
        makeMap();
        GameManagerObj = GameObject.Find("GameManager");
        GameManagerScript = GameManagerObj.GetComponent<GameManager>();

        //PlaneCubeScriptMatrix 1차원 배열화
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                PlaneCubeScriptList.Add(PlaneCubeScriptMatrix[i, j]);
            }
        }
        photonView = PhotonView.Get(this);
        
    }

    void setMapRandomPoint()
    {
        foreach(var cube in PlaneCubeScriptList)
        {
            setPlaneCubeRandomPoint(cube.VertexNumber);
        }
    }

    void setPlaneCubeRandomPoint(int vertexNumber)
    {
        int point = Random.Range(0, 100 + 1);
        if (point <= Constants.ADDITIONALPOINT_PERCENTAGE)
        {
            //15%의 확률로 생성
            point = 1;
        }
        else
        {
            point = 0;
        }
        photonView.RPC("setLocalPlaneCubePoint",RpcTarget.All,vertexNumber,point);
    }

    void setPlaneCubePoint(int vertexNumber,int point)
    {
        photonView.RPC("setLocalPlaneCubePoint", RpcTarget.All, vertexNumber, point);
    }

    [PunRPC]
    void setLocalPlaneCubePoint(int vertexNumber, int point)
    {
        SearchPlaneCubeScriptByVertexNumber(vertexNumber).AdditionalPoint=point;
        MapRefresh();
    }

    void Update()
    {
        if (OtherPlayerCharactorObj == null)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                OtherPlayerCharactorObj = GameObject.FindGameObjectWithTag("TagPlayer2");
                if (OtherPlayerCharactorObj != null)
                {
                    OtherPlayerCharactorScript = OtherPlayerCharactorObj.GetComponent<PlayerCharactor>();
                    GameManagerScript.OtherPlayerCharactorObj = OtherPlayerCharactorObj;
                    GameManagerScript.OtherPlayerCharactorScript = OtherPlayerCharactorScript;
                }
            }
            else
            {
                OtherPlayerCharactorObj = GameObject.FindGameObjectWithTag("TagPlayer1");
                if (OtherPlayerCharactorObj != null)
                {
                    OtherPlayerCharactorScript = OtherPlayerCharactorObj.GetComponent<PlayerCharactor>();
                    GameManagerScript.OtherPlayerCharactorObj = OtherPlayerCharactorObj;
                    GameManagerScript.OtherPlayerCharactorScript = OtherPlayerCharactorScript;
                }
            }
        }
    }

    //멀티쓰레딩으로 구현
    public override void SelectMoveView()
    {
        base.SelectMoveView();
        Parallel.ForEach(PlaneCubeScriptList, cubescript =>
        {
            cubescript.SelectMoveViewCalculateMaterial_threadsafe();
        }
        );
        foreach (var planeCubeScript in PlaneCubeScriptList)
        {
            planeCubeScript.AdjustMaterial();
        }
    }

    public override void SelectObstacleView()
    {
        base.SelectObstacleView();
        Parallel.ForEach(PlaneCubeScriptList,cubescript=> 
            {
                cubescript.SelectObstacleViewCalculateMaterial_threadsafe();
            }
        );
        foreach(var planeCubeScript in PlaneCubeScriptList)
        {
            planeCubeScript.AdjustMaterial();
        }
    }

    public override void StartPhase()
    {
        base.StartPhase();
        CameraObj.SendMessage("StartPhase");
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                PlaneCubeMatrix[i, j].SendMessage("StartPhase");
            }
        }
        //마스터 서버인 경우에 설정하는 것
        if (PhotonNetwork.IsMasterClient)
        {
            //랜덤포인트
            setMapRandomPoint();
        }
    }

    public override void NextPhase()
    {
        base.NextPhase();
        CameraObj.SendMessage("NextPhase");
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                PlaneCubeMatrix[i, j].SendMessage("NextPhase");
            }
        }
    }
    
    void makeMap()
    {
        PlaneCubeMatrix = new GameObject[xLength, zLength];
        
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                Vector3 objPos = new Vector3(PlaneMapStartPos.position.x + i, PlaneMapStartPos.position.y, PlaneMapStartPos.position.z + j);
                PlaneCubeMatrix[i, j] = Instantiate(PlaneCubePrefab, objPos, PlaneMapStartPos.rotation);
            }
        }

        PlaneCubeScriptMatrix = new PlaneCube[xLength, zLength];
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                PlaneCubeScriptMatrix[i, j] = PlaneCubeMatrix[i, j].GetComponent<PlaneCube>();
            }
        }

        int VertexNumber = 0;
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                PlaneCubeScriptMatrix[i, j].VertexNumber = VertexNumber;
                VertexNumber++;
            }
        }
        MaxVertexNumber = VertexNumber - 1;

        ZeroPlaneCube = Instantiate(PlaneCubePrefab,new Vector3(0,0,0),PlaneMapStartPos.rotation);
        ZeroPlaneCube.GetComponent<Renderer>().enabled = false;

        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                PlaneCube csPlaneCube = PlaneCubeMatrix[i, j].GetComponent<PlaneCube>();
                GameObject LeftPlaneCube = ZeroPlaneCube;
                GameObject RightPlaneCube = ZeroPlaneCube;
                GameObject TopPlaneCube = ZeroPlaneCube;
                GameObject BottomPlaneCube = ZeroPlaneCube;

                if (i - 1 >= 0)
                {
                    LeftPlaneCube = PlaneCubeMatrix[i - 1, j];
                }


                if (i + 1 < xLength)
                {
                    RightPlaneCube = PlaneCubeMatrix[i + 1, j];
                }

                
                if (j - 1 >= 0)
                {
                    BottomPlaneCube = PlaneCubeMatrix[i, j - 1];
                }

                if (j + 1 < zLength)
                {
                    TopPlaneCube = PlaneCubeMatrix[i, j + 1];
                }

                csPlaneCube.setAroundPlaneCubes(LeftPlaneCube, RightPlaneCube, TopPlaneCube, BottomPlaneCube);
            }
        }
    }
    void MapRefresh()
    {
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                PlaneCubeScriptMatrix[i, j].RefreshView();
            }
        }
    }

    
    void PutPlayerHourse((int, int) xz)
    {
        GameObject cube = PlaneCubeMatrix[xz.Item1, xz.Item2];
        Vector3 vector3 = new Vector3(cube.transform.position.x,
                                      cube.transform.position.y,
                                      cube.transform.position.z);
        vector3.y += (float)0.5;

        //master 
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerCharactorObj = PhotonNetwork.Instantiate("Charactor1Prefab",
                                                            vector3,
                                                            cube.transform.rotation);
            PlayerCharactorScript = PlayerCharactorObj.GetComponent<PlayerCharactor>();
        }
        else
        {
            PlayerCharactorObj = PhotonNetwork.Instantiate("Charactor2Prefab",
                                                            vector3,
                                                            cube.transform.rotation);
            PlayerCharactorScript = PlayerCharactorObj.GetComponent<PlayerCharactor>();
        }
        SetPlayerCoordinate(xz);

        GameManagerScript.PlayerCharactorObj = PlayerCharactorObj;
        GameManagerScript.PlayerCharactorScript = PlayerCharactorScript;
    }
    //setPlayerHere은 1개로 한다.
    void SetPlayerCoordinate((int,int)xz)
    {
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                PlaneCubeScriptMatrix[i, j].PlayerHere = false;
            }
        }
        PlaneCubeScriptMatrix[xz.Item1, xz.Item2].PlayerHere = true;
        //플레이어 말의 이동
        GameObject cube = PlaneCubeMatrix[xz.Item1, xz.Item2];
        Vector3 vector3 = new Vector3(cube.transform.position.x,
                                      cube.transform.position.y,
                                      cube.transform.position.z);
        vector3.y += (float)0.5;
        PlayerCharactorObj.transform.position = vector3;
        MapRefresh();
    }
    public void MovePlayer((int,int)xz)
    {
        PlaneCube cubeScript = PlaneCubeScriptMatrix[xz.Item1, xz.Item2];
        if (cubeScript.checkCanGo())
        {
            SetPlayerCoordinate(xz);
            PlayerCharactorObj.SendMessage("setCoordinate", xz);
            //이동후의 처리
            if(cubeScript.checkAdditionalPoint())
            {
                //추가포인트 있음
                setPlaneCubePoint(cubeScript.VertexNumber, 0);
                GameManagerObj.SendMessage("addPointOne");
            }
            MapRefresh();
        }
    }

    public (int,int) GetCubeCoordinate(GameObject cubeObj)
    {
        int x = -1;
        int z = -1;
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                if (PlaneCubeMatrix[i, j] == cubeObj)
                {
                    x = i;
                    z = j;
                }
            }
        }
        return (x, z);
    }

    public void InstallObstacle((int, int, Direction) obstacleData)
    {
        //코스트를 확인해서 아예 막혀버리면 취소시킨다.
        int x = obstacleData.Item1;
        int z = obstacleData.Item2;
        Direction direction = obstacleData.Item3;

        int obstacleStartVertex = PlaneCubeScriptMatrix[x, z].VertexNumber;
        int obstacleEndVertex=-1;
        switch(direction)
        {
            case Direction.Right:
                obstacleEndVertex = PlaneCubeScriptMatrix[x + 1, z].VertexNumber;
                break;
            case Direction.Left:
                obstacleEndVertex = PlaneCubeScriptMatrix[x-1, z].VertexNumber;
                break;
            case Direction.Top:
                obstacleEndVertex = PlaneCubeScriptMatrix[x, z+1].VertexNumber;
                break;
            case Direction.Bottom:
                obstacleEndVertex = PlaneCubeScriptMatrix[x, z-1].VertexNumber;
                break;
        }
        
        if (!CheckCanPlayAfterInstallObstacle(obstacleStartVertex,obstacleEndVertex))
        {
            return;
        }
        else
        {
            PlaneCubeMatrix[x, z].SendMessage("InstallObstacle", direction);
        }
    }

    public void DestroyObstacle((int, int, Direction) obstacleData)
    {
        int x = obstacleData.Item1;
        int z = obstacleData.Item2;
        Direction direction = obstacleData.Item3;
        PlaneCubeMatrix[x, z].SendMessage("DestoryObstacle", direction);
    }

    public (int,int) GetPlayerCoordinate()
    {
        int x = -1;
        int z = -1;
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                if (PlaneCubeScriptMatrix[i, j].PlayerHere==true)
                {
                    x = i;
                    z = j;
                }
            }
        }
        return (x, z);
    }

    //vertex=num인 찾기 O(n^2)
    PlaneCube SearchPlaneCubeScriptByVertexNumber(int VertexNumber)
    {
        PlaneCube planeCube=null;
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                if (PlaneCubeScriptMatrix[i, j].VertexNumber== VertexNumber)
                {
                    planeCube = PlaneCubeScriptMatrix[i, j];
                }
            }
        }
        return planeCube;
    }

    public (int, int) SearchCoordinateByVertexNumber(int VertexNumber)
    {
        int x = -1;
        int z = -1;
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                if (PlaneCubeScriptMatrix[i, j].VertexNumber == VertexNumber)
                {
                    x = i;
                    z = j;
                }
            }
        }
        return (x,z);
    }

    //costgraph O(n^3)
    int[,] GetOneRoadCostGraph()
    {
        //계산되지 않은 Graph
        //56,57,58,59,60,61,62,63
        //......
        //......
        //00,01,02,03,04,05,06,07
        int VertexLength = MaxVertexNumber + 1;
        int[,] CostGraph = new int[VertexLength * VertexLength, VertexLength * VertexLength];
        for (int i = 0; i < VertexLength; i++)
        {
            for (int j = 0; j < VertexLength; j++)
            {
                //i->j로 가는 비용
                PlaneCube startplaneCube = SearchPlaneCubeScriptByVertexNumber(i);
                CostGraph[i, j] = startplaneCube.VertexOneRoadCost(j);
            }
        }
        return CostGraph;
    }

    int[,] CalculateCostGraph(int[,] CostGraph)
    {
        int VertexLength = MaxVertexNumber + 1;
        for (int k = 0; k < VertexLength; k++)
        {
            for (int i = 0; i < VertexLength; i++)
            {
                for (int j = 0; j < VertexLength; j++)
                {
                    if (CostGraph[i, j] > CostGraph[i, k] + CostGraph[k, j])
                    {
                        CostGraph[i, j] = CostGraph[i, k] + CostGraph[k, j];
                    }
                }
            }
        }
        return CostGraph;
    }


    int[,] GetCurrentCostGraph()
    {
        return CalculateCostGraph(GetOneRoadCostGraph());
    }

    //O(n^3)
    int GetCurrentCostVertexToVertex(int vertex1,int vertex2)
    {
        int[,] CostGraph = GetCurrentCostGraph();
        return CostGraph[vertex1, vertex2];
    }

    //장애물을 설치한 후에 비용 체크
    //정점A-정점B에 장애물 설치
    int[,] GetAfterInstallObstacleCostGraph(int obstacleStartVertex,int obstacleEndVertex)
    {
        int[,] OneRoadCostGraph = GetOneRoadCostGraph();
        OneRoadCostGraph[obstacleStartVertex, obstacleEndVertex] = Constants.BLOCKCOST;
        OneRoadCostGraph[obstacleEndVertex, obstacleStartVertex] = Constants.BLOCKCOST;
        return CalculateCostGraph(OneRoadCostGraph);
    }
    int GetAfterInstallObstacleCostVertexToVertex(int vertex1,int vertex2,
                                                  int obstacleStartVertex, int obstacleEndVertex)
    {
        int[,] CostGraph = GetAfterInstallObstacleCostGraph(obstacleStartVertex, obstacleEndVertex);
        return CostGraph[vertex1, vertex2];
    }

    //장애물의 설치가 완료된 후에 진행유무를 확인
    //코스트를 기반으로 1. 기본 2. 플레이어의 말 3. 상대 플레이어의 말을 탐색해야 한다.
    public bool CheckCanPlayAfterInstallObstacle(int obstacleStartVertex, int obstacleEndVertex)
    {
        int[,] costGraph = GetAfterInstallObstacleCostGraph(obstacleStartVertex,
                                                            obstacleEndVertex);
        //플레이어의 승리위치의 반대가 상대의 승리위치
        (int, int) PlayerVictoryCoordinate = (0,GameManagerScript.VictoryZ);
        (int, int) OtherPlayerVictoryCoordinate = (0,GameManagerScript.StartZ);

        int PlayerVictoryVertexNumber = PlaneCubeScriptMatrix[PlayerVictoryCoordinate.Item1, PlayerVictoryCoordinate.Item2].VertexNumber;
        int OtherPlayerVictoryVertexNumber = PlaneCubeScriptMatrix[OtherPlayerVictoryCoordinate.Item1, OtherPlayerVictoryCoordinate.Item2].VertexNumber;

        //현재위치 가져오기
        (int, int) PlayerCoordinate = (PlayerCharactorScript.x, PlayerCharactorScript.z);
        (int, int) OtherPlayerCoordinate = (OtherPlayerCharactorScript.x, OtherPlayerCharactorScript.z);

        int PlayerVertexNumber = PlaneCubeScriptMatrix[PlayerCoordinate.Item1,PlayerCoordinate.Item2].VertexNumber;
        int OtherPlayerVertexNumber= PlaneCubeScriptMatrix[OtherPlayerCoordinate.Item1, OtherPlayerCoordinate.Item2].VertexNumber;

        int PlayerCost = costGraph[PlayerVertexNumber, PlayerVictoryVertexNumber];
        int OtherPlayerCost = costGraph[OtherPlayerVertexNumber, OtherPlayerVictoryVertexNumber];

        if ((PlayerCost < Constants.BLOCKCOST)&&(OtherPlayerCost<Constants.BLOCKCOST))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

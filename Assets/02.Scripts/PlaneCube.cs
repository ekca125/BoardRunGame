using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Left,
    Right,
    Top,
    Bottom
}

public class PlaneCube : PlaneCubeStatus
{
    public int AdditionalPoint { set; get; }

    public GameObject ObstaclePrefab;

    public GameObject LeftObstacle { set; get; }
    public GameObject RightObstacle { set; get; }
    public GameObject TopObstacle { set; get; }
    public GameObject BottomObstacle { set; get; }
    

    GameManager GameManagerScript;
    PlaneCubeMapManager PlaneCubeMapManagerScript;

    private PlaneCube LeftPlaneCubeScript;
    private PlaneCube RightPlaneCubeScript;
    private PlaneCube TopPlaneCubeScript;
    private PlaneCube BottomPlaneCubeScript;

    private GameObject LeftPlaneCube;
    private GameObject RightPlaneCube;
    private GameObject TopPlaneCube;
    private GameObject BottomPlaneCube;

    void Start()
    {
        GameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        PlaneCubeMapManagerScript = GameObject.Find("PlaneCubeMapManager").GetComponent<PlaneCubeMapManager>();
    }
    public int VertexNumber { set; get; }
    public int VertexOneRoadCost(int GoalVertexNumber)
    {
        //1회로 가는 것이 가능하면 비용 1
        //가는것이 불가능하면 비용 99999
        //자신에서 이 정점까지 가는 거리를 찾는 것
        //주위 큐브 맞음 + 장애물 없음
        if(LeftPlaneCubeScript.VertexNumber==GoalVertexNumber)
        {
            if (LeftObstacle != null)
                return Constants.BLOCKCOST;
            else
                return Constants.BASICCOST;
        }
        else if(RightPlaneCubeScript.VertexNumber==GoalVertexNumber)
        {
            if (RightObstacle != null)
                return Constants.BLOCKCOST;
            else
                return Constants.BASICCOST;
        }
        else if(TopPlaneCubeScript.VertexNumber==GoalVertexNumber)
        {
            if (TopObstacle != null)
                return Constants.BLOCKCOST;
            else
                return Constants.BASICCOST;
        }
        else if(BottomPlaneCubeScript.VertexNumber==GoalVertexNumber)
        {
            if (BottomObstacle != null)
                return Constants.BLOCKCOST;
            else
                return Constants.BASICCOST;
        }
        else
        {
            return Constants.BLOCKCOST;
        }
    }

    public override bool checkAdditionalPoint()
    {
        if(AdditionalPoint==0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public override bool checkCanDeleteGoObstalce()
    {
        //장애물이 없어지면 가는 것이 가능한 경우
        if (PlayerHere)
            return false;
        if (LeftPlaneCubeScript.PlayerHere == true)
        {
            if (LeftObstacle != null)
                return true;
            else
                return false;
        }
        if (RightPlaneCubeScript.PlayerHere == true)
        {
            if (RightObstacle != null)
                return true;
            else
                return false;
        }
        if (TopPlaneCubeScript.PlayerHere == true)
        {
            if (TopObstacle != null)
                return true;
            else
                return false;
        }
        if (BottomPlaneCubeScript.PlayerHere == true)
        {
            if (BottomObstacle != null)
                return true;
            else
                return false;
        }
        return false;
    }

    Vector3 m_vector; 
    public void InstallObstacle(Direction direction)
    {
        if (!checkCanInstallObstacle())
            return;

        m_vector = new Vector3(transform.position.x,
                               transform.position.y,
                               transform.position.z);
        m_vector.y = m_vector.y + (float)0.5;

        switch (direction)
        {
            case Direction.Right:
                if (RightObstacle != null)
                    return;
                m_vector.x = m_vector.x + (float)0.5;
                m_vector.z = m_vector.z + (float)0;
                RightObstacle = Instantiate(ObstaclePrefab, m_vector, transform.rotation);
                RightObstacle.transform.Rotate(Vector3.up * 90);
                RightPlaneCubeScript.InstallObstacleGameObject(direction, RightObstacle);
                break;
            case Direction.Left:
                if (LeftObstacle != null)
                    return;
                m_vector.x = m_vector.x - (float)0.5;
                m_vector.z = m_vector.z + (float)0;
                LeftObstacle = Instantiate(ObstaclePrefab, m_vector, transform.rotation);
                LeftObstacle.transform.Rotate(Vector3.up * 90);
                LeftPlaneCubeScript.InstallObstacleGameObject(direction, LeftObstacle);
                break;
            case Direction.Bottom:
                if (BottomObstacle != null)
                    return;
                m_vector.x = m_vector.x + (float)0;
                m_vector.z = m_vector.z - (float)0.5;
                BottomObstacle = Instantiate(ObstaclePrefab, m_vector, transform.rotation);
                BottomObstacle.transform.Rotate(Vector3.up * 0);
                BottomPlaneCubeScript.InstallObstacleGameObject(direction, BottomObstacle);
                break;
            case Direction.Top:
                if (TopObstacle != null)
                    return;
                m_vector.x = m_vector.x + (float)0;
                m_vector.z = m_vector.z + (float)0.5;
                TopObstacle = Instantiate(ObstaclePrefab, m_vector, transform.rotation);
                TopObstacle.transform.Rotate(Vector3.up * 0);
                TopPlaneCubeScript.InstallObstacleGameObject(direction, TopObstacle);
                break;
            default:
                break;
        }
    }
    void InstallObstacleGameObject(Direction OtherCubeDirection ,GameObject obstacle)
    {
        switch (OtherCubeDirection)
        {
            case Direction.Right:
                LeftObstacle = obstacle;
                break;
            case Direction.Left:
                RightObstacle = obstacle;
                break;
            case Direction.Bottom:
                TopObstacle = obstacle;
                break;
            case Direction.Top:
                BottomObstacle = obstacle;
                break;
            default:
                break;
        }
    }

    public void DestoryObstacle(Direction directon)
    {
        switch (directon)
        {
            case Direction.Right:
                if (RightObstacle != null)
                {
                    RightObstacle.transform.position = new Vector3(Constants.DelayDestoryPostionX,
                                                                    Constants.DelayDestoryPostionY,
                                                                    Constants.DelayDestoryPostionZ);
                    Destroy(RightObstacle);
                    RightObstacle = null;
                    RightPlaneCubeScript.DestoryObstacleGameObject(Direction.Right);
                }
                break;
            case Direction.Left:
                if (LeftObstacle != null)
                {
                    LeftObstacle.transform.position = new Vector3(Constants.DelayDestoryPostionX,
                                                                    Constants.DelayDestoryPostionY,
                                                                    Constants.DelayDestoryPostionZ);
                    Destroy(LeftObstacle);
                    LeftObstacle = null;
                    LeftPlaneCubeScript.DestoryObstacleGameObject(Direction.Left);
                }
                break;
            case Direction.Bottom:
                if (BottomObstacle != null)
                {
                    BottomObstacle.transform.position = new Vector3(Constants.DelayDestoryPostionX,
                                                                    Constants.DelayDestoryPostionY,
                                                                    Constants.DelayDestoryPostionZ);
                    Destroy(BottomObstacle);
                    BottomObstacle = null;
                    BottomPlaneCubeScript.DestoryObstacleGameObject(Direction.Bottom);
                }
                break;
            case Direction.Top:
                if (TopObstacle != null)
                {
                    TopObstacle.transform.position = new Vector3(Constants.DelayDestoryPostionX,
                                                                    Constants.DelayDestoryPostionY,
                                                                    Constants.DelayDestoryPostionZ);
                    Destroy(TopObstacle);
                    TopObstacle = null;
                    TopPlaneCubeScript.DestoryObstacleGameObject(Direction.Top);
                }
                break;
            default:
                break;
        }
    }
    void DestoryObstacleGameObject(Direction OtherCubeDirection)
    {
        switch (OtherCubeDirection)
        {
            case Direction.Right:
                LeftObstacle = null;
                break;
            case Direction.Left:
                RightObstacle = null;
                break;
            case Direction.Bottom:
                TopObstacle = null;
                break;
            case Direction.Top:
                BottomObstacle = null;
                break;
            default:
                break;
        }
    }


    public override void StartPhase()
    {
        base.StartPhase();
        RefreshView();
    }

    public override void NextPhase()
    {
        base.NextPhase();
        RefreshView();
    }

    public override void SelectMoveView()
    {
        base.SelectMoveView();
        RefreshView();
    }
    public override void SelectObstacleView()
    {
        base.SelectObstacleView();
        RefreshView();
    }

    //멀티쓰레딩 함수
    public void SelectObstacleViewCalculateMaterial_threadsafe()
    {
        base.SelectObstacleView();
        CalculateAdjustMaterial();
    }

    public void SelectMoveViewCalculateMaterial_threadsafe()
    {
        base.SelectMoveView();
        CalculateAdjustMaterial();
    }



    public bool PlayerHere { get; set; }
    public PlaneCube()
    {
        PlayerHere = false;
        //장애물
        LeftObstacle = null;
        RightObstacle = null;
        TopObstacle = null;
        BottomObstacle = null;
        //주위큐브
        LeftPlaneCubeScript = null;
        RightPlaneCubeScript = null;
        TopPlaneCubeScript = null;
        BottomPlaneCubeScript = null;
        //주위큐브
        LeftPlaneCube = null;
        RightPlaneCube = null;
        TopPlaneCube = null;
        BottomPlaneCube = null;

        VertexNumber = -1;
        AdditionalPoint = 0;
    }
    public void setAroundPlaneCubes(GameObject LeftPlaneCube,
                                    GameObject RightPlaneCube,
                                    GameObject TopPlaneCube,
                                    GameObject BottomPlaneCube)
    {
        this.LeftPlaneCube = LeftPlaneCube;
        this.RightPlaneCube = RightPlaneCube;
        this.TopPlaneCube = TopPlaneCube;
        this.BottomPlaneCube = BottomPlaneCube;


        this.LeftPlaneCubeScript = LeftPlaneCube.GetComponent<PlaneCube>();
        this.RightPlaneCubeScript = RightPlaneCube.GetComponent<PlaneCube>();
        this.TopPlaneCubeScript = TopPlaneCube.GetComponent<PlaneCube>();
        this.BottomPlaneCubeScript = BottomPlaneCube.GetComponent<PlaneCube>();
    }

    override public bool checkCanGo()
    {
        if (PlayerHere)
            return false;
        if(LeftPlaneCubeScript.PlayerHere==true)
        {
            if (LeftObstacle == null)
                return true;
            else
                return false;
        }
        if (RightPlaneCubeScript.PlayerHere == true)
        {
            if (RightObstacle == null)
                return true;
            else
                return false;
        }
        if (TopPlaneCubeScript.PlayerHere == true)
        {
            if (TopObstacle == null)
                return true;
            else
                return false;
        }
        if (BottomPlaneCubeScript.PlayerHere == true)
        {
            if (BottomObstacle == null)
                return true;
            else
                return false;
        }
        return false;
    }
    public override bool checkCanInstallObstacle()
    {
        //추가된 코드
        if (LeftObstacle != null)
            return true;
        else if (RightObstacle != null)
            return true;
        else if (TopObstacle != null)
            return true;
        else if (BottomObstacle != null)
            return true;


        int Victory1 = 0;
        int Victory2 = Constants.MapZLength - 1;

        (int, int) MyCoordinate = PlaneCubeMapManagerScript.SearchCoordinateByVertexNumber(
                                                    VertexNumber);
        //Debug.Log(MyCoordinate);
        //Debug.Log((Victory1, Victory2));
        if ((MyCoordinate.Item2 == Victory1) || (MyCoordinate.Item2 == Victory2))
        {

            return false;
        }

        //장애물이 설치되도 추가적으로 가능하도록
        //더 많은 비용을 소모하거나
        Direction directon = GameManagerScript.GetObstacleDirecton();

        if (Direction.Left == directon && LeftObstacle == null)
        {
            if (LeftPlaneCubeScript.VertexNumber == -1)
            {
                return true;
            }

            if (PlaneCubeMapManagerScript.CheckCanPlayAfterInstallObstacle(
                                            this.VertexNumber,
                                            LeftPlaneCubeScript.VertexNumber))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (Direction.Right == directon && RightObstacle == null)
        {
            if (RightPlaneCubeScript.VertexNumber == -1)
            {
                return true;
            }

            if (PlaneCubeMapManagerScript.CheckCanPlayAfterInstallObstacle(
                                            this.VertexNumber,
                                            RightPlaneCubeScript.VertexNumber))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (Direction.Top == directon && TopObstacle == null)
        {
            if (TopPlaneCubeScript.VertexNumber == -1)
            {
                return true;
            }

            if (PlaneCubeMapManagerScript.CheckCanPlayAfterInstallObstacle(
                                            this.VertexNumber,
                                            TopPlaneCubeScript.VertexNumber))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (Direction.Bottom == directon && BottomObstacle == null)
        {
            if (BottomPlaneCubeScript.VertexNumber == -1)
            {
                return true;
            }

            if (PlaneCubeMapManagerScript.CheckCanPlayAfterInstallObstacle(
                                            this.VertexNumber,
                                            BottomPlaneCubeScript.VertexNumber))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
            return false;
    }

    //장애물 방향에 따른 여부 판단.
    //코드 최적화 대신에 코어를 더 추가하는 멀티쓰레딩사용


    /*
    public override bool checkCanInstallObstacle()
    {
        int Victory1 = 0;
        int Victory2 = Constants.MapZLength - 1;

        (int,int) MyCoordinate = PlaneCubeMapManagerScript.SearchCoordinateByVertexNumber(
                                                    VertexNumber);
        //Debug.Log(MyCoordinate);
        //Debug.Log((Victory1, Victory2));
        if((MyCoordinate.Item2== Victory1) || (MyCoordinate.Item2== Victory2))
        {
            
            return false;
        }

        //장애물이 설치되도 추가적으로 가능하도록
        //더 많은 비용을 소모하거나
        Direction directon=GameManagerScript.GetObstacleDirecton();
        if (Direction.Left == directon && LeftObstacle == null)
        {
            if(LeftPlaneCubeScript.VertexNumber==-1)
            {
                return true;
            }

            if (PlaneCubeMapManagerScript.CheckCanPlayAfterInstallObstacle(
                                            this.VertexNumber,
                                            LeftPlaneCubeScript.VertexNumber))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (Direction.Right == directon && RightObstacle == null)
        {
            if (RightPlaneCubeScript.VertexNumber == -1)
            {
                return true;
            }

            if (PlaneCubeMapManagerScript.CheckCanPlayAfterInstallObstacle(
                                            this.VertexNumber,
                                            RightPlaneCubeScript.VertexNumber))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (Direction.Top == directon && TopObstacle == null)
        {
            if (TopPlaneCubeScript.VertexNumber == -1)
            {
                return true;
            }

            if (PlaneCubeMapManagerScript.CheckCanPlayAfterInstallObstacle(
                                            this.VertexNumber,
                                            TopPlaneCubeScript.VertexNumber))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (Direction.Bottom == directon && BottomObstacle == null)
        {
            if (BottomPlaneCubeScript.VertexNumber == -1)
            {
                return true;
            }

            if (PlaneCubeMapManagerScript.CheckCanPlayAfterInstallObstacle(
                                            this.VertexNumber,
                                            BottomPlaneCubeScript.VertexNumber))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
            return false;
    }
    */
}

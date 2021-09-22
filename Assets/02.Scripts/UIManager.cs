using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : GamePhaseStatus
{
    GameObject GameMangerObj;
    GameManager GameManagerScript;

    public GameObject PhaseTextObj;
    public GameObject PlayerPointTextObj;
    public GameObject OtherPlayerPointTextObj;
    public GameObject PlayerTimeTextObj;
    public GameObject OtherPlayerTimeTextObj;
    public GameObject PlayerLogObj;

    Text PhaseText;
    Text PlayerPointText;
    Text OtherPlayerPointText;
    Text PlayerTimeText;
    Text OtherPlayerTimeText;
    Text PlayerLogText;

    bool firstTurn;
    int PlayerPrevTurnPoint;
    int OtherPlayerPrevTurnPoint;

    void Start()
    {
        GameMangerObj = GameObject.Find("GameManager");
        GameManagerScript = GameMangerObj.GetComponent<GameManager>();

        PhaseText = PhaseTextObj.GetComponent<Text>();
        PlayerPointText = PlayerPointTextObj.GetComponent<Text>();
        OtherPlayerPointText = OtherPlayerPointTextObj.GetComponent<Text>();
        PlayerTimeText = PlayerTimeTextObj.GetComponent<Text>();
        OtherPlayerTimeText = OtherPlayerTimeTextObj.GetComponent<Text>();
        PlayerLogText = PlayerLogObj.GetComponent<Text>();

        //point
        firstTurn = true;
        PlayerPrevTurnPoint = 9999;
        OtherPlayerPrevTurnPoint = 9999;

        UpdateSelectPhaseText();
    }

    public void showPlayerLog(string log)
    {
        PlayerLogText.text = log;
    }

    public override void StartPhase()
    {
        base.StartPhase();
        UpdateSelectPhaseText();
    }
    public override void NextPhase()
    {
        base.NextPhase();
        UpdateSelectPhaseText();
    }

    public override void gotoWinPhase()
    {
        base.gotoWinPhase();
        UpdateSelectPhaseText();
    }

    public override void gotoLosePhase()
    {
        base.gotoLosePhase();
        UpdateSelectPhaseText();
    }

    public override void gotoDrawPhase()
    {
        base.gotoDrawPhase();
        UpdateSelectPhaseText();
    }

    public override void gotoCheckPhase()
    {
        base.gotoCheckPhase();
        UpdateSelectPhaseText();
    }

    void UpdateSelectPhaseText()
    {
        string updateText = "";

        switch (GetGamePhase())
        {
            case GamePhase.NotPhase:
                updateText = "Player Waiting";
                break;
            case GamePhase.CommandPhase:
                updateText= "Playing Game";
                PlayerLogText.text = "";
                break;
            
            case GamePhase.SyncPhase:
                updateText = "Waiting Turn ";
                break;
            case GamePhase.EnforcementPhase:
                updateText = "Waiting Turn ";
                firstTurn = false;
                PlayerPrevTurnPoint = GameManagerScript.ObstacleAmount;
                OtherPlayerPrevTurnPoint = GameManagerScript.OtherPlayerCharactorScript.point;
                break;
            case GamePhase.WinPhase:
                updateText = "You Win";
                break;
            case GamePhase.LosePhase:
                updateText = "You Lose";
                break;
            case GamePhase.DrawPhase:
                updateText = "Draw";
                break;
            case GamePhase.CheckPhase:
                updateText = "Check";
                break;
            default:
                updateText = "unknown";
                break;
        }
        PhaseText.text = updateText;
    }

    string TimeDataToTimeTextData(float timeData)
    {
        float min = Mathf.Floor(timeData / 60);
        float sec = Mathf.Floor(timeData % 60);
        return min.ToString() + ":" + sec.ToString();
    }

    void Update()
    {
        //point
        string PlayerPointTextPrefix = "현재 포인트 : ";
        string OtherPlayerPointTextPrefix = "상대 포인트 : ";
        
        if (firstTurn)
        {
            PlayerPointText.text = PlayerPointTextPrefix + GameManagerScript.ObstacleAmount.ToString();
            if (GameManagerScript.OtherPlayerCharactorScript != null)
            {
                OtherPlayerPointText.text = OtherPlayerPointTextPrefix + GameManagerScript.OtherPlayerCharactorScript.point.ToString();
            }
        }
        else
        {
            string PrevPointDifferenceText = (GameManagerScript.ObstacleAmount- PlayerPrevTurnPoint).ToString();
            PlayerPointText.text = PlayerPointTextPrefix + GameManagerScript.ObstacleAmount.ToString()
                                    + "(" + PrevPointDifferenceText + ")";
            if (GameManagerScript.OtherPlayerCharactorScript != null)
            {
                string PrevOtherPlayerPointDifferenceText = (GameManagerScript.OtherPlayerCharactorScript.point - OtherPlayerPrevTurnPoint).ToString();
                OtherPlayerPointText.text = OtherPlayerPointTextPrefix + GameManagerScript.OtherPlayerCharactorScript.point.ToString()
                    +"("+PrevOtherPlayerPointDifferenceText+")";
            }
        }

        //포인트 변경시 (-0)으로 표시
        
        
        //time
        string PlayerTimeTextPrefix = "현재 남은 시간 : ";
        string OtherPlayerTimeTextPrefix = "상대 남은 시간 : ";
        string PlayerSecondTimeTextPrefix = "현재 초읽기 : ";
        string OtherPlayerSecondTimeTextPrefix = "상대 초읽기 : ";

        if (!GameManagerScript.Overtime)
        {
            string timeTextData = TimeDataToTimeTextData(GameManagerScript.Turntime_second);
            PlayerTimeText.text = PlayerTimeTextPrefix + timeTextData;
        }
        else
        {
            string timeTextData = TimeDataToTimeTextData(GameManagerScript.Overtime_second);
            PlayerTimeText.text = PlayerSecondTimeTextPrefix + timeTextData;
        }
        
        
        //Other Player
        if (GameManagerScript.OtherPlayerCharactorScript != null)
        {
            if (!GameManagerScript.OtherPlayerCharactorScript.Overtime)
            {
                string timeTextData = TimeDataToTimeTextData(GameManagerScript.OtherPlayerCharactorScript.Turntime_second);
                OtherPlayerTimeText.text = OtherPlayerTimeTextPrefix + timeTextData;
            }
            else
            {
                string timeTextData = TimeDataToTimeTextData(GameManagerScript.OtherPlayerCharactorScript.Overtime_second);
                OtherPlayerTimeText.text = OtherPlayerSecondTimeTextPrefix + timeTextData;
            }
        }
    }

    //버튼 확인 
    public override void SelectMoveView()
    {
        base.SelectMoveView();
        GameMangerObj.SendMessage("SelectMoveView");
    }

    public override void SelectObstacleView()
    {
        base.SelectObstacleView();
        GameMangerObj.SendMessage("SelectObstacleView");
    }

    public void LeftObstacleClicked()
    {
        GameMangerObj.SendMessage("SetObstacleDirection", Direction.Left);
        SelectObstacleView();
    }
    public void RightObstacleClicked()
    {
        GameMangerObj.SendMessage("SetObstacleDirection", Direction.Right);
        SelectObstacleView();
    }
    public void TopObstacleClicked()
    {
        GameMangerObj.SendMessage("SetObstacleDirection", Direction.Top);
        SelectObstacleView();
    }
    public void BottomObstacleClicked()
    {
        GameMangerObj.SendMessage("SetObstacleDirection", Direction.Bottom);
        SelectObstacleView();
    }

    public void NextTurnClicked()
    {
        GameMangerObj.SendMessage("commandNextTurn");
    }
}

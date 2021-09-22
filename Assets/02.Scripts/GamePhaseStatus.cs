using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    NotPhase,
    CommandPhase,
    SyncPhase,
    EnforcementPhase,
    WinPhase,
    LosePhase,
    DrawPhase,
    CheckPhase
}

public enum ViewType
{
    ObstacleView,
    MoveView
}

public class GamePhaseStatus : MonoBehaviour
{
    private GamePhase Status=GamePhase.NotPhase;
    ViewType viewType = ViewType.MoveView;
    virtual public void StartPhase()
    {
        Status = GamePhase.CommandPhase;

    }
    virtual public void NextPhase()
    {
        switch(Status)
        {
            case GamePhase.CommandPhase:
                Status = GamePhase.SyncPhase;
                break;
            case GamePhase.SyncPhase:
                Status = GamePhase.EnforcementPhase;
                break;
            case GamePhase.EnforcementPhase:
                Status = GamePhase.CommandPhase;
                break;
            default:
                Debug.Assert(true);
                break;
        }
    }

    virtual public void gotoWinPhase()
    {
        Status = GamePhase.WinPhase;
    }

    virtual public void gotoLosePhase()
    {
        Status = GamePhase.LosePhase;
    }

    virtual public void gotoDrawPhase()
    {
        Status = GamePhase.DrawPhase;
    }

    virtual public void gotoCheckPhase()
    {
        Status = GamePhase.CheckPhase;
    }


    public GamePhase GetGamePhase()
    {
        return Status;
    }

    virtual public void SelectObstacleView()
    {
        viewType = ViewType.ObstacleView;
    }
    virtual public void SelectMoveView()
    {
        viewType = ViewType.MoveView;
    }
    public ViewType GetViewType()
    {
        return viewType;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class PlaneCubeStatus : GamePhaseStatus
{
    public Material materialCommon;
    public Material materialCanGo;
    public Material materialCanNotObstacle;
    public Material materialCanDeleteGoObstalce;

    public Material materialGold;
    //다른 상태가 있는 경우
    public Material materialOtherGold;

    public abstract bool checkCanGo();
    public abstract bool checkCanInstallObstacle();
    public abstract bool checkCanDeleteGoObstalce();
    public abstract bool checkAdditionalPoint();
    public void RefreshView()
    {
        CalculateAdjustMaterial();
        AdjustMaterial();
    }

    Material adjustMaterial;
    //thread-safe

    public void CalculateAdjustMaterial()
    {
        if (GetGamePhase() == GamePhase.CommandPhase)
        {
            if (GetViewType() == ViewType.MoveView)
            {
                if (checkCanGo())
                {
                    if(checkAdditionalPoint()==true)
                    {
                        adjustMaterial = materialOtherGold;
                    }
                    else
                    {
                        adjustMaterial = materialCanGo;
                    }
                }
                else
                {
                    if (checkCanDeleteGoObstalce())
                        adjustMaterial = materialCanDeleteGoObstalce;
                    else
                    {
                        if (checkAdditionalPoint() == true)
                        {
                            adjustMaterial = materialGold;
                        }
                        else
                        {
                            adjustMaterial = materialCommon;
                        }
                    }
                }
            }
            else if (GetViewType() == ViewType.ObstacleView)
            {
                if (checkCanInstallObstacle())
                {
                    if (checkAdditionalPoint() == true)
                    {
                        adjustMaterial = materialGold;
                    }
                    else
                    {
                        adjustMaterial = materialCommon;
                    }
                }
                else
                {
                    if (checkAdditionalPoint() == true)
                    {
                        adjustMaterial = materialOtherGold;
                    }
                    else
                    {
                        adjustMaterial = materialCanNotObstacle;
                    }
                }
            }
        }
    }
    //thread-unsafe
    public void AdjustMaterial()
    {
        Renderer this_renderer = GetComponent<Renderer>();
        if (this_renderer == null)
        {
            Debug.Log("planecube renderer error");
        }
        this_renderer.material = adjustMaterial;
    }
}

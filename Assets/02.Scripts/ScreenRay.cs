using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenRay : GamePhaseStatus
{
    GameObject GameMangerObj;
    GameObject PlaneMapManagerObj;
    PlaneCubeMapManager PlaneMapManagerScript;
    void Start()
    {
        GameMangerObj = GameObject.Find("GameManager");
        PlaneMapManagerObj = GameObject.Find("PlaneCubeMapManager");
        PlaneMapManagerScript = PlaneMapManagerObj.GetComponent<PlaneCubeMapManager>();
    }

    void Update()
    {
        GameObject planecubeObj = null;
        if (Input.GetButtonDown("Fire1"))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag.Equals("TagPlaneCube"))
                {
                    planecubeObj = hit.transform.gameObject;
                }
            }
        }

        if(planecubeObj==null)
        {
            return;
        }
        else
        {
            (int, int) xz = PlaneMapManagerScript.GetCubeCoordinate(planecubeObj);
            PlaneCube planeCube = planecubeObj.GetComponent<PlaneCube>();
            if(GetGamePhase()==GamePhase.CommandPhase)
            {
                if(GetViewType()== ViewType.MoveView)
                {
                    if (planeCube.checkCanGo())
                    {
                        GameMangerObj.SendMessage("commandMove", xz);
                    }
                    else if(planeCube.checkCanDeleteGoObstalce())
                    {
                        //
                        GameMangerObj.SendMessage("commandDestroyObstacleAndGo", xz);
                    }
                }
                else if(GetViewType() == ViewType.ObstacleView)
                {
                    if (planeCube.checkCanInstallObstacle())
                    {
                        GameMangerObj.SendMessage("commandObstacle", xz);
                    }
                }
            }
        }
    }
}

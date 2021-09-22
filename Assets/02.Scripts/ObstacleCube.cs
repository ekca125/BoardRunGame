using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCube : MonoBehaviour
{
    bool FirstInstantiate;
    float UpMeter;
    Vector3 OneFrameVector;
    Vector3 FinishVector;

    void Start()
    {
        FirstInstantiate = true;
        UpMeter = 0.5F;
        OneFrameVector = new Vector3(0,
                        (UpMeter/Constants.DEFUALT_ANIMATION_SECOND)/ Constants.DEFAULT_FRAME,
                         0);
        FinishVector = new Vector3( transform.position.x,
                                    transform.position.y+UpMeter,
                                    transform.position.z);
        Invoke("FinishTranslate", Constants.DEFUALT_ANIMATION_SECOND);
    }

    void FinishTranslate()
    {
        //주어진 시간을 초과하면 프레임을 모두 무시하고 위치시킨다.
        transform.position = FinishVector;
        FirstInstantiate = false;
    }

    private void FixedUpdate()
    {
        if (FirstInstantiate == true)
        {
            transform.Translate(OneFrameVector);
        }
    }
}
// RelativeFrame = Time.deltaTime/0.002


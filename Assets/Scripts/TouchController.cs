using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : MonoBehaviour
{
    public GameObject target;// saves the selected discs refernce
    public GameController gameController;
    public float snapDistance; // distance w.r.t tower after dropping the disc
    public LayerMask m_Mask = -1; // to cast ray only on discs

    private bool isMouseDrag;
    private float hitClamp; //y position after hitting any disc while moving or snapping
    private Vector3 screenPosition, offset, lastPosition;

    void Update()
    {

        if (Input.GetMouseButtonDown(0) && gameController.winState ==0  && gameController.isAnimating == false)
        {
            RaycastHit hitInfo;

            target = ReturnClickedObject(out hitInfo);

            if (target != null)
            {
                isMouseDrag = true;
                lastPosition = target.transform.position;
                //Convert world position to screen position.
                screenPosition = Camera.main.WorldToScreenPoint(target.transform.position);
                offset = target.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z));
            }
            hitClamp = 0;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isMouseDrag = false;
            hitClamp = 0;

            //don't check for position if no disc was selected
            if(target!=null)
                CheckDiscPosition();

            target = null;
        }

        if (isMouseDrag)
        {
            //track mouse position.
            Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);

            //convert screen position to world position with offset changes.
            Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace) + offset;
            currentPosition.z = 0;
 
            RaycastHit hit;

            //Stop moving the disc in y if if stacks on another disc
            if (Physics.Raycast(target.transform.position, Vector3.down, out hit, 5, m_Mask.value, QueryTriggerInteraction.Collide))
            {
                hitClamp = hit.transform.position.y + target.transform.localScale.y * gameController.hieghtFactor;
            }
            else
            {
                // no disc found inside the current 
                hitClamp = 0;
            }

            currentPosition.y = Mathf.Clamp(currentPosition.y, hitClamp, gameController.waypointA.y);

            //Check if disc is inside the tower or above tower
            if (currentPosition.y < gameController.waypointA.y)
            {
                //if disc is inside the tower 'A' then snap to it
                if (target.transform.position.x < gameController.waypointA.x + snapDistance)
                {
                    currentPosition.x = gameController.waypointA.x;
                }
                //if disc is inside the tower 'B' then snap to it
                else if (target.transform.position.x > gameController.waypointC.x - snapDistance)
                {
                    currentPosition.x = gameController.waypointC.x;
                }
                //if disc is inside the tower 'C' then snap to it
                else if (target.transform.position.x < gameController.waypointB.x + snapDistance && target.transform.position.x > gameController.waypointB.x - snapDistance)
                {
                    currentPosition.x = gameController.waypointB.x;
                }
            }
            else
            {
               //Disc is above the tower so fix the y position and clamp x between tower 'A' and 'C'
                currentPosition.x = Mathf.Clamp(currentPosition.x, gameController.waypointA.x, gameController.waypointC.x);
            }
            
            
            target.transform.position = currentPosition;
        }

    }

    // Check disc position after release 
    public void CheckDiscPosition()
    {

        RaycastHit hit;
        Vector3 snapPos = new Vector3();

        //if near to wavepoint 'A' within snap distance then snap to 'A'
        if (target.transform.position.x < gameController.waypointA.x + snapDistance)
        {
            snapPos = gameController.waypointA;
            snapPos.y = 0;

        }
        //if near to wavepoint 'B' within snap distance then snap to 'B'
        else if (target.transform.position.x > gameController.waypointC.x - snapDistance) 
        {
            snapPos = gameController.waypointC;
            snapPos.y = 0;

        }
        //if near to wavepoint 'C' within snap distance then snap to 'C'
        else if (target.transform.position.x < gameController.waypointB.x + snapDistance && target.transform.position.x > gameController.waypointB.x - snapDistance)
        {
            snapPos = gameController.waypointB;
            snapPos.y = 0;

        }
        else
        {
            //Disc dropped out of snap distance between towers move back to last position
            gameController.ShowAlert("Please drop the disc on top of tower.",2);
            snapPos = lastPosition;
        }

        //Check y position of selected disc after dropping and snapping
        //if any disc is already in the tower then stack on top of that
        if (Physics.Raycast(target.transform.position, Vector3.down, out hit, 5, m_Mask.value, QueryTriggerInteraction.Collide))
        {
            hitClamp = hit.transform.position.y + target.transform.localScale.y * gameController.hieghtFactor;

            if (hit.transform.localScale.x < target.transform.localScale.x)
            {
                Debug.Log("Can't place it here" + lastPosition);
                gameController.ShowAlert("Can't place bigger disc on top of smaller disc.", 2);
                snapPos = lastPosition;
            }
            else
            {
                snapPos.y = hitClamp;
            }

        }

        // Move to final snap position after snap and stack check
        MoveTarget(snapPos);
    }



    public void MoveTarget(Vector3 position)
    {
        gameController.moves++;
        //speed on animation according to the current disc postion and snapped tower position
        float distance = Vector3.Distance(target.transform.position, position);
       

       //Tween to target location and check for game complete condition after moving
        iTween.MoveTo(target, iTween.Hash(
            "position", position,
            "time", distance / 10,
            "oncomplete", "CheckGameComplete",
            "oncompletetarget", gameObject
            ));
    }

    //Checks whether game is completed or not
    public void CheckGameComplete()
    {
        Debug.Log("Check Complete");

        for(int i=0;i<gameController.numberOfDiscs;i++)
        {
            //Checks if any disc is not in towerC then return
            if(!Mathf.Approximately(gameController.disks[i].transform.position.x,gameController.waypointC.x))
            {
                return;
            }

            //Checks if it is last disc then show complete
            if(i==gameController.numberOfDiscs-1)
            {
                gameController.winState++;
                Debug.Log("Game Complete");
                gameController.GameComplete();
            }
        }
    }

    // Return the selected disc 
    GameObject ReturnClickedObject(out RaycastHit hit)
    {
        GameObject target = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray.origin,ray.direction* 10, out hit,10, m_Mask.value, QueryTriggerInteraction.Ignore))
        {
            target = hit.collider.gameObject;

            if (Physics.Raycast(target.transform.position, Vector3.up, out hit, 1, m_Mask.value, QueryTriggerInteraction.Ignore))
            {
                target = null;
            }
            
        }
        return target;
    }
}

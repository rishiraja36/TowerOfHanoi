using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : MonoBehaviour
{
    public GameObject target;
    private bool isMouseDrag;
    public GameController gameController;
    private Vector3 screenPosition, offset,lastPosition;
    public float snapDistance;
    public LayerMask m_Mask = -1;
    private float hitClamp;

    // Start is called before the first frame update
    void Start()
    {
        
    }

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
            
            if (Physics.Raycast(target.transform.position, Vector3.down, out hit, 5, m_Mask.value, QueryTriggerInteraction.Collide))
            {
                hitClamp = hit.transform.position.y + target.transform.localScale.y * gameController.hieghtFactor;
            }
            else
            {
                hitClamp = 0;
            }

            currentPosition.y = Mathf.Clamp(currentPosition.y, hitClamp, gameController.waypointA.y);

            if (currentPosition.y < gameController.waypointA.y)
            {
                if (target.transform.position.x < gameController.waypointA.x + snapDistance)
                {
                    currentPosition.x = gameController.waypointA.x;
                }
                else if (target.transform.position.x > gameController.waypointC.x - snapDistance)
                {
                    currentPosition.x = gameController.waypointC.x;
                }
                else if (target.transform.position.x < gameController.waypointB.x + snapDistance && target.transform.position.x > gameController.waypointB.x - snapDistance)
                {
                    currentPosition.x = gameController.waypointB.x;
                }
               /* else if ((target.transform.position.x > gameController.waypointA.x + movementOffset && target.transform.position.x < gameController.waypointB.x - movementOffset)
                    || (target.transform.position.x > gameController.waypointB.x + movementOffset && target.transform.position.x < gameController.waypointC.x - movementOffset))
                {
                    currentPosition.y = gameController.waypointA.y;
                }
                else
                {
                    currentPosition.x = gameController.waypointB.x;
                }*/

               
            }
            else
            {
               
                currentPosition.x = Mathf.Clamp(currentPosition.x, gameController.waypointA.x, gameController.waypointC.x);
            }
            
            
            target.transform.position = currentPosition;
        }

    }

    public void CheckDiscPosition()
    {

        RaycastHit hit;
        Vector3 snapPos = new Vector3();

        if (target.transform.position.x < gameController.waypointA.x + snapDistance)
        {
            snapPos = gameController.waypointA;
            snapPos.y = 0;

            if (Physics.Raycast(target.transform.position, Vector3.down, out hit, 5, m_Mask.value, QueryTriggerInteraction.Collide))
            {
                Debug.Log(hit.transform.name + " " + hit.transform.position.y + " " + hitClamp+ " "+target.transform.name);

                    hitClamp = hit.transform.position.y + target.transform.localScale.y * gameController.hieghtFactor;
                   
                    if (hit.transform.localScale.x < target.transform.localScale.x)
                    {
                        Debug.Log("Can't place it here" + lastPosition);
                        snapPos = lastPosition;
                    }
                    else
                    {
                        //  snapPos = hit.transform.position;
                        snapPos.y = hitClamp;
                    }
                
                //}
            }

            Debug.Log("Move to A" + snapPos + " " + gameController.waypointA);
        }
        else if (target.transform.position.x > gameController.waypointC.x - snapDistance)
        {
            snapPos = gameController.waypointC;
            snapPos.y = 0;

            if (Physics.Raycast(target.transform.position, Vector3.down, out hit, 5, m_Mask.value, QueryTriggerInteraction.Collide))
            {
                Debug.Log(hit.transform.name + " " + hit.transform.position.y + " " + hitClamp + " " + target.transform.name);

                
                    hitClamp = hit.transform.position.y + target.transform.localScale.y * gameController.hieghtFactor;
                    
                    if (hit.transform.localScale.x < target.transform.localScale.x)
                    {
                        Debug.Log("Can't place it here" + lastPosition);
                        snapPos = lastPosition;
                    }
                    else
                    {
                        snapPos.y = hitClamp;
                    }
                
            }
            Debug.Log("Move to C" + snapPos + " " + gameController.waypointC);
        }
        else if (target.transform.position.x < gameController.waypointB.x + snapDistance && target.transform.position.x > gameController.waypointB.x - snapDistance)
        {
            snapPos = gameController.waypointB;
            snapPos.y = 0;
            if (Physics.Raycast(target.transform.position, Vector3.down, out hit, 5, m_Mask.value, QueryTriggerInteraction.Collide))
            {
                Debug.Log(hit.transform.name + " " + hit.transform.position.y + " " + hitClamp + " " + target.transform.name);

                    hitClamp = hit.transform.position.y + target.transform.localScale.y * gameController.hieghtFactor;

                    Debug.Log(hit.transform.name + " " + hit.transform.position.y + " " + hitClamp);

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
            Debug.Log("Move to B" + snapPos + " " + gameController.waypointB);
        }
        else
        {
            Debug.Log("Can't snap to any tower move back" + lastPosition);
            gameController.ShowAlert("Please drop the disc on top of tower.",2);
            snapPos = lastPosition;
        }
  
        MoveTarget(snapPos);
    }

    public void MoveTarget(Vector3 position)
    {
        gameController.moves++;
        float distance = Vector3.Distance(target.transform.position, position);
       

       // iTween.MoveTo(target, position, distance / 10);
        iTween.MoveTo(target, iTween.Hash(
            "position", position,
            "time", distance / 10,
            "oncomplete", "CheckGameComplete",
            "oncompletetarget", gameObject
            ));
    }

    public void CheckGameComplete()
    {
        Debug.Log("Check Complete");

        for(int i=0;i<gameController.numberOfDiscs;i++)
        {
            if(!Mathf.Approximately(gameController.disks[i].transform.position.x,gameController.waypointC.x))
            {
                return;
            }
            if(i==gameController.numberOfDiscs-1)
            {
                gameController.winState++;
                Debug.Log("Game Complete");
                gameController.GameComplete();
            }
        }
    }

    // Return the selected object 
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

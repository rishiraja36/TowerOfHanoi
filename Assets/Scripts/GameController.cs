using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public GameObject[] disks;//Discs Refrences
    // wavepoints on top of tower for snapping and moving purpose
    public Vector3 waypointA;
    public Vector3 waypointB;
    public Vector3 waypointC;

    public int numberOfDiscs = 6,moves= 0, winState=0;
    public float speed = 1,hieghtFactor=1.2f, timer;
    public bool isAnimating;

    public Text timerTxt, movesTxt,alertTxt,totalTimeTxt,totalMovesTxt;
    public GameObject pausePanel, alertPanel, gameCompletePanel;

    //List of sequence of discs and respective target position
    List<GameObject> diskSeq = new List<GameObject>();
    List<Vector3> posASeq = new List<Vector3>();
    List<Vector3> posBSeq = new List<Vector3>();
    int i = -1;

    // Use this for initialization
    void Start ()
    {
        // assigning the referenes
        waypointA = GameObject.Find("wayPoint A").transform.position;
        waypointB = GameObject.Find("wayPoint B").transform.position;
        waypointC = GameObject.Find("wayPoint C").transform.position;
        ResetGame();
    }

    // Update is called once per frame
    
    void Update ()
    {

        if(!isAnimating && winState == 0)
        {
            timer += Time.deltaTime;
           
            timerTxt.text = floatToTime(timer);
        }
       
        movesTxt.text ="Moves :"+ moves.ToString();
    }

    // Resets the game
    public void ResetGame()
    {
        for (int i = 0; i < disks.Length; i++)
        {
            if (i < numberOfDiscs)
            {
                disks[i].SetActive(true);
                Vector3 pos = waypointA;
                pos.y = (disks[i].transform.localScale.y * hieghtFactor) * (numberOfDiscs - i - 1);
                disks[i].transform.position = pos;
            }
            else
            {
                disks[i].SetActive(false);

            }
        }

        moves = 0;
        timer = 0;
        winState = 0;

    }
    public void RestartGame()
    {
        Time.timeScale = 1;
        ResetGame();
        gameCompletePanel.SetActive(false);
        pausePanel.SetActive(false);
    }
    public void PauseGame()
    {
        Time.timeScale = 0;
        alertPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
        alertPanel.SetActive(false);
    }

    public void ShowAlert(string message,float time)
    {
        StartCoroutine(IShowAlert(message, time));
    }

    public void GameComplete()
    {
        gameCompletePanel.SetActive(true);
        totalMovesTxt.text = "Total Moves : "+moves.ToString();
        totalTimeTxt.text = "Total Time : " + floatToTime(timer);
    }

    string floatToTime(float timer)
    {
        string minutes = Mathf.Floor(timer / 60).ToString("00");
        string seconds = (timer % 60).ToString("00");
        return string.Format("{0}m:{1}s", minutes, seconds);
    }

    IEnumerator IShowAlert(string message,float closeTime)
    {
        alertPanel.SetActive(true);
        alertTxt.text = message;
        yield return new WaitForSeconds(closeTime);
        alertPanel.SetActive(false);
    }

    //region of automatic solution
    #region

    //Show animating sequences for tower of hanoi solution
    public void ShowSolution()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
        ResetGame();
        hanoi(numberOfDiscs, waypointA, waypointC, waypointB);
        Debug.Log("IN ARRAYS===>");
        for (int i = 0; i < Mathf.Pow(2, numberOfDiscs) - 1; i++)
        {
            Debug.Log("Move disk " + diskSeq[i].name + " from " + posASeq[i] + " to " + posBSeq[i]);
        }
        isAnimating = true;
        StartCoroutine(moveAll());
    }

    // Moving discs as per sequence to thier target location as per the list
    IEnumerator moveAll()
    {
        for (int i = 0; i < Mathf.Pow(2, numberOfDiscs) - 1; i++)
        {
            yield return StartCoroutine(move(diskSeq[i], posASeq[i], posBSeq[i]));
            if (i == Mathf.Pow(2, numberOfDiscs) - 3)
                winState++;
        }
        isAnimating = false;
        GameComplete();
    }

    // A recurrsive function to find the solution for the given discs
    void hanoi(int n, Vector3 from, Vector3 to, Vector3 aux)
    {
        if (n == 1)
        {
            addToSequence(disks[0], from, to);
            return;
        }
        hanoi(n - 1, from, aux, to);
        addToSequence(disks[n - 1], from, to);
        hanoi(n - 1, aux, to, from);
    }

    // Saving the sequences in the lists which disc should move to which location
    void addToSequence(GameObject disk, Vector3 from, Vector3 to)
    {
        i++;
        diskSeq.Add(disk);
        posASeq.Add(from);
        posBSeq.Add(to);

    }

    // moving the disc to thier targeted location using itween for tweening
    public IEnumerator move(GameObject disk, Vector3 posA, Vector3 posB)
    {
        iTween.MoveTo(disk, posA, 2 / speed);
        yield return new WaitForSeconds(2 / speed);
        iTween.MoveTo(disk, posB, 1 / speed);
        yield return new WaitForSeconds(1 / speed);

        RaycastHit hit;
        if (Physics.Raycast(posB + Vector3.down * 2, Vector3.down, out hit, 6))
        {
            iTween.MoveTo(disk, hit.point + Vector3.up * .3f, 2 / speed);
            yield return new WaitForSeconds(2 / speed);
        }
    }
    #endregion



}

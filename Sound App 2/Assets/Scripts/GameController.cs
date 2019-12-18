using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Networking;
//so I can edit the data of the array in the data controller
using System.Linq;

public class GameController : MonoBehaviour {

    //so i can use randnum next and not get an error because Unity has System.Random, too
    public System.Random randnum = new System.Random();

    public Text scoreDisplayText;
    public Text timeRemainingDisplayText;
    public SimpleObjectPool answerButtonObjectPool;
    public Transform answerButtonParent;
    public GameObject roundEndDisplay;
    public GameObject questionDisplay;
    public Button replay;

    public Image bad;
    public Image good;
    public Animator animb;
    public Animator animg;

    public AudioSource questionAudioSource;
    private float ClipLength;
    public AudioSource bgMusic;
    private AudioClip questionClip;

    private DataController dataController;
    private RoundData currentRoundData;
    private QuestionData[] questionPool;
    private int questionCounter;
    public List<int> questionsAsked;

    private bool isRoundActive;
    private bool spokenQuestion;
    private float timeRemaining;
    private int questionIndex;
    private int playerScore;
    Dictionary<int, float> wrongScore = new Dictionary<int, float>();
    int wrongScoreInt = 0;
    private List<GameObject> answerButtonGameObjects = new List<GameObject>();

    // Use this for initialization
    void Start ()
    {
        dataController = FindObjectOfType<DataController> ();
        currentRoundData = dataController.GetCurrentRoundData ();
        questionPool = currentRoundData.questions;
        timeRemaining = currentRoundData.timeLimitInSeconds;
        UpdateTimeRemainingDisplay();

        playerScore = 0;
        wrongScoreInt = 0;

        //Set the list
        List<int> questionsAsked = new List<int>();
        //Get a random number for the initial index value, and add it to the array
        questionIndex = Random.Range(0, questionPool.Length);
        questionCounter = 0;
        Debug.Log(questionIndex);

        questionAudioSource.pitch = (float)1.00;

        //animg = good.GetComponent<Animator>();
        //animb = bad.GetComponent<Animator>();

        ShowQuestion ();
        isRoundActive = true;

    }

    IEnumerator GetAudioClip()
    {
      QuestionData questionData = questionPool [questionIndex];
      Debug.Log(questionData.questionSound);
      using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(questionData.questionSound, AudioType.OGGVORBIS))
      {
          yield return www.Send();

          if (www.isNetworkError)
          {
              Debug.Log(www.error);
          }
          else
          {
              questionClip = DownloadHandlerAudioClip.GetContent(www);
              Debug.Log(questionClip);
              questionAudioSource.clip = questionClip;

              //Pitch Correct and get clip length
              questionAudioSource.outputAudioMixerGroup.audioMixer.SetFloat("pitchParam", 1f / questionAudioSource.pitch);
              ClipLength = questionAudioSource.clip.length / questionAudioSource.pitch;
              Debug.Log("Clip:" + ClipLength + " Speed:" + questionAudioSource.pitch);

              //Play Audio
              questionAudioSource.Play();
              //Show buttons after audio is done
              StartCoroutine(AssignAnswerButtons(ClipLength));
          }
      }
    }

    private void ShowQuestion()
    {
        if (questionCounter < questionPool.Length){
          questionCounter ++;
        }
        //hide replay button
        Vector3 replayscale = new Vector3(0,0,0);
        replay.transform.localScale = replayscale;

        //reset the time
        spokenQuestion = false;
        timeRemaining = currentRoundData.timeLimitInSeconds;

        //Remove the old answer buttons, first
        RemoveAnswerButtons ();

        //Store into question data the question one data
        //QuestionData questionData = questionPool [questionIndex];
        //fetch the audio clip from the server
        StartCoroutine(GetAudioClip());

        /*//questionAudioSource.clip = (AudioClip)questionData.questionSound;
        questionAudioSource.clip = questionClip;

        //Pitch Correct and get clip length
        questionAudioSource.outputAudioMixerGroup.audioMixer.SetFloat("pitchParam", 1f / questionAudioSource.pitch);
        ClipLength = questionAudioSource.clip.length / questionAudioSource.pitch;
        Debug.Log("Clip:" + ClipLength + " Speed:" + questionAudioSource.pitch);

        //Play Audio
        questionAudioSource.Play();
        //Show buttons after audio is done
        StartCoroutine(AssignAnswerButtons(ClipLength));*/
    }
    IEnumerator AssignAnswerButtons(float time)
    {
      yield return new WaitForSeconds(time);
        // Code to execute after the delay

        QuestionData questionData = questionPool [questionIndex];
        //Asign answers to boxes
        Random rnd = new Random();
        //randomizes output
        var answersInRandomOrder = questionData.answers.OrderBy(x => randnum.Next()).ToArray();

        for (int i = 0; i < answersInRandomOrder.Length; i++)
        {
            GameObject answerButtonGameObject = answerButtonObjectPool.GetObject();
            answerButtonGameObjects.Add(answerButtonGameObject);
            answerButtonGameObject.transform.SetParent(answerButtonParent);

            AnswerButton answerButton = answerButtonGameObject.GetComponent<AnswerButton>();
            answerButton.Setup(answersInRandomOrder[i]);
        }
        //The question is done now, show button and play bg music
        spokenQuestion = true;
        bgMusic.Play();
        Vector3 replayscale = new Vector3(1,1,1);
        replay.transform.localScale = replayscale;
     }

    public void ReplayAudio()
    {
      //hide replay button
      Vector3 replayscale = new Vector3(0,0,0);
      replay.transform.localScale = replayscale;

      spokenQuestion = false;
      bgMusic.Stop();
      questionAudioSource.Play();
      StartCoroutine(ReplayButton(ClipLength));
    }

    IEnumerator ReplayButton(float time)
    {
      yield return new WaitForSeconds(time);
        // start the clock again, show the replay button, and start up the music again
        spokenQuestion = true;
        Vector3 replayscale = new Vector3(1,1,1);
        replay.transform.localScale = replayscale;
        bgMusic.Play();
     }

    private void RemoveAnswerButtons()
    {
        while (answerButtonGameObjects.Count > 0)
        {
            answerButtonObjectPool.ReturnObject(answerButtonGameObjects[0]);
            answerButtonGameObjects.RemoveAt(0);
        }
    }

    void ChooseQuestion()
    {
      int random = Random.Range(0, questionPool.Length);
      while(questionsAsked.Contains(random)){
        random = Random.Range(0, questionPool.Length);
      }
      questionIndex = random;
    }

    public void AnswerButtonClicked(bool isCorrect)
    {
        if (isCorrect)
        {
            //score goes up, speed goes up
            playerScore += currentRoundData.pointsAddedForCorrectAnswer;
            scoreDisplayText.text = "Score: " + playerScore.ToString();
            questionAudioSource.pitch += (float)0.25;

            animg.Play("goodAnimationOut");
        } else {
          //they got it wrong, log what they're wrong score is and the speed they got it wrong at. slow down the audio, too
          wrongScoreInt ++;
          wrongScore.Add(wrongScoreInt,questionAudioSource.pitch);
          questionAudioSource.pitch -= (float)0.25;

          animb.Play("badAnimationOut");
        }

        if (questionCounter < questionPool.Length) {
            questionsAsked.Add(questionIndex);
            ChooseQuestion ();
            bgMusic.Stop();
            ShowQuestion ();
        } else
        {
            //end round if there are no more questions
            EndRound();
        }

    }

    public void EndRound()
    {
        isRoundActive = false;

        questionDisplay.SetActive (false);
        roundEndDisplay.SetActive (true);
    }

    private void UpdateTimeRemainingDisplay()
    {
        timeRemainingDisplayText.text = "Time: " + Mathf.Round (timeRemaining).ToString ();
    }

    // Update is called once per frame
    void Update ()
    {
        if (isRoundActive)
        {
            //wait for the question to be spoken
            if (spokenQuestion) {
              timeRemaining -= Time.deltaTime;
              UpdateTimeRemainingDisplay();
            }

            if (timeRemaining <= 0f)
            {
                //No time left. Count it as a wrong question
                AnswerButtonClicked(false);
            }

        }
    }
}

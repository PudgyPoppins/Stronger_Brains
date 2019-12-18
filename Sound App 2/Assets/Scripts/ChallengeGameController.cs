using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Networking;
//so I can edit the data of the array in the data controller
using System.Linq;

public class ChallengeGameController : MonoBehaviour {

    //so i can use randnum next and not get an error because Unity has System.Random, too
    public System.Random randnum = new System.Random();

    public Text scoreDisplayText;
    public Text timeRemainingDisplayText;
    public SimpleObjectPool answerButtonObjectPool;
    public Transform answerButtonParent;
    public GameObject roundEndDisplay;
    public GameObject questionDisplay;
    public Button replay;

    public AudioSource questionAudioSource;
    private float ClipLengthNew;
    private float ClipLengthTotal;
    private float soundSpeed;
    public AudioSource bgMusic;
    private AudioClip questionClip;

    private DataController dataController;
    private RoundData currentRoundData;
    private QuestionData[] questionPool;
    private int questionCounter;
    public List<int> questionsAsked;

    private bool isRoundActive;
    private bool spokenQuestion;
    private bool showAnswers;
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
        soundSpeed = 1;

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

              //undo pitch correcting
              questionAudioSource.outputAudioMixerGroup.audioMixer.SetFloat("pitchParam", 1f / 1);
              questionAudioSource.pitch = 1;
              questionAudioSource.time = 0;
              questionAudioSource.Play();
              //After x seconds, change speed
              showAnswers = true;
              StartCoroutine(ChangeSpeed((float)questionData.timeUntilImportant));
          }
      }
    }
    IEnumerator ChangeSpeed(float time)
    {
      yield return new WaitForSeconds(time);
        // Code to execute after the delay
        QuestionData questionData = questionPool [questionIndex];
        questionAudioSource.Stop();

        questionAudioSource.pitch = (float)soundSpeed; //set the pitch to soundSpeed
        //Pitch Correct and get clip new length
        questionAudioSource.outputAudioMixerGroup.audioMixer.SetFloat("pitchParam", 1f / soundSpeed);
        ClipLengthNew = (questionAudioSource.clip.length - questionData.timeUntilImportant) / soundSpeed; //the cliplength of the important part
        ClipLengthTotal = ClipLengthNew + questionData.timeUntilImportant;
        Debug.Log("Clip:" + ClipLengthTotal + " Speed:" + questionAudioSource.pitch);

        questionAudioSource.time = questionData.timeUntilImportant;
        questionAudioSource.Play();
        if (showAnswers == true){
          StartCoroutine(AssignAnswerButtons(ClipLengthNew));
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

            AnswerButtonChallenge answerButton = answerButtonGameObject.GetComponent<AnswerButtonChallenge>();
            answerButton.Setup(answersInRandomOrder[i]);
        }
        //The question is done now, show button and play bg music
        questionAudioSource.pitch = 1.0f; //set the pitch back to one after the sound is done
        questionAudioSource.time = 0;
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
      questionAudioSource.time = 0;
      //undo pitch correcting
      questionAudioSource.outputAudioMixerGroup.audioMixer.SetFloat("pitchParam", 1f / 1);
      questionAudioSource.pitch = 1;
      questionAudioSource.Play();

      QuestionData questionData = questionPool [questionIndex];
      showAnswers = false;
      StartCoroutine(ChangeSpeed((float)questionData.timeUntilImportant));
      StartCoroutine(ReplayButton(ClipLengthTotal));
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
            soundSpeed += (float)0.25;
        } else {
          //they got it wrong, log what they're wrong score is and the speed they got it wrong at. slow down the audio, too
          wrongScoreInt ++;
          wrongScore.Add(wrongScoreInt,questionAudioSource.pitch);
          soundSpeed -= (float)0.25;
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

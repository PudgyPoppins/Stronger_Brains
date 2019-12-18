using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AnswerButtonChallenge : MonoBehaviour {

    public Text answerText;

    private AnswerData answerData;
    private ChallengeGameController gameController;

    // Use this for initialization
    void Start ()
    {
        gameController = FindObjectOfType<ChallengeGameController> ();
    }

    public void Setup(AnswerData data)
    {
        answerData = data;
        answerText.text = answerData.answerText;
    }

    public void HandleClick()
    {
        gameController.AnswerButtonClicked (answerData.isCorrect);
    }
}

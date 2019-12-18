using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text word;
    public bool same;
    Color[] colors = {Color.green,Color.red, Color.blue, Color.magenta, Color.yellow, Color.black};
    string[] colorNames = new string[] {"Green", "Red", "Blue", "Pink", "Yellow", "Black"};

    public Text timeRemainingDisplayText;
    public Text scoreDisplayText;
    private float timeRemaining;
    private int playerScore;
    private int rightScore;

    public List<int> previousIndexes;

    private int wordsShown;
    public List<float> reactionTimes;
    private float reactionTime;
    public Text ReactionTimeText;

    public GameObject FinishPanel;

    // Start is called before the first frame update
    void Start()
    {
      previousIndexes.Add(-1); previousIndexes.Add(-1);

      wordsShown = -1; //-1 because it'll show x amount, then quit. I want it to show it, let you get a score, then quit
      rightScore = 0;
      //change time depending on round type
      if (FormController.LimitedTime == true){
        timeRemaining = FormController.totalTime;
      } else{
        timeRemaining = FormController.timeBetween;
      }
      nextWord();
    }

    private void scoreUpdate(bool right, float rt)
    {
      wordsShown += 1; //if there's a score update, a word was just shown
      reactionTime = 0;

      reactionTimes.Add(rt);
      if(right){playerScore+=1; rightScore +=1;}
      else{playerScore-=1;}
      float percentage = ((float)rightScore/((float)wordsShown + 1f)) * 100f;
      scoreDisplayText.text = "Score: " + playerScore.ToString() + " (" + percentage.ToString("F2") + "%" + ")";

      if (!(FormController.LimitedTime)){ timeRemaining = FormController.timeBetween; }//reset the time if needed

      nextWord();

    }

    private void nextWord()
    {
      if (Random.value < .60){//there is a 60% chance that the two values won't be the same
        same = false;
        int nameIndex = 0; int colorsIndex = 0;
        while (nameIndex == colorsIndex){
          nameIndex = Random.Range (0, colorNames.Length);
          colorsIndex = Random.Range (0, colors.Length);
          while(previousIndexes[0] == nameIndex && previousIndexes[1] == colorsIndex){//if the two exact values have both been used last time
            nameIndex = Random.Range (0, colorNames.Length);
            colorsIndex = Random.Range (0, colors.Length);
          }
        }


        word.text = colorNames[nameIndex];
        word.color = colors[colorsIndex];

        previousIndexes.Clear();
        previousIndexes.Add(nameIndex); previousIndexes.Add(colorsIndex);
      } else{//the two values are the same
        same = true;
        int sameIndex = Random.Range (0, colorNames.Length);

        while(previousIndexes[0] == sameIndex && previousIndexes[1] == sameIndex){//if it hasn't been used before, it'll keep these values
          sameIndex = Random.Range (0, colorNames.Length);
        }


        word.text = colorNames[sameIndex];
        word.color = colors[sameIndex];

        previousIndexes.Clear();
        previousIndexes.Add(sameIndex); previousIndexes.Add(sameIndex);
      }
    }

    private void Answer(bool userSaidSame)
    {
      if((userSaidSame && same) || (!userSaidSame && !same)){ scoreUpdate(true, reactionTime);} //user was right
      else { scoreUpdate(false, reactionTime);}
    }

    private void UpdateTimeRemainingDisplay(string s)
    {
        timeRemainingDisplayText.text = s + ": " + Mathf.Round (timeRemaining).ToString ();
    }
    // Update is called once per frame
    void Update ()
    {
      reactionTime += Time.deltaTime;
      timeRemaining -= Time.deltaTime;
      if(FormController.LimitedTime){UpdateTimeRemainingDisplay("Time");}
      else {UpdateTimeRemainingDisplay("Time Remaining");}

      if (timeRemaining <= 0f && FormController.LimitedTime || (wordsShown == FormController.maxWords && !(FormController.LimitedTime))){
        FinishPanel.SetActive(true);//end round if time is up, or if all the words are done
        string rtString = "";
        for (var i = 0; i < (reactionTimes.Count - 1); i++){rtString += reactionTimes[i].ToString("F2") + "s, ";}
        rtString += "wow! You completed " + (wordsShown - 1).ToString() + " words!";
        ReactionTimeText.text = "Your reaction times: " + rtString;
      }

      else if(timeRemaining <= 0f && !(FormController.LimitedTime)){//next word if time remaining is up, and reset time.
        scoreUpdate(false, FormController.timeBetween);//time ran out, count it as wrong
      }

      if (Input.GetKeyDown(".")){Answer(true);}//the user said it was the same value
      else if(Input.GetKeyDown("z")){Answer(false);} //user said not same value

    }
}

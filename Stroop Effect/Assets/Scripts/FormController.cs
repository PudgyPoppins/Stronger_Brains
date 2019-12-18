using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class FormController : MonoBehaviour
{
   public Toggle toggle;
   public GameObject panel1;
   public GameObject panel2;

   private InputField TimeInput;
   private InputField MaxWordInput;
   private InputField TimeBetweenInput;
   private InputField MaxRepWordsInput;

   public static bool LimitedTime;

   public static int totalTime;
   public static int maxWords;
   public static int maxRepWords;
   public static int timeBetween;

   public string Jeff;

    void Start()
    {
      DontDestroyOnLoad(this);

      LimitedTime = true;
      toggle.onValueChanged.AddListener(delegate {
          ToggleValueChanged(toggle);
      });

      TimeInput = GameObject.Find("Panel 1/TimeInput").GetComponent<InputField>();
      MaxWordInput = GameObject.Find("Panel 2/MaxWordInput").GetComponent<InputField>();
      TimeBetweenInput = GameObject.Find("Panel 2/TimeBetweenInput").GetComponent<InputField>();
      MaxRepWordsInput = GameObject.Find("Panel 2/MaxWordDispInput").GetComponent<InputField>();
    }

    void ToggleValueChanged(Toggle change) //hide and show the two UI panels
    {
      if(toggle.isOn){
        panel1.transform.localScale = new Vector3(1,1,1);
        panel2.transform.localScale = new Vector3(0,0,0);
        LimitedTime = true;
      } else {
        panel2.transform.localScale = new Vector3(1,1,1);
        panel1.transform.localScale = new Vector3(0,0,0);
        LimitedTime = false;
      }
    }

    public int GetInputContent(InputField input, int numval){
      if(input.text.ToString().Length > 0){
        numval = int.Parse(input.text);
      } else {
        numval = int.Parse(input.placeholder.GetComponent<Text>().text);
      }
      return numval;
    }

    //get all of these integers set from their respective input fields, if it's blank use the default placeholder values
    public void onSubmit(){
      totalTime = GetInputContent(TimeInput, totalTime);
      maxWords = GetInputContent(MaxWordInput, maxWords);
      timeBetween = GetInputContent(TimeBetweenInput, timeBetween);
      maxRepWords = GetInputContent(MaxRepWordsInput, maxRepWords);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonPress( string strPar )
    {
        Debug.Log("Pressed");
        if (strPar == "easy")
        {
            SceneManager.LoadScene("_Scene_Easy");
        }
        else if (strPar == "medium")
        {
            SceneManager.LoadScene("_Scene_Medium");
        }
        else if (strPar == "hard")
        {
            SceneManager.LoadScene("_Scene_Hard");
        }
    }

    void MediumButtonClick()
    {

    }

    void HardButtonClick()
    {

    }
}

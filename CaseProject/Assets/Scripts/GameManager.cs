using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<CarCountainer> CarParents = new List<CarCountainer>();

    [SerializeField] GameObject NextPanel;

    [SerializeField] TextMeshProUGUI FinishTotalPassengerText;
    [SerializeField] TextMeshProUGUI FinishTotalSecondText;
    [SerializeField] TextMeshProUGUI TotalPassengerText;

    bool _isGameFinished;
    float timer;
    int value;

    private void Awake()
    {
        Instance = this;
    }

    public void TextUpdate(int val)
    {
        value += val;
        TotalPassengerText.text = "TotalPassengers: " + value.ToString();
    }

    private void Update()
    {
        if (_isGameFinished)
            return;

        timer += Time.deltaTime;

        if (CarParents.Count <= 0)
        {
            _isGameFinished = true;
            CancelInvoke("FinishInvoke");
            Invoke("FinishInvoke", 1f);
        }
    }

    void FinishInvoke()
    {
        NextPanel.gameObject.SetActive(true);
        FinishTotalSecondText.text = Mathf.FloorToInt(timer).ToString();
        FinishTotalPassengerText.text = value.ToString();
    }

    public void LoadNextLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        string prefix = "Level";
        if (currentSceneName.StartsWith(prefix))
        {
            string numberPart = currentSceneName.Substring(prefix.Length);
            if (int.TryParse(numberPart, out int currentLevelNumber))
            {
                int nextLevelNumber = currentLevelNumber + 1;
                string nextSceneName = prefix + nextLevelNumber;

                if (Application.CanStreamedLevelBeLoaded(nextSceneName))
                    SceneManager.LoadScene(nextSceneName);
                else
                    SceneManager.LoadScene(prefix + "1");
            }
        }
    }
}

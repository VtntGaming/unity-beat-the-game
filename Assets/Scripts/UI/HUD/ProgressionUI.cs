using System;
using TMPro;
using UnityEngine;

public class ProgressionUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    TMP_Text LevelTxt;
    RectTransform progressBar;
    PlayerProgression currentPlayerProgresson;

    // Stored handler so we can unsubscribe later
    private Action<float> xpAddedHandler;

    private void Awake()
    {
        LevelTxt = transform.Find("Level").Find("LevelDisplay").GetComponent<TMP_Text>();
        progressBar = transform.Find("XPBar").Find("Progress").GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        listenToProgressionEvents();
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid leaks
        if (currentPlayerProgresson != null && xpAddedHandler != null)
            currentPlayerProgresson.XPAdded -= xpAddedHandler;

        xpAddedHandler = null;
        currentPlayerProgresson = null;
    }

    void updateProgressionUI()
    {
        PlayerProgression progression = getProgression();
        if (progression != null)
        {
            int currentLevel = progression.GetLevel();  // Get current level
            float levelProgression = progression.GetLevelProgress();    // Progression ratio to the next level

            LevelTxt.SetText(currentLevel.ToString());
            progressBar.localScale = new Vector3(levelProgression, 1, 1);
        }else
        {
            Debug.Log("Failed to update the ui, progression could not be found");
        }
    }

    void listenToProgressionEvents()
    {
        PlayerProgression progression = getProgression();

        // Unsubscribe previous
        if (currentPlayerProgresson != null && xpAddedHandler != null)
            currentPlayerProgresson.XPAdded -= xpAddedHandler;

        currentPlayerProgresson = progression;

        if (progression != null)
        {
            // store handler so we can unsubscribe later
            xpAddedHandler = (amt) => updateProgressionUI();
            progression.XPAdded += xpAddedHandler;

            // Update UI immediately to reflect current state
            updateProgressionUI();
        }
    }


    PlayerProgression getProgression()
    {
        PlayerStats stats = GameObject.FindAnyObjectByType<PlayerStats>();
        if (stats != null)
            return stats.plrProgression;

        return null;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

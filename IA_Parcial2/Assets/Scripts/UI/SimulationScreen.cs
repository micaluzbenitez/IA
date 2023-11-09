using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimulationScreen : MonoBehaviour
{
    public GameObject verticalLayout = null;
    public TMP_Text generationsCountTxt = null;
    public TMP_Text turnsLeftTxt = null;
    public TMP_Text bestFitnessTxt = null;
    public TMP_Text avgFitnessTxt = null;
    public TMP_Text worstFitnessTxt = null;
    public TMP_Text timerTxt = null;
    public Slider timerSlider = null;

    [Header("Gameplay")]
    public TMP_Text totalAgentsTxt = null;
    public TMP_Text totalFoodsTxt = null;
    public TMP_Text totalDeathsTxt = null;
    public TMP_Text agentsATxt = null;
    public TMP_Text foodsATxt = null;
    public TMP_Text deathsATxt = null;
    public TMP_Text extinctsATxt = null;
    public TMP_Text agentsBTxt = null;
    public TMP_Text foodsBTxt = null;
    public TMP_Text deathsBTxt = null;
    public TMP_Text extinctsBTxt = null;

    [Header("Buttons")]
    public Button pauseBtn = null;
    public Button saveBtn = null;
    public Button stopBtn = null;

    string generationsCountText = "";
    string turnsLeftText = "";
    string bestFitnessText = "";
    string avgFitnessText = "";
    string worstFitnessText = "";
    string timerText = "";

    string totalAgentsText = "";
    string totalFoodsText = "";
    string totalDeathsText = "";
    string agentsAText = "";
    string foodsAText = "";
    string deathsAText = "";
    string extinctsAText = "";
    string agentsBText = "";
    string foodsBText = "";
    string deathsBText = "";
    string extinctsBText = "";

    public void Init(Action onPauseGame, Action onStopSimulation)
    {
        timerSlider.onValueChanged.AddListener(OnTimerChange);
        timerText = timerTxt.text;

        timerTxt.text = string.Format(timerText, PopulationManager.Instance.IterationCount);

        if (string.IsNullOrEmpty(generationsCountText)) generationsCountText = generationsCountTxt.text;
        if (string.IsNullOrEmpty(turnsLeftText)) turnsLeftText = turnsLeftTxt.text;
        if (string.IsNullOrEmpty(bestFitnessText)) bestFitnessText = bestFitnessTxt.text;
        if (string.IsNullOrEmpty(avgFitnessText)) avgFitnessText = avgFitnessTxt.text;
        if (string.IsNullOrEmpty(worstFitnessText)) worstFitnessText = worstFitnessTxt.text;

        if (string.IsNullOrEmpty(totalAgentsText)) totalAgentsText = totalAgentsTxt.text;
        if (string.IsNullOrEmpty(totalFoodsText)) totalFoodsText = totalFoodsTxt.text;
        if (string.IsNullOrEmpty(totalDeathsText)) totalDeathsText = totalDeathsTxt.text;
        if (string.IsNullOrEmpty(agentsAText)) agentsAText = agentsATxt.text;
        if (string.IsNullOrEmpty(foodsAText)) foodsAText = foodsATxt.text;
        if (string.IsNullOrEmpty(deathsAText)) deathsAText = deathsATxt.text;
        if (string.IsNullOrEmpty(extinctsAText)) extinctsAText = extinctsATxt.text;
        if (string.IsNullOrEmpty(agentsBText)) agentsBText = agentsBTxt.text;
        if (string.IsNullOrEmpty(foodsBText)) foodsBText = foodsBTxt.text;
        if (string.IsNullOrEmpty(deathsBText)) deathsBText = deathsBTxt.text;
        if (string.IsNullOrEmpty(extinctsBText)) extinctsBText = extinctsBTxt.text;

        pauseBtn.onClick.AddListener(() => onPauseGame?.Invoke());
        saveBtn.onClick.AddListener(() => PopulationManager.Instance.SaveData());
        stopBtn.onClick.AddListener(() => onStopSimulation?.Invoke());
    }

    void OnTimerChange(float value)
    {
        PopulationManager.Instance.IterationCount = (int)value;
        timerTxt.text = string.Format(timerText, PopulationManager.Instance.IterationCount);
    }

    public void Active(bool status)
    {
        verticalLayout.SetActive(status);
    }

    void LateUpdate()
    {
        timerTxt.text = string.Format(timerText, PopulationManager.Instance.IterationCount);
        generationsCountTxt.text = string.Format(generationsCountText, PopulationManager.Instance.generation);
        turnsLeftTxt.text = string.Format(turnsLeftText, PopulationManager.Instance.turnsLeft);
        bestFitnessTxt.text = string.Format(bestFitnessText, PopulationManager.Instance.bestFitness);
        avgFitnessTxt.text = string.Format(avgFitnessText, PopulationManager.Instance.avgFitness);
        worstFitnessTxt.text = string.Format(worstFitnessText, PopulationManager.Instance.worstFitness);

        totalAgentsTxt.text = string.Format(totalAgentsText, PopulationManager.Instance.totalAgents);
        totalFoodsTxt.text = string.Format(totalFoodsText, PopulationManager.Instance.totalFoodsConsumed);
        totalDeathsTxt.text = string.Format(totalDeathsText, PopulationManager.Instance.totalDeaths);
        agentsATxt.text = string.Format(agentsAText, PopulationManager.Instance.teams[0].agents);
        foodsATxt.text = string.Format(foodsAText, PopulationManager.Instance.teams[0].foods);
        deathsATxt.text = string.Format(deathsAText, PopulationManager.Instance.teams[0].deaths);
        extinctsATxt.text = string.Format(extinctsAText, PopulationManager.Instance.teams[0].extincts);
        agentsBTxt.text = string.Format(agentsBText, PopulationManager.Instance.teams[1].agents);
        foodsBTxt.text = string.Format(foodsBText, PopulationManager.Instance.teams[1].foods);
        deathsBTxt.text = string.Format(deathsBText, PopulationManager.Instance.teams[1].deaths);
        extinctsBTxt.text = string.Format(extinctsBText, PopulationManager.Instance.teams[1].extincts);
    }
}


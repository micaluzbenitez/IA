using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartConfigurationScreen : MonoBehaviour
{
    [Serializable]
    public class TeamsUI
    {
        public string Tag = "";
        public TMP_Text eliteCountTxt = null;
        public Slider eliteCountSlider = null;
        public TMP_Text mutationChanceTxt = null;
        public Slider mutationChanceSlider = null;
        public TMP_Text mutationRateTxt = null;
        public Slider mutationRateSlider = null;
        public TMP_Text hiddenLayersCountTxt = null;
        public Slider hiddenLayersCountSlider = null;
        public TMP_Text neuronsPerHLCountTxt = null;
        public Slider neuronsPerHLSlider = null;
        public TMP_Text biasTxt = null;
        public Slider biasSlider = null;
        public TMP_Text sigmoidSlopeTxt = null;
        public Slider sigmoidSlopeSlider = null;

        [HideInInspector] public string elitesText = "";
        [HideInInspector] public string mutationChanceText = "";
        [HideInInspector] public string mutationRateText = "";
        [HideInInspector] public string hiddenLayersCountText = "";
        [HideInInspector] public string biasText = "";
        [HideInInspector] public string sigmoidSlopeText = "";
        [HideInInspector] public string neuronsPerHLCountText = "";
    }

    public GameObject verticalLayout = null;
    public TMP_Text populationCountTxt = null;
    public Slider populationCountSlider = null;
    public TMP_Text turnsTxt = null;
    public Slider turnsSlider = null;
    public TMP_Text agentMaxGenerationTxt = null;
    public Slider agentMaxGenerationSlider = null; 
    public TMP_Text deathChanceTxt = null;
    public Slider deathChanceSlider = null;
    public TMP_Text plusMutationRateTxt = null;
    public Slider plusMutationRateSlider = null;

    [Header("Teams")]
    public TeamsUI[] teamsUI = null;

    [Header("Buttons")]
    [SerializeField] private Button startButton = null;
    [SerializeField] private Button loadButton = null;

    string populationText = "";
    string turnsText = "";
    string agentMaxGenerationText = "";
    string deathChanceText = "";
    string plusMutationRateText = "";

    public void Init(Action<bool> onStartGame)
    {
        populationCountSlider.onValueChanged.AddListener(OnPopulationCountChange);
        turnsSlider.onValueChanged.AddListener(OnTurnsChange);
        agentMaxGenerationSlider.onValueChanged.AddListener(OnAgentMaxGenerationChange);
        deathChanceSlider.onValueChanged.AddListener(OnDeathChanceAChange);
        plusMutationRateSlider.onValueChanged.AddListener(OnPlusMutationRateChange);

        teamsUI[0].eliteCountSlider.onValueChanged.AddListener(OnEliteCountAChange);
        teamsUI[0].mutationChanceSlider.onValueChanged.AddListener(OnMutationChanceAChange);
        teamsUI[0].mutationRateSlider.onValueChanged.AddListener(OnMutationRateAChange);
        teamsUI[0].hiddenLayersCountSlider.onValueChanged.AddListener(OnHiddenLayersCountAChange);
        teamsUI[0].neuronsPerHLSlider.onValueChanged.AddListener(OnNeuronsPerHLAChange);
        teamsUI[0].biasSlider.onValueChanged.AddListener(OnBiasAChange);
        teamsUI[0].sigmoidSlopeSlider.onValueChanged.AddListener(OnSigmoidSlopeAChange);

        teamsUI[1].eliteCountSlider.onValueChanged.AddListener(OnEliteCountBChange);
        teamsUI[1].mutationChanceSlider.onValueChanged.AddListener(OnMutationChanceBChange);
        teamsUI[1].mutationRateSlider.onValueChanged.AddListener(OnMutationRateBChange);
        teamsUI[1].hiddenLayersCountSlider.onValueChanged.AddListener(OnHiddenLayersCountBChange);
        teamsUI[1].neuronsPerHLSlider.onValueChanged.AddListener(OnNeuronsPerHLBChange);
        teamsUI[1].biasSlider.onValueChanged.AddListener(OnBiasBChange);
        teamsUI[1].sigmoidSlopeSlider.onValueChanged.AddListener(OnSigmoidSlopeBChange);

        populationText = populationCountTxt.text;
        turnsText = turnsTxt.text;
        agentMaxGenerationText = agentMaxGenerationTxt.text;
        deathChanceText = deathChanceTxt.text;
        plusMutationRateText = plusMutationRateTxt.text;

        teamsUI[0].elitesText = teamsUI[0].eliteCountTxt.text;
        teamsUI[0].mutationChanceText = teamsUI[0].mutationChanceTxt.text;
        teamsUI[0].mutationRateText = teamsUI[0].mutationRateTxt.text;
        teamsUI[0].hiddenLayersCountText = teamsUI[0].hiddenLayersCountTxt.text;
        teamsUI[0].neuronsPerHLCountText = teamsUI[0].neuronsPerHLCountTxt.text;
        teamsUI[0].biasText = teamsUI[0].biasTxt.text;
        teamsUI[0].sigmoidSlopeText = teamsUI[0].sigmoidSlopeTxt.text;

        teamsUI[1].elitesText = teamsUI[1].eliteCountTxt.text;
        teamsUI[1].mutationChanceText = teamsUI[1].mutationChanceTxt.text;
        teamsUI[1].mutationRateText = teamsUI[1].mutationRateTxt.text;
        teamsUI[1].hiddenLayersCountText = teamsUI[1].hiddenLayersCountTxt.text;
        teamsUI[1].neuronsPerHLCountText = teamsUI[1].neuronsPerHLCountTxt.text;
        teamsUI[1].biasText = teamsUI[1].biasTxt.text;
        teamsUI[1].sigmoidSlopeText = teamsUI[1].sigmoidSlopeTxt.text;

        populationCountSlider.value = PopulationManager.Instance.PopulationCount;
        turnsSlider.value = PopulationManager.Instance.Turns;
        agentMaxGenerationSlider.value = PopulationManager.Instance.AgentMaxGeneration;

        teamsUI[0].eliteCountSlider.value = PopulationManager.Instance.teams[0].EliteCount;
        teamsUI[0].mutationChanceSlider.value = PopulationManager.Instance.teams[0].MutationChance * 100.0f;
        teamsUI[0].mutationRateSlider.value = PopulationManager.Instance.teams[0].MutationRate * 100.0f;
        teamsUI[0].hiddenLayersCountSlider.value = PopulationManager.Instance.teams[0].HiddenLayers;
        teamsUI[0].neuronsPerHLSlider.value = PopulationManager.Instance.teams[0].NeuronsCountPerHL;
        teamsUI[0].biasSlider.value = PopulationManager.Instance.teams[0].Bias;
        teamsUI[0].sigmoidSlopeSlider.value = PopulationManager.Instance.teams[0].P;

        teamsUI[1].eliteCountSlider.value = PopulationManager.Instance.teams[1].EliteCount;
        teamsUI[1].mutationChanceSlider.value = PopulationManager.Instance.teams[1].MutationChance * 100.0f;
        teamsUI[1].mutationRateSlider.value = PopulationManager.Instance.teams[1].MutationRate * 100.0f;
        teamsUI[1].hiddenLayersCountSlider.value = PopulationManager.Instance.teams[1].HiddenLayers;
        teamsUI[1].neuronsPerHLSlider.value = PopulationManager.Instance.teams[1].NeuronsCountPerHL;
        teamsUI[1].biasSlider.value = PopulationManager.Instance.teams[1].Bias;
        teamsUI[1].sigmoidSlopeSlider.value = PopulationManager.Instance.teams[1].P;

        OnPopulationCountChange(populationCountSlider.value);
        OnTurnsChange(turnsSlider.value);
        OnAgentMaxGenerationChange(agentMaxGenerationSlider.value);
        OnDeathChanceAChange(deathChanceSlider.value);
        OnPlusMutationRateChange(plusMutationRateSlider.value);

        OnEliteCountAChange(teamsUI[0].eliteCountSlider.value);
        OnMutationChanceAChange(teamsUI[0].mutationChanceSlider.value);
        OnMutationRateAChange(teamsUI[0].mutationRateSlider.value);
        OnHiddenLayersCountAChange(teamsUI[0].hiddenLayersCountSlider.value);
        OnNeuronsPerHLAChange(teamsUI[0].neuronsPerHLSlider.value);
        OnBiasAChange(teamsUI[0].biasSlider.value);
        OnSigmoidSlopeAChange(teamsUI[0].sigmoidSlopeSlider.value);

        OnEliteCountBChange(teamsUI[1].eliteCountSlider.value);
        OnMutationChanceBChange(teamsUI[1].mutationChanceSlider.value);
        OnMutationRateBChange(teamsUI[1].mutationRateSlider.value);
        OnHiddenLayersCountBChange(teamsUI[1].hiddenLayersCountSlider.value);
        OnNeuronsPerHLBChange(teamsUI[1].neuronsPerHLSlider.value);
        OnBiasBChange(teamsUI[1].biasSlider.value);
        OnSigmoidSlopeBChange(teamsUI[1].sigmoidSlopeSlider.value);

        startButton.onClick.AddListener(() => { onStartGame?.Invoke(false); });
        loadButton.onClick.AddListener(() => PopulationManager.Instance.LoadData(onStartGame));
    }

    void OnPopulationCountChange(float value)
    {
        PopulationManager.Instance.PopulationCount = (int)value;
        populationCountTxt.text = string.Format(populationText, PopulationManager.Instance.PopulationCount);
    }

    void OnTurnsChange(float value)
    {
        PopulationManager.Instance.Turns = (int)value;
        turnsTxt.text = string.Format(turnsText, PopulationManager.Instance.Turns);
    }

    void OnAgentMaxGenerationChange(float value)
    {
        PopulationManager.Instance.AgentMaxGeneration = (int)value;
        agentMaxGenerationTxt.text = string.Format(agentMaxGenerationText, PopulationManager.Instance.AgentMaxGeneration);
    }

    void OnDeathChanceAChange(float value)
    {
        PopulationManager.Instance.DeathChance = value / 100.0f;
        deathChanceTxt.text = string.Format(deathChanceText, (int)(PopulationManager.Instance.DeathChance * 100));
    }

    void OnPlusMutationRateChange(float value)
    {
        PopulationManager.Instance.PlusMutationRate = value / 10.0f;
        plusMutationRateTxt.text = string.Format(plusMutationRateText, (int)(PopulationManager.Instance.PlusMutationRate * 10));
    }

    void OnEliteCountAChange(float value)
    {
        PopulationManager.Instance.teams[0].EliteCount = (int)value;
        teamsUI[0].eliteCountTxt.text = string.Format(teamsUI[0].elitesText, PopulationManager.Instance.teams[0].EliteCount);
    }

    void OnMutationChanceAChange(float value)
    {
        PopulationManager.Instance.teams[0].MutationChance = value / 100.0f;
        teamsUI[0].mutationChanceTxt.text = string.Format(teamsUI[0].mutationChanceText, (int)(PopulationManager.Instance.teams[0].MutationChance * 100));
    }

    void OnMutationRateAChange(float value)
    {
        PopulationManager.Instance.teams[0].MutationRate = value / 100.0f;
        teamsUI[0].mutationRateTxt.text = string.Format(teamsUI[0].mutationRateText, (int)(PopulationManager.Instance.teams[0].MutationRate * 100));
    }

    void OnHiddenLayersCountAChange(float value)
    {
        PopulationManager.Instance.teams[0].HiddenLayers = (int)value;
        teamsUI[0].hiddenLayersCountTxt.text = string.Format(teamsUI[0].hiddenLayersCountText, PopulationManager.Instance.teams[0].HiddenLayers);
    }

    void OnNeuronsPerHLAChange(float value)
    {
        PopulationManager.Instance.teams[0].NeuronsCountPerHL = (int)value;
        teamsUI[0].neuronsPerHLCountTxt.text = string.Format(teamsUI[0].neuronsPerHLCountText, PopulationManager.Instance.teams[0].NeuronsCountPerHL);
    }

    void OnBiasAChange(float value)
    {
        PopulationManager.Instance.teams[0].Bias = value;
        teamsUI[0].biasTxt.text = string.Format(teamsUI[0].biasText, PopulationManager.Instance.teams[0].Bias.ToString("0.00"));
    }

    void OnSigmoidSlopeAChange(float value)
    {
        PopulationManager.Instance.teams[0].P = value;
        teamsUI[0].sigmoidSlopeTxt.text = string.Format(teamsUI[0].sigmoidSlopeText, PopulationManager.Instance.teams[0].P.ToString("0.00"));
    }

    void OnEliteCountBChange(float value)
    {
        PopulationManager.Instance.teams[1].EliteCount = (int)value;
        teamsUI[1].eliteCountTxt.text = string.Format(teamsUI[1].elitesText, PopulationManager.Instance.teams[1].EliteCount);
    }

    void OnMutationChanceBChange(float value)
    {
        PopulationManager.Instance.teams[1].MutationChance = value / 100.0f;
        teamsUI[1].mutationChanceTxt.text = string.Format(teamsUI[1].mutationChanceText, (int)(PopulationManager.Instance.teams[1].MutationChance * 100));
    }

    void OnMutationRateBChange(float value)
    {
        PopulationManager.Instance.teams[1].MutationRate = value / 100.0f;
        teamsUI[1].mutationRateTxt.text = string.Format(teamsUI[1].mutationRateText, (int)(PopulationManager.Instance.teams[1].MutationRate * 100));
    }

    void OnHiddenLayersCountBChange(float value)
    {
        PopulationManager.Instance.teams[1].HiddenLayers = (int)value;
        teamsUI[1].hiddenLayersCountTxt.text = string.Format(teamsUI[1].hiddenLayersCountText, PopulationManager.Instance.teams[1].HiddenLayers);
    }

    void OnNeuronsPerHLBChange(float value)
    {
        PopulationManager.Instance.teams[1].NeuronsCountPerHL = (int)value;
        teamsUI[1].neuronsPerHLCountTxt.text = string.Format(teamsUI[1].neuronsPerHLCountText, PopulationManager.Instance.teams[1].NeuronsCountPerHL);
    }

    void OnBiasBChange(float value)
    {
        PopulationManager.Instance.teams[1].Bias = value;
        teamsUI[1].biasTxt.text = string.Format(teamsUI[1].biasText, PopulationManager.Instance.teams[1].Bias.ToString("0.00"));
    }

    void OnSigmoidSlopeBChange(float value)
    {
        PopulationManager.Instance.teams[1].P = value;
        teamsUI[1].sigmoidSlopeTxt.text = string.Format(teamsUI[1].sigmoidSlopeText, PopulationManager.Instance.teams[1].P.ToString("0.00"));
    }

    public void Active(bool status)
    {
        verticalLayout.SetActive(status);
    }
}
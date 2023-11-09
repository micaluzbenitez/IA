using UnityEngine;

public class Agent : AgentBase
{
    [Header("Fitness Settings")]
    public float consumeNearFoodFitness = 0f;
    public float consumeFoodFitness = 0f;
    public float outLimitYFitness = 0f;

    Vector3 startPosition = Vector3.zero;
    Vector3 movePosition = Vector3.zero;
    Vector2Int moveIndex = Vector2Int.zero;
    Vector2Int index = Vector2Int.zero;

    float unit = 0f;
    bool dead = false;
    bool toStay = false;
    int generationCount = 0;
    Food nearFood = null;
    int foodsConsumed = 0;
    float limitX = 0f;
    int maxIndex = 0;

    public Vector2Int Index { get => index; set => index = value; }
    public int GenerationCount { get => generationCount; set => generationCount = value; }
    public bool ToStay { get => toStay; set => toStay = value; }
    public int FoodsConsumed { get => foodsConsumed; }
    public TEAM Team { get => team; }
    public bool Dead { get => dead; }

    public void Init(float unit, int size, TEAM team)
    {
        this.unit = unit;
        this.team = team;

        limitX = size / 2f * unit;
        maxIndex = size;

        generationCount = 0;
    }

    public void Move(float lerp)
    {
        if (dead || toStay) return;

        transform.position = Vector3.Lerp(startPosition, movePosition, lerp);
    }

    public void SetNearFood(Food nearFood)
    {
        this.nearFood = nearFood;
    }

    public void Think()
    {
        OnThink();
    }

    public void ResetData()
    {
        moveIndex = index;

        startPosition = transform.position;
        movePosition = transform.position;

        generationCount++;
        toStay = false;

        OnReset();
    }

    public void ConsumeFood()
    {
        UpdateFitness(nearFood != null && index == nearFood.Index ? consumeNearFoodFitness : consumeFoodFitness);
        foodsConsumed++;
        PopulationManager.Instance.AddFoodsConsumed(team);
    }

    protected override void ProcessInputs()
    {
        if (nearFood != null)
        {
            Vector3 foodPosition = nearFood.transform.position;
            Vector3 foodDirection = GetDirToFood(foodPosition);

            inputs[0] = foodPosition.x;
            inputs[1] = foodPosition.z;
            inputs[2] = foodDirection.x;
            inputs[3] = foodDirection.z;
        }
    }

    protected override void ProcessOutputs(float[] outputs)
    {
        if (!toStay)
        {
            transform.position = movePosition;
            startPosition = transform.position;
            index = moveIndex;
        }

        if (outputs != null && outputs.Length >= 3)
        {
            bool vertical = outputs[0] < 0.5f;
            float positive = outputs[1] < 0.5f ? -1f : 1f;

            Vector3 dir = new Vector3(vertical ? positive : 0f, 0f, !vertical ? positive : 0f);
            movePosition = transform.position + dir * unit;
            transform.forward = dir;

            moveIndex = index + new Vector2Int((int)dir.x, (int)dir.z);

            toStay = outputs[2] < 0.5f;

            UpdatePositionLimit();
            UpdateIndexLimit();

            if (moveIndex.y < 0 || moveIndex.y > maxIndex) // Check limit Y
            {
                UpdateFitness(outLimitYFitness);
            }
        }
    }

    protected override void OnReset()
    {
        base.OnReset();
        foodsConsumed = 0;
    }

    private Vector3 GetDirToFood(Vector3 foodPosition)
    {
        return (foodPosition - transform.position).normalized;
    }

    private void UpdatePositionLimit()
    {
        Vector3 pos = movePosition;

        if (pos.x > limitX)
        {
            pos.x -= limitX * 2;
            startPosition = pos - new Vector3(unit, 0f, 0f);
        }
        else if (pos.x < -limitX)
        {
            pos.x += limitX * 2;
            startPosition = pos + new Vector3(unit, 0f, 0f);
        }

        movePosition = pos;
    }

    private void UpdateIndexLimit()
    {
        Vector2Int auxIndex = moveIndex;

        if (auxIndex.x > maxIndex) auxIndex.x = 0;
        else if (auxIndex.x < 0) auxIndex.x = maxIndex;

        moveIndex = auxIndex;
    }

    public void Death()
    {
        dead = true;
        PopulationManager.Instance.AddDeaths(team);
    }
}
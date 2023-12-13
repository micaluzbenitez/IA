using UnityEngine;

public enum STATES
{
    WALK,
    EAT
}

public class Agent : AgentBase
{
    [Header("Fitness Settings")]
    public float consumeNearFoodFitness = 0f;
    public float consumeFoodFitness = 0f;
    public float outLimitYFitness = 0f;

    Vector2Int index = Vector2Int.zero;
    Vector2Int moveIndex = Vector2Int.zero;
    Vector3 startPosition = Vector3.zero;
    Vector3 movePosition = Vector3.zero;

    float unit = 0f;
    bool dead = false;
    bool toStay = false;
    bool inOutLimit = false;
    int steps = 0;
    int generationCount = 0;

    Food nearFood = null;
    int foodsConsumed = 0;

    float limitX = 0f;
    int maxIndex = 0;

    public Vector2Int Index { get => index; set => index = value; }
    public bool ToStay { get => toStay; set => toStay = value; }
    public bool InOutLimit { get => inOutLimit; set => inOutLimit = value; }
    public int GenerationCount { get => generationCount; set => generationCount = value; }
    public bool Dead { get => dead; set => dead = value; }
    public TEAM Team { get => team; }
    public int FoodsConsumed { get => foodsConsumed; set => foodsConsumed = value; }


    public STATES currentState = STATES.WALK;

    public void Init(float unit, int size, TEAM team)
    {
        this.unit = unit;
        this.team = team;

        limitX = size / 2f * unit;
        maxIndex = size;

        generationCount = 1;
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

        foodsConsumed = 0;
        toStay = false;
        inOutLimit = false;
        steps = 0;

        OnReset();
    }

    public void ConsumeFood()
    {
        if (currentState == STATES.EAT)
        {
            bool isNearFood = nearFood != null && index == nearFood.Index;
            UpdateFitness(isNearFood ? consumeNearFoodFitness : consumeFoodFitness);
        }

        foodsConsumed++;
        steps = 0;
        PopulationManager.Instance.AddFoodsConsumed(team);
    }

    protected override void ProcessInputs()
    {
        inputs[0] = index.x;
        inputs[1] = index.y;
        inputs[2] = steps;

        if (currentState == STATES.EAT)
        {
            if (nearFood != null)
            {
                inputs[3] = nearFood.Index.x;
                inputs[4] = nearFood.Index.y;
                inputs[5] = index.x == nearFood.Index.x ? 1f : -1f;
                inputs[6] = index.y == nearFood.Index.y ? 1f : -1f;
            }
        }

        if (fitness > 5 && currentState == STATES.WALK) currentState++;
    }

    protected override void ProcessOutputs(float[] outputs)
    {
        if (!toStay)
        {
            transform.position = movePosition;
            startPosition = transform.position;
            index = moveIndex;
            steps++;
        }

        if (outputs != null && outputs.Length >= 3)
        {
            bool vertical = outputs[0] < 0.5f;
            float positive = outputs[1] < 0.5f ? -1f : 1f;
            bool stay = outputs[2] < 0.5f;

            Vector3 dir = new Vector3(vertical ? positive : 0f, 0f, !vertical ? positive : 0f);

            transform.forward = dir;
            movePosition = transform.position + dir * unit;
            moveIndex = index + new Vector2Int((int)dir.x, (int)dir.z);

            toStay = stay;

            UpdatePositionLimit();
            UpdateIndexLimit();

            if (currentState == STATES.WALK)
            {
                if (!stay) UpdateFitness(2);
            }

            if (moveIndex.y < 0 || moveIndex.y > maxIndex) // Chequeo el limite Y
            {
                SetFitness(outLimitYFitness);
                inOutLimit = true;
            }
        }
    }

    private void UpdatePositionLimit()
    {
        Vector3 startPos = startPosition;
        Vector3 movePos = movePosition;

        if (movePos.x > limitX)
        {
            movePos.x -= limitX * 2;
            startPos = movePos - new Vector3(unit, 0f, 0f);
        }
        else if (movePos.x < -limitX)
        {
            movePos.x += limitX * 2;
            startPos = movePos + new Vector3(unit, 0f, 0f);
        }

        startPosition = startPos;
        movePosition = movePos;
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
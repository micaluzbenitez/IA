using UnityEngine;
using Toolbox;
using RTSGame.Entities.Buildings;

namespace RTSGame.Entities.Agents
{
    public class Villager : Agent
    {
        [Header("Gold mine")]
        [SerializeField] private float timePerMine;
        [SerializeField] private float maxGoldRecolected;

        [Header("Food")]
        [SerializeField] private int goldsPerFood;

        [Header("UI")]
        [SerializeField] private TextMesh goldText;

        // Gold mine
        private GoldMine goldMine;
        private int goldQuantity;
        private Timer mineTimer = new Timer();

        // Food
        private bool needsFood = false;

        protected override void Awake()
        {
            base.Awake();
            goldText.text = goldQuantity.ToString();
            mineTimer.SetTimer(timePerMine, Timer.TIMER_MODE.DECREASE);
        }

        protected override void Update()
        {
            base.Update();
            UpdateMineTimer();
            CheckForFood();

            if (Input.GetKeyDown(KeyCode.M)) SetTargetPosition(FindNearestGoldMine());
        }

        protected Vector3 FindNearestGoldMine()
        {
            GoldMine[] goldMines = FindObjectsOfType<GoldMine>();
            int randomIndex = Random.Range(0, goldMines.Length);
            return goldMines[randomIndex].gameObject.transform.position;
        }

        private void UpdateMineTimer()
        {
            if (mineTimer.Active) mineTimer.UpdateTimer();

            if (mineTimer.ReachedTimer())
            {
                if (goldMine.ConsumeGold())
                {
                    goldQuantity++;
                    goldText.text = goldQuantity.ToString();

                    if (goldQuantity == maxGoldRecolected) SetTargetPosition(FindUrbanCenter());
                    else if (goldQuantity % goldsPerFood == 0) needsFood = true;
                    else mineTimer.ActiveTimer();
                }
                else
                {
                    SetTargetPosition(FindNearestGoldMine());
                }
            }
        }

        private void CheckForFood()
        {
            if (needsFood)
            {
                if (goldMine.ConsumeFood())
                {
                    mineTimer.ActiveTimer();
                    needsFood = false;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("GoldMine"))
            {
                goldMine = collision.GetComponent<GoldMine>();
                mineTimer.ActiveTimer();
            }

            if (collision.CompareTag("UrbanCenter"))
            {
                UrbanCenter urbanCenter = collision.GetComponent<UrbanCenter>();
                urbanCenter.DeliverGold(goldQuantity);
                goldQuantity = 0;
                goldText.text = goldQuantity.ToString();
                SetTargetPosition(FindNearestGoldMine());
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("GoldMine")) mineTimer.DesactiveTimer();
        }
    }
}
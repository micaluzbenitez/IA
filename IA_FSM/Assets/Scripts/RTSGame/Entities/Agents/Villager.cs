using UnityEngine;
using RTSGame.Interfaces;
using Toolbox;

namespace RTSGame.Entities.Agents
{
    public class Villager : Agent
    {
        [Header("Gold")]
        [SerializeField] private TextMesh text;

        [Header("Gold mine")]
        [SerializeField] private float timePerMine;
        [SerializeField] private float maxGoldRecolected;

        private IInteractable interactable;
        private int goldQuantity;

        // Gold mine
        private Timer mineTimer = new Timer();

        protected override void Awake()
        {
            base.Awake();
            text.text = goldQuantity.ToString();
            mineTimer.SetTimer(timePerMine, Timer.TIMER_MODE.DECREASE);
        }

        protected override void Update()
        {
            base.Update();
            UpdateMineTimer();

            if (Input.GetKeyDown(KeyCode.M)) SetTargetPosition(FindNearestGoldMine());
        }

        private void UpdateMineTimer()
        {
            if (mineTimer.Active) mineTimer.UpdateTimer();

            if (mineTimer.ReachedTimer())
            {
                if (interactable.Interact(goldQuantity))
                {
                    goldQuantity++;
                    text.text = goldQuantity.ToString();

                    if (goldQuantity == maxGoldRecolected) SetTargetPosition(FindUrbanCenter());
                    else mineTimer.ActiveTimer();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            interactable = collision.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (collision.CompareTag("GoldMine"))
                {
                    mineTimer.ActiveTimer();
                }
                if (collision.CompareTag("UrbanCenter"))
                {
                    interactable.Interact(goldQuantity); 
                    SetTargetPosition(FindNearestGoldMine());
                    goldQuantity = 0;
                    text.text = goldQuantity.ToString();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("GoldMine")) mineTimer.DesactiveTimer();
        }
    }
}
using UnityEngine;
using RTSGame.Entities.Buildings;
using RTSGame.Interfaces;
using Toolbox;

namespace RTSGame.Entities.Agents
{
    public class Villager : Agent
    {
        [Header("Gold mine")]
        [SerializeField] private float timePerMine;

        private DebugText debugText;
        private IInteractable interactable;
        private int goldQuantity;

        // Gold mine
        private Timer mineTimer = new Timer();

        private void Start()
        {
            debugText = GetComponent<DebugText>();
            if (debugText)
            {
                debugText.SetParent(transform);
                debugText.Text.text = goldQuantity.ToString();
            }

            mineTimer.SetTimer(timePerMine, Timer.TIMER_MODE.DECREASE);
        }

        protected override void Update()
        {
            base.Update();
            UpdateMineTimer();

            if (Input.GetKeyDown(KeyCode.M)) SetTargetPosition(FindNearestGoldMine());
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            interactable = collision.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (collision.CompareTag("GoldMine")) mineTimer.ActiveTimer();
                if (collision.CompareTag("UrbanCenter")) interactable.Interact();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("GoldMine")) mineTimer.DesactiveTimer();
        }

        private void UpdateMineTimer()
        {
            if (mineTimer.Active) mineTimer.UpdateTimer();

            if (mineTimer.ReachedTimer())
            {
                if (interactable.Interact())
                {
                    goldQuantity++;
                    if (debugText) debugText.Text.text = goldQuantity.ToString();
                    mineTimer.ActiveTimer();
                }
            }
        }

        private Vector3 FindNearestGoldMine()
        {
            GoldMine[] goldMines = FindObjectsOfType<GoldMine>();
            int randomIndex = Random.Range(0, goldMines.Length);
            return goldMines[randomIndex].gameObject.transform.position;
        }
    }
}
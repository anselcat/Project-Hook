using ASK.Core;
using ASK.Helpers;

using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public partial class MovementStateMachine : PlayerStateMachine<MovementStateMachine, MovementStateMachine.MovementState, MovementStateInput> {
        private SpriteRenderer _spriteR;

        //Expose to inspector
        public UnityEvent<MovementStateMachine> OnPlayerStateChange;

        public bool UsingDrill => IsOnState<Diving>() || IsOnState<Dogoing>();
        public bool DrillingIntoGround => IsOnState<Dogoing>();

        private bool _hasInputted;

        #region Overrides
        protected override void SetInitialState() 
        {
            SetState<Grounded>();
        }

        protected override void Init()
        {
            base.Init();
            _spriteR = GetComponentInChildren<SpriteRenderer>();
        }

        protected void OnEnable()
        {
            StateTransition += InvokeUnityStateChangeEvent;
            MyCore.DeathManager.OnPlayerRespawn += OnRespawn;
            MyCore.DeathManager.OnDeath += OnDeath;
        }

        protected void OnDisable()
        {
            StateTransition -= InvokeUnityStateChangeEvent;
            MyCore.DeathManager.OnPlayerRespawn -= OnRespawn;
            MyCore.DeathManager.OnDeath -= OnDeath;
        }

        private void InvokeUnityStateChangeEvent()
        {
            OnPlayerStateChange?.Invoke(this);
        }

        protected override void Update()
        {
            base.Update();
            
            if (!_hasInputted && MyCore.Input.AnyKeyPressed()) _hasInputted = true;

            if (MyCore.Input.JumpStarted())
            {
                CurrState.JumpPressed();
            }

            if (MyCore.Input.JumpFinished())
            {
                CurrState.JumpReleased();
            }

            if (MyCore.Input.DiveStarted())
            {
                CurrState.DivePressed();
            }

            if (MyCore.Input.RetryStarted())
            {
                MyCore.Actor.Die();
            }

            // CurrInput.moveDirection = MyCore.Input.GetMovementInput();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            GameTimer.FixedUpdate(CurrInput.jumpBufferTimer);
        }
        #endregion

        public void SetGrounded(bool isGrounded, bool isMovingUp) {
            CurrState.SetGrounded(isGrounded, isMovingUp);
        }

        public void RefreshAbilities()
        {
            CurrState.RefreshAbilities();
        }

        public void OnDeath()
        {
            // _spriteR.SetAlpha(0);
            // CurrInput.diePos = diePos;
            Transition<Dead>();
        }

        public void OnRespawn()
        {
            Transition<Airborne>();
        }

        public Vector2 ProcessMoveX(PlayerActor p, Vector2 velocity, int direction) {
            return CurrState.MoveX(p, velocity, direction);
        }
    }
}
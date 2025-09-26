using System;
using System.Collections;
using UnityEngine;

namespace InputManagement
{
    /// <summary>
    /// An input receiver that implements charge and release functionality
    /// </summary>
    public class BufferedChargeInputReceiver : ButtonInputReceiver
    {
        [Tooltip("The minimum amount of time required to start charging. Any less time and it will have been considered a 'tap' input")]
        [SerializeField] private float tapDetectionWindow = 0.1f;
        [SerializeField] private float chargeTime = 0.5f;
        [Tooltip("The charge will automatically complete if there is less than this amount of time left when the button is released")]
        [SerializeField] private float chargeReleaseBufferTime = 0.1f;
        /// <summary>
        /// Like chargeTimerStarted, only this is false if you're still withint the tap detection window
        /// </summary>
        public bool Charging { get; private set; }
        /// <summary>
        /// Used to track if the charge timer has started or not
        /// </summary>
        private bool chargeTimerStarted = false;
        protected Timer timer = new Timer();
        public float progress => timer.progress;

        public event EventHandler<ChargeInputEventArgs> OnChargeStart;
        /// <summary>
        /// Called when
        /// - cancelling the charge. (Not called if charge fully released, or cancelled during the tap detection window)
        /// - this input receiver gets deactivated
        /// </summary>
        public event EventHandler<ChargeInputEventArgs> OnChargeCancel;

        protected override void InterceptInput()
        {
            // The buffer threshold 
            if (HandleReleaseBufferInputOverride())
                return;

            // Starting and releasing charge
            if (buttonInputProvider.GetHeld(priority))
            {
                if (!chargeTimerStarted)
                {
                    StartCharge();
                }

                // If you're still charging outside of the tap detection window, announce that you've started charging
                if (!Charging && !isWithinTapDetectionWindow)
                {
                    Charging = true;
                    OnChargeStart?.Invoke(this, new ChargeInputEventArgs
                    {
                        duration = chargeTime,
                        chargedTime = 0,
                    });
                }
            }
            else if (buttonInputProvider.GetReleasedThisFrame(priority) && chargeTimerStarted)
            {
                ReleaseChargeInput();
            }
        }

        private void StartCharge()
        {
            chargeTimerStarted = true;
            timer.Start(chargeTime);
        }

        protected virtual void ReleaseChargeInput()
        {
            // Override player input and start a buffer to finish the charge if its almost completed
            if (bufferOverrideApplicable)
            {
                StartChargeReleaseBuffer();
                return;
            }

            if (timer.finished)
            {
                FullyReleaseCharge();
            }
            else
            {
                CancelCharge();
            }
        }

        private void FullyReleaseCharge()
        {
            Charging = false;
            chargeTimerStarted = false;
            ResolveInput();
        }

        private void CancelCharge()
        {
            Charging = false;
            chargeTimerStarted = false;

            if (isWithinTapDetectionWindow)
            {
                // If the charge was cancelled before the minimum charge time, ignore the input
                TapDetected();
            }
            else
            {
                OnChargeCancel?.Invoke(this, new ChargeInputEventArgs
                {
                    duration = chargeTime,
                    chargedTime = chargeTime - timer.timeUntilCompletion
                });
            }

        }

        protected override void OnInputReceivingEnabled()
        {
            StartCoroutine(BufferTapCheck());
            base.OnInputReceivingEnabled();
        }

        private IEnumerator BufferTapCheck()
        {
            yield return null;
            if (!buttonInputProvider.GetHeld(priority) && buttonInputProvider.TryUseBuffer(priority))
            {
                TapDetected();
            }
        }

        protected override void OnInputReceivingDisabled()
        {
            CancelCharge();
            base.OnInputReceivingDisabled();
        }

        #region Tap Detection Window

        public event EventHandler OnTapInputExecuted;

        private bool isWithinTapDetectionWindow => timer.timeElapsed <= tapDetectionWindow;


        private void TapDetected()
        {
            OnTapInputExecuted?.Invoke(this, EventArgs.Empty);
        }


        #endregion


        #region Release Buffer

        private bool bufferOverrideActive;
        private bool bufferOverrideApplicable => !timer.finished && timer.timeUntilCompletion < chargeReleaseBufferTime;

        /// <summary>
        /// If the player releases the button when they are almost fully charged, complete the rest of the charge automatically for them
        /// </summary>
        /// <returns></returns>
        private bool HandleReleaseBufferInputOverride()
        {
            if (timer.finished && bufferOverrideActive)
            {
                ReleaseChargeInput();
                bufferOverrideActive = false;
            }

            return bufferOverrideActive;
        }

        /// <summary>
        /// If the player releases the charge when they are almost fully charged, complete the rest of the charge automatically for them
        /// </summary>
        /// <returns></returns>
        private void StartChargeReleaseBuffer()
        {
            bufferOverrideActive = true;
        }

        #endregion

    }

    public class ChargeInputEventArgs : EventArgs
    {
        public float duration;
        public float chargedTime;

        public float chargedAmount => chargedTime / duration;
    }
}

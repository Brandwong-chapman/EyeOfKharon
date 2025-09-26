using System;
using UnityEngine;

namespace InputManagement
{
    /// <summary>
    /// Implements detecting button presses and a buffer system
    /// </summary>
    [Serializable]
    public class ButtonInputProvider : InputProvider
    {

        #region Detecting Input

        private float lastTimePressed;
        private float lastTimeReleased;
        private bool held;

        internal void OnPress()
        {
            held = true;
            lastTimePressed = Time.unscaledTime;
            OnPressEvent?.Invoke(this, new PriorityEventArgs
            {
                requiredPriority = RequiredPriority
            });

            alreadyUsedBuffer = false;
        }

        internal void OnRelease()
        {
            held = false;
            lastTimeReleased = Time.unscaledTime;
            OnReleaseEvent?.Invoke(this, new PriorityEventArgs
            {
                requiredPriority = RequiredPriority
            });
        }

        #endregion


        #region Public Accessors

        public event EventHandler<PriorityEventArgs> OnPressEvent;
        public event EventHandler<PriorityEventArgs> OnReleaseEvent;
        public class PriorityEventArgs : EventArgs
        {
            public int requiredPriority;
        }

        public bool GetHeld(int priority = 0)
        {
            if (priority < RequiredPriority)
                return false;

            return held;
        }

        public bool GetPressedThisFrame(int priority = 0)
        {
            if (priority < RequiredPriority)
                return false;

            return Time.unscaledTime == lastTimePressed;
        }

        public bool GetReleasedThisFrame(int priority = 0)
        {
            if (priority < RequiredPriority)
                return false;

            return Time.unscaledTime == lastTimeReleased;
        }

        #endregion


        #region Buffer System

        private const float BUFFER_TIME = 0.2f;
        private bool alreadyUsedBuffer;
        private float bufferUsedTime;
        private bool bufferAvailable => withinBufferTime && !alreadyUsedBuffer;
        private bool withinBufferTime => lastTimePressed + BUFFER_TIME > Time.unscaledTime;

        public bool TryUseBuffer(int priority = 0)
        {
            if (priority < RequiredPriority)
                return false;

            if (bufferAvailable || bufferUsedTime == Time.unscaledTime)
            {
                bufferUsedTime = Time.unscaledTime;
                alreadyUsedBuffer = true;
                return true;
            }

            return false;
        }

        public void RefreshBuffer()
        {
            alreadyUsedBuffer = true;
        }

        #endregion

    }
}
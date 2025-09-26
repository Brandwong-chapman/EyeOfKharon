using System;

namespace InputManagement
{
    /// <summary>
    /// Handles more complex button interactions, such as double tapping or charging up an ability
    /// </summary>
    public class ButtonInputReceiver : InputListener
    {
        protected ButtonInputProvider buttonInputProvider => (ButtonInputProvider)inputProvider;

        public event EventHandler OnExecuteInput;
        private bool refreshBufferQueued;

        /// <summary>
        /// Handles implementation of input gestures: double tapping, charging, etc...
        /// MAKE SURE TO USE THIS OBJECT'S PRIORITY
        /// Calls ResolveInput() when the criteria are met
        /// </summary>
        protected override void InterceptInput()
        {
            if (refreshBufferQueued)
            {
                buttonInputProvider.RefreshBuffer();
                refreshBufferQueued = false;
                return;
            }

            try
            {
                // This is just an example of how to implement a buffered input
                if (buttonInputProvider.TryUseBuffer(priority))
                {
                    ResolveInput();
                }
            }
            catch
            {
                // Handle exceptions
                if (buttonInputProvider == null)
                    Unbind(null);
            }
        }

        public void RefreshBuffer()
        {
            refreshBufferQueued = true;
        }

        /// <summary>
        /// Execute the input
        /// </summary>
        protected virtual void ResolveInput()
        {
            OnExecuteInput?.Invoke(this, EventArgs.Empty);
        }
    }
}
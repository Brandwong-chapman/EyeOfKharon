using UnityEngine;

namespace InputManagement
{
    public class AxisInputProvider : InputProvider
    {
        private Vector2 value;

        internal void SetValue(Vector2 value)
        {
            this.value = value;
        }

        public Vector2 GetValue(int priority = 0)
        {
            if (priority < RequiredPriority)
                return Vector2.zero;

            return value;
        }
    }
}

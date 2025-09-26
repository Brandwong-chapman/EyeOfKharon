namespace InputManagement
{
    /// <summary>
    /// All inputs on any given frame. Shared by a single input manager
    /// </summary>
    [System.Serializable]
    public class InputState
    {
        public AxisInputProvider move = new AxisInputProvider();
        public AxisInputProvider aim = new AxisInputProvider();
        public ButtonInputProvider jump = new ButtonInputProvider();
        public ButtonInputProvider primary = new ButtonInputProvider();
        public ButtonInputProvider secondary = new ButtonInputProvider();
        public ButtonInputProvider interact = new ButtonInputProvider();
        public ButtonInputProvider crouch = new ButtonInputProvider();
        public ButtonInputProvider sprint = new ButtonInputProvider();
        public ButtonInputProvider pause = new ButtonInputProvider();
        public ButtonInputProvider dash = new ButtonInputProvider();
    }
}

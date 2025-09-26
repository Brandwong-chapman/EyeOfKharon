using System;
using UnityEngine;

namespace ControllerSystem.Platformer2D
{
    public class PlatformerPauseModule : PlatformerMotorModule
    {
        private bool isPaused = false;
        public override void HandleMovement()
        {
            HandlePause();
        }

        public void HandlePause()
        {
            if (Controller.Input.pause.GetPressedThisFrame())
            {
                isPaused = !isPaused;
                if (isPaused)
                    GameTimeScale.Pause();
                else
                    GameTimeScale.Resume();
            }
            
           
        }
    }
}

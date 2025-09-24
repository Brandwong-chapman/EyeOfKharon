using System;
using UnityEngine;

namespace ControllerSystem.Platformer2D
{
    public class PlatformerPauseModule : PlatformerMotorModule
    {
        private bool isPaused = false;
        public override void HandleMovement()
        {
            print("a");
            HandlePause();
        }

        public void HandlePause()
        {
            if (Controller.Input.pause.GetPressedThisFrame())
            {
                print("s");
                isPaused = !isPaused;
                if (isPaused)
                    GameTimeScale.Pause();
                else
                    GameTimeScale.Resume();
            }
            
           
        }
    }
}

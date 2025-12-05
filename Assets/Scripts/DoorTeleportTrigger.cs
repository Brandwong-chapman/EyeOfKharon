using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class DoorTeleportTrigger : MonoBehaviour
{
    public SubLevelDoor teleportData;

    private bool _playerInside;
    private Coroutine _holdRoutine;
    private ControllerSystem.Platformer2D.PlatformerMotor _playerMotor;
    
    private int _collidersInside = 0;
    

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (!other.CompareTag("PlayerCollision")) return;
            _playerMotor = other.GetComponentInParent<ControllerSystem.Platformer2D.PlatformerMotor>();
        if (_playerMotor != null)
        {
            _collidersInside++;
            if (_collidersInside == 1) // first collider entered
            {
                _playerInside = true;
                _holdRoutine = StartCoroutine(HoldUpRoutine());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerCollision")) return;

            _collidersInside--;
            if (_collidersInside == 0)
            {
                _playerInside = false;
                StopCoroutine(_holdRoutine);
            }
    }


    private IEnumerator HoldUpRoutine()
    {
        float timer = 0f;

        while (_playerInside)
        {
            // Use same input buffer system

                Vector2 input = _playerMotor.Controller.Input.move.GetValue();
                if (input.y > 0.5f)
                    timer += Time.deltaTime;
                else
                    timer = 0f;
                
                Debug.Log($"Up input: {input.y}, timer: {timer}");

            
            if (timer >= teleportData.holdUpTime)
            {
                SceneManager.LoadScene(teleportData.sceneName);
                yield break;
            }

            yield return null;
        }
    }

  
}

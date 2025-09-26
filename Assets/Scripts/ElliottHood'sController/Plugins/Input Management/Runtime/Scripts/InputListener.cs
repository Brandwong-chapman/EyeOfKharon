using InputManagement;
using System;
using System.Collections;
using UnityEngine;

public abstract class InputListener : MonoBehaviour, IPriorityGroupable
{
    [SerializeField] protected int priority = 0;
    protected InputProvider inputProvider;

    #region Enabling / Disabling the Input Listener

    public event EventHandler OnEnabled;
    public event EventHandler OnDisabled;
    protected virtual void OnInputReceivingEnabled() { }
    protected virtual void OnInputReceivingDisabled() { }

    private bool active;
    /// <summary>
    /// Whether or not the input listener is active. This is used to determine if the input listener should be receiving input.
    /// </summary>
    internal bool Active
    {
        get
        {
            return active;
        }
        set
        {
            value = value && inputProvider != null && GetRetrievable();

            if (active == value)
                return;

            active = value;

            if (active)
            {
                StopAllCoroutines();
                StartCoroutine(InterceptInputCoroutine());

                OnEnabled?.Invoke(this, EventArgs.Empty);
                OnInputReceivingEnabled();
            }
            else
            {
                StopAllCoroutines();

                OnDisabled?.Invoke(this, EventArgs.Empty);
                OnInputReceivingDisabled();
            }
        }
    }

    #endregion

    /// <summary>
    /// Handles implementation of input gestures: double tapping, charging, etc...
    /// MAKE SURE TO GET INPUTS USING THIS OBJECT'S PRIORITY ex. inputProvider.GetValue(priority);
    /// </summary>
    protected abstract void InterceptInput();

    #region Intercepting Input

    internal virtual void BindToInput(InputProvider inputProvider)
    {
        this.inputProvider = inputProvider;
        enabled = true;
    }

    private IEnumerator InterceptInputCoroutine()
    {
        while (Active)
        {
            yield return null;
            InterceptInput();
        }
    }

    internal virtual void Unbind(InputProvider inputProvider)
    {
        this.inputProvider = null;
        enabled = false;
    }

    #endregion


    #region IPriority Groupable

    public int GetPriority()
    {
        return priority;
    }

    public bool GetRetrievable()
    {
        return this != null && enabled && gameObject.activeInHierarchy;
    }

    #endregion


    #region Enabling / Disabling
    private void OnEnable()
    {
        if (inputProvider != null && GetRetrievable())
        {
            inputProvider.UpdateReceiverEnableStates();
        }
    }

    private void OnDisable()
    {
        if (inputProvider != null)
        {
            inputProvider.UpdateReceiverEnableStates();
        }
    }

    #endregion
}

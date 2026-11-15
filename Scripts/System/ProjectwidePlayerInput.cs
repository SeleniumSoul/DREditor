///Project-wide Player input script by SeleniumSoul
///Adapted from Puppet Play

using DREditor;
using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectwidePlayerInput : MonoBehaviour, IModule
{
    public enum ActionFlag
    {
        performed, started, cancelled
    }

    private const BindingFlags bindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    [SerializeField] private InputActionAsset inputAsset;

    public void Awake()
    {
        if (inputAsset == null)
            inputAsset = InputSystem.actions;

        inputAsset.Enable();
    }

    public Task Init() { return Task.CompletedTask; }

    public void RegisterInput(InputActionReference inputAction, Action<InputAction.CallbackContext> action, ActionFlag flag = ActionFlag.performed)
    {
        Process(true, inputAction, action, flag);
    }

    public void UnregisterInput(InputActionReference inputAction, Action<InputAction.CallbackContext> action, ActionFlag flag = ActionFlag.performed)
    {
        Process(false, inputAction, action, flag);
    }

    private void Process(bool register, InputActionReference inputAction, Action<InputAction.CallbackContext> action, ActionFlag flag = ActionFlag.performed)
    {
        if (inputAction == null || inputAction.action == null)
        {
            Debug.LogError("RegisterInput failed: InputActionReference or its inner action is null!");
            return;
        }

        string mapName = inputAction.action.actionMap?.name;
        string actionName = inputAction.action.name;

        if (string.IsNullOrEmpty(mapName) || string.IsNullOrEmpty(actionName))
        {
            Debug.LogError("RegisterInput failed: Action map or action name is missing!");
            return;
        }

        if (register)
        {
            switch (flag)
            {
                case ActionFlag.performed:
                    inputAsset.FindActionMap(mapName).FindAction(actionName).performed += action;
                    break;
                case ActionFlag.started:
                    inputAsset.FindActionMap(mapName).FindAction(actionName).started += action;
                    break;
                case ActionFlag.cancelled:
                    inputAsset.FindActionMap(mapName).FindAction(actionName).canceled += action;
                    break;
            }
        }
        else
        {
            switch (flag)
            {
                case ActionFlag.performed:
                    inputAsset.FindActionMap(mapName).FindAction(actionName).performed -= action;
                    break;
                case ActionFlag.started:
                    inputAsset.FindActionMap(mapName).FindAction(actionName).started -= action;
                    break;
                case ActionFlag.cancelled:
                    inputAsset.FindActionMap(mapName).FindAction(actionName).canceled -= action;
                    break;
            }
        }
    }

    public void SetMapActive(string mapName, bool active, bool solo = true)
    {
        if (!solo)
        {
            if (active)
                inputAsset.FindActionMap(mapName).Enable();
            else
                inputAsset.FindActionMap(mapName).Disable();

            return;
        }

        foreach(var map in inputAsset.actionMaps)
        {
            if (active)
                map.Enable();
            else
                map.Disable();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class ActionSet : PlayerActionSet {

    public PlayerAction Left;
    public PlayerAction Right;
    public PlayerAction Up;
    public PlayerAction Down;

    public PlayerAction ActionPrimary;
    public PlayerAction ActionPrimaryMouse;
    public PlayerAction ActionSecondary;
    public PlayerAction ActionSecondaryMouse;
    public PlayerAction ActivateLight;
    public PlayerAction Grab;

    public PlayerAction Menu;

    public PlayerOneAxisAction MoveX;
    public PlayerOneAxisAction MoveY;

    public ActionSet() {
        Left = CreatePlayerAction("Move Left");
        Right = CreatePlayerAction("Move Right");
        Up = CreatePlayerAction("Move Up");
        Down = CreatePlayerAction("Move Down");
        ActionPrimary = CreatePlayerAction("ActionPrimary");
        ActionPrimaryMouse = CreatePlayerAction("ActionPrimaryMouse");
        ActionSecondary = CreatePlayerAction("ActionSecondary");
        ActionSecondaryMouse = CreatePlayerAction("ActionSecondaryMouse");
        
        ActivateLight = CreatePlayerAction("ActivateLight");
        Grab = CreatePlayerAction("Grab");
        MoveX = CreateOneAxisPlayerAction(Left, Right);
        MoveY = CreateOneAxisPlayerAction(Down, Up);

        Menu = CreatePlayerAction("OpenMenu");
    }
}

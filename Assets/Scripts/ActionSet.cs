using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class ActionSet : PlayerActionSet {

    public PlayerAction Left;
    public PlayerAction Right;
    public PlayerAction Up;
    public PlayerAction Down;
    public PlayerAction Action;
    public PlayerAction Grab;
    public PlayerOneAxisAction MoveX;
    public PlayerOneAxisAction MoveY;

    public ActionSet() {
        Left = CreatePlayerAction("Move Left");
        Right = CreatePlayerAction("Move Right");
        Up = CreatePlayerAction("Move Up");
        Down = CreatePlayerAction("Move Down");
        Action = CreatePlayerAction("Action");
        Grab = CreatePlayerAction("Grab");
        MoveX = CreateOneAxisPlayerAction(Left, Right);
        MoveY = CreateOneAxisPlayerAction(Down, Up);
    }
}

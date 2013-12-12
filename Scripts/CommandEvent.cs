using UnityEngine;
using System.Collections;

public class CommandEvent : InputEvent 
{
    public CommandEvent()
    {
    }

    public CommandEvent(GamePlayState.GameMode command)
    {
        this.Command = command;
    }

    public GamePlayState.GameMode Command
    {
        get; set;
    }
}

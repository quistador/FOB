using UnityEngine;
using System.Collections;

public class CommandEvent : InputEvent 
{

    public CommandEvent()
    {
    }

    public CommandEvent(GamePlayState.CommandState command)
    {
        this.Command = command;
    }

    public GamePlayState.CommandState Command
    {
        get; set;
    }
}

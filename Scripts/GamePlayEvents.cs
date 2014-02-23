using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// contains event delegate definitions for events. 
/// </summary>
namespace GamePlayEventDelegates
{
    public delegate void OrderAddedEventHandler(Order sender);
    public delegate void ActionModeButtonPressedEventHandler(int someParameter);
    public delegate void UnitArrivedInBuilding(GamePlayEvent eventInfo);
    public delegate void UnitDepartedBuilding(GamePlayEvent eventInfo);
    public delegate void GamePaused();
    public delegate void GameUnPaused();
}

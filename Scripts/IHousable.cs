using UnityEngine;
using System.Collections;

public interface IHousable
{
    void HandleUnitArrived(GamePlayEvent eventInfo);
    void HandleUnitDeparted(GamePlayEvent eventInfo);
    bool ContainsNode(int NodeId);
    UnitListControl UnitsInHousing();
}

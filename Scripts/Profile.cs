using UnityEngine;
using System.Collections;

public class Profile
{
    private Army army;
    private string _playerName;

    public Profile(string player)
    {
        this._playerName = player;
    }

    public Army GetArmy()
    {
        ObjectToXmlSerializer deserializer = new ObjectToXmlSerializer();
        Army armyDeserialized = deserializer.DeserializeUnitsFromFile(this._playerName);
        return armyDeserialized;
    }
}

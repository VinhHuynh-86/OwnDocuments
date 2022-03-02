using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OPS.AntiCheat.Field;

[System.Serializable]
public class Item
{
    public string ID;
    public ProtectedInt32 price;
    public Define.ITEM_STATUS status;
    public Define.NEW_STATUS newStatus;
    public string nameItem;
    public string venueRequirement;
    
    public void UpdateInfo(ProfileItem profileCostumeItem)
    {
        ID = profileCostumeItem.ID;
        status = profileCostumeItem.status;
        if(status == Define.ITEM_STATUS.DEFAULT)
        {
            newStatus = Define.NEW_STATUS.DONE;
            profileCostumeItem.newStatus = Define.NEW_STATUS.DONE;
        }
        else
        {
            newStatus = profileCostumeItem.newStatus;
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetHistoryManager : GenericObjectPool<BetHistoryView>
{
  [SerializeField] private SocketIOManager socket;
  
  internal void PopulateBetHistory()
  {
    base.ReturnAllItemsToPool();

    List<BetHistory> betData = socket.BetHistoryData.payload.betHistory;

    if (betData.Count > 0)
    {
      foreach(var data in betData)
      {
        var item = base.GetFromPool();
        item.Set(data);
        item.transform.SetAsLastSibling();
      }
    }
    else
    {
      Debug.LogWarning("No Bet Data Found!");
    }
  }  
}

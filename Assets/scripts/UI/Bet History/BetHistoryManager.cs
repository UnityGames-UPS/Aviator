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
      for (int i = 0; i < betData.Count; i++)
      {
        var item = base.GetFromPool();
        item.Set(betData[i]);
        item.transform.SetSiblingIndex(i);
      }
    }
    else
    {
      Debug.LogWarning("No Bet Data Found!");
    }
  }  
}

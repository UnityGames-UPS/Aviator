using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundAnalyticsManager : GenericObjectPool<RoundAnalyticsView>
{
  [SerializeField] private SocketIOManager socket;
  
  internal void PopulateRoundAnalytics()
  {
    base.ReturnAllItemsToPool();

    
  }
}

using UnityEngine;
using System.Collections;
using System;

namespace CustomNetworkCode
{
    public class Player : IDisposable
    {
        public string Name { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public NetworkPlayer NetworkPlayerInstance { get; set; }
        public string IP { get; set; }
        public int Stability { get; set; }
        public int AdditionalStability { get; set; }
        public int TotalStability { get { return Stability + AdditionalStability; } }
        public int Class { get; set; }
        public string CurrentWeapon { get; set; }
        
        public int GetAveragePing()
        {
            return Network.GetAveragePing(NetworkPlayerInstance);
        }
        public int GetLastPing()
        {
            return Network.GetLastPing(NetworkPlayerInstance);
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public sealed class ZoneWhiteListCache
    {
        private static ZoneWhiteListCache uniqueInstance;

        private ZoneWhiteListCache()
        {
        }

        public static ZoneWhiteListCache GetInstance()
        {
            if (null == uniqueInstance)
            {
                uniqueInstance = new ZoneWhiteListCache();
            }

            return uniqueInstance;
        }

        private ConcurrentDictionary<string, List<string>> _zoneWhiteListDic = new ConcurrentDictionary<string, List<string>>();

        public List<string> GetZoneWhiteList(string zoneId)
        {
            if (_zoneWhiteListDic.ContainsKey(zoneId))
            {
                if (_zoneWhiteListDic.TryGetValue(zoneId, out var zonWhiteList))
                    return zonWhiteList;
            }
            return new List<string>();
        }

        public bool AddZoneWhiteList(string zoneId, List<string> zoneWhiteList)
        {
            if (_zoneWhiteListDic.ContainsKey(zoneId))
            {
                if (_zoneWhiteListDic.TryRemove(zoneId, out var oldZoneWhiteList))
                {
                    return _zoneWhiteListDic.TryAdd(zoneId, zoneWhiteList);
                }
            }
            else
            {
                return _zoneWhiteListDic.TryAdd(zoneId, zoneWhiteList);
            }

            return false;
        }
    }
}

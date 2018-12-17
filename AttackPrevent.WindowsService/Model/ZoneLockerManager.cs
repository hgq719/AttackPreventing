using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AttackPrevent.Model;

namespace AttackPrevent.WindowsService
{
    public class ZoneLocker
    {
        public string ZoneId;
        public volatile bool IsRunning;
    }

    public static class ZoneLockerManager
    {
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private static List<ZoneLocker> _zoneLockers;

        public static void RefreshZoneLockers(List<ZoneEntity> zoneEntities )
        {
            _lock.EnterWriteLock();
            try
            {
                foreach (var zone in zoneEntities)
                {
                    if (_zoneLockers.All(p => p.ZoneId != zone.ZoneId))
                    {
                        _zoneLockers.Add(new ZoneLocker());
                    }
                }

                foreach (var zoneLocker in _zoneLockers)
                {
                    if (!(zoneEntities.Any(p => p.ZoneId == zoneLocker.ZoneId)) &&
                        zoneLocker.IsRunning == false
                    )
                    {
                        _zoneLockers.Remove(zoneLocker);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
        }

        public static bool IsRunning(string zoneId)
        {
            _lock.EnterReadLock();
            try
            {
                var zonelocker = _zoneLockers.Find(p => p.ZoneId == zoneId);
                return zonelocker == null || zonelocker.IsRunning;
            }
            finally
            {
                _lock.ExitReadLock();
            }

        }

        public static void SetZoneRunningStatus(string zoneId, bool isRunning)
        {
            _lock.EnterWriteLock();
            try
            {
                var zonelocker = _zoneLockers.Find(p => p.ZoneId == zoneId);
                if (zonelocker != null)
                {
                    zonelocker.IsRunning = isRunning;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}

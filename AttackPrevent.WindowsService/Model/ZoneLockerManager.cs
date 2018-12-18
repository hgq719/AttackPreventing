using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AttackPrevent.Model;

namespace AttackPrevent.WindowsService
{
    public class ZoneLocker
    {
        public string ZoneId;
        public bool IsRunning;
    }

    public static class ZoneLockerManager
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private static List<ZoneLocker> _zoneLockers = new List<ZoneLocker>();

        public static void RefreshZoneLockers(List<ZoneEntity> zoneEntities )
        {
            Lock.EnterWriteLock();
            try
            {
                foreach (var zone in zoneEntities)
                {
                    if (_zoneLockers.All(p => p.ZoneId != zone.ZoneId))
                    {
                        _zoneLockers.Add(new ZoneLocker() { ZoneId = zone.ZoneId, IsRunning = false });
                    }
                }

                var zoneLockersToRemove = _zoneLockers.Where(zoneLocker => !(zoneEntities.Any(p => p.ZoneId == zoneLocker.ZoneId)) && zoneLocker.IsRunning == false).ToList();
                _zoneLockers.RemoveAll(zoneLocker => zoneLockersToRemove.Any(p => p.ZoneId == zoneLocker.ZoneId));
            }
            finally
            {
                Lock.ExitWriteLock();
            }
            
        }

        public static bool IsRunning(string zoneId)
        {
            Lock.EnterReadLock();
            try
            {
                var zonelocker = _zoneLockers.Find(p => p.ZoneId == zoneId);
                return zonelocker == null || zonelocker.IsRunning;
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        public static void SetZoneRunningStatus(string zoneId, bool isRunning)
        {
            Lock.EnterWriteLock();
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
                Lock.ExitWriteLock();
            }
        }
    }
}

using System;

namespace WebApp.Service.Infrastructure.Database
{
    [Flags]
    public enum DbSeedObjects
    {
        None = 0,
        /// DB objects (triggers, views, SPs, UDFs, etc.)
        DbObjects = 0x1,
        /// <summary>
        /// Base data (users, roles, etc.)
        /// </summary>
        BaseData = 0x2,
        /// <summary>
        /// All data
        /// </summary>
        AllData = BaseData,
        /// <summary>
        /// All seedable objects
        /// </summary>
        All = AllData | DbObjects
    }
}

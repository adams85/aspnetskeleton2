using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace WebApp.DataAccess.Infrastructure
{
    internal interface IExtendedDbContextTransaction : IDbContextTransaction
    {
        void RegisterForCommit(Action callback);
    }
}

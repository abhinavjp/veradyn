using System;
using System.Collections.Generic;
using System.Threading;
using Veradyn.Core.Models.Domain;

namespace Veradyn.Core.InMemory.Data
{
    // Global Singleton Store using ReaderWriterLockSlim
    public static class InMemoryStore
    {
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public static List<Tenant> Tenants { get; } = new List<Tenant>();
        public static List<Client> Clients { get; } = new List<Client>();
        public static List<User> Users { get; } = new List<User>();
        public static List<AuthorizationCode> AuthCodes { get; } = new List<AuthorizationCode>();
        public static List<Token> Tokens { get; } = new List<Token>();
        public static List<ConsentGrant> Consents { get; } = new List<ConsentGrant>();

        public static void Read(Action action)
        {
            _lock.EnterReadLock();
            try
            {
                action();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public static void Write(Action action)
        {
            _lock.EnterWriteLock();
            try
            {
                action();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}

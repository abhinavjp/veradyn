using System;
using System.Collections.Generic;
using System.Linq;
using Veradyn.Core.Interfaces.Data;
using Veradyn.Core.Models.Domain;

namespace Veradyn.Core.InMemory.Data
{
    public class InMemoryUnitOfWork : IUnitOfWork
    {
        private readonly List<Action> _pendingChanges = new List<Action>();

        public IRepository<Tenant> Tenants { get; }
        public IRepository<Client> Clients { get; }
        public IRepository<User> Users { get; }
        public IRepository<AuthorizationCode> AuthCodes { get; }
        public IRepository<Token> Tokens { get; }
        public IRepository<ConsentGrant> Consents { get; }

        public InMemoryUnitOfWork()
        {
            // Initialize Repositories pointing to the static store Lists
            Tenants = new InMemoryRepository<Tenant>(InMemoryStore.Tenants, _pendingChanges, t => t.Id);
            Clients = new InMemoryRepository<Client>(InMemoryStore.Clients, _pendingChanges, c => c.ClientId);
            Users = new InMemoryRepository<User>(InMemoryStore.Users, _pendingChanges, u => u.Id);
            AuthCodes = new InMemoryRepository<AuthorizationCode>(InMemoryStore.AuthCodes, _pendingChanges, a => a.Code);
            Tokens = new InMemoryRepository<Token>(InMemoryStore.Tokens, _pendingChanges, t => t.Id);
            Consents = new InMemoryRepository<ConsentGrant>(InMemoryStore.Consents, _pendingChanges, c => c.Id);
        }

        public void Commit()
        {
            if (_pendingChanges.Count == 0) return;

            // Execute all pending changes under a Write Lock
            InMemoryStore.Write(() =>
            {
                foreach (var action in _pendingChanges)
                {
                    action();
                }
            });
            _pendingChanges.Clear();
        }

        public void Rollback()
        {
            _pendingChanges.Clear();
        }

        public void Dispose()
        {
            _pendingChanges.Clear();
        }
    }

    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private readonly List<T> _source;
        private readonly List<Action> _pendingChanges;
        private readonly Func<T, string> _idSelector;

        public InMemoryRepository(List<T> source, List<Action> pendingChanges, Func<T, string> idSelector)
        {
            _source = source;
            _pendingChanges = pendingChanges;
            _idSelector = idSelector;
        }

        public void Add(T entity)
        {
            // Defer execution
            _pendingChanges.Add(() => _source.Add(entity));
        }

        public IEnumerable<T> Find(Func<T, bool> predicate)
        {
            // Read immediately (Consistent Read)
            // Note: This does NOT see pending changes in this UoW transaction yet, unless we query them too.
            // For this simple implementation, we read committed state.
            List<T> result = null;
            InMemoryStore.Read(() =>
            {
                result = _source.Where(predicate).ToList();
            });
            return result;
        }

        public IEnumerable<T> GetAll()
        {
            List<T> result = null;
            InMemoryStore.Read(() =>
            {
                result = _source.ToList();
            });
            return result;
        }

        public T GetById(string id)
        {
            T result = null;
            InMemoryStore.Read(() =>
            {
                result = _source.FirstOrDefault(x => _idSelector(x) == id);
            });
            return result;
        }

        public void Remove(T entity)
        {
            var id = _idSelector(entity);
            _pendingChanges.Add(() =>
            {
                var item = _source.FirstOrDefault(x => _idSelector(x) == id);
                if (item != null) _source.Remove(item);
            });
        }

        public void Update(T entity)
        {
            // For in-memory objects, if we modified properties of the referenced object that we got from GetById,
            // they might already be modified in the source list if we didn't clone them.
            // However, assuming we are careful, or if we want to replace the object:

            var id = _idSelector(entity);
            _pendingChanges.Add(() =>
            {
                var index = _source.FindIndex(x => _idSelector(x) == id);
                if (index != -1)
                {
                    _source[index] = entity;
                }
            });
        }
    }
}

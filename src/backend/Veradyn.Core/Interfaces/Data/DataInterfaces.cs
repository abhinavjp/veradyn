using System;
using System.Collections.Generic;
using Veradyn.Core.Models.Domain;

namespace Veradyn.Core.Interfaces.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Tenant> Tenants { get; }
        IRepository<Client> Clients { get; }
        IRepository<User> Users { get; }
        IRepository<AuthorizationCode> AuthCodes { get; }
        IRepository<Token> Tokens { get; }
        IRepository<ConsentGrant> Consents { get; }

        void Commit();
        void Rollback();
    }

    public interface IRepository<T> where T : class
    {
        T GetById(string id);
        IEnumerable<T> Find(Func<T, bool> predicate);
        IEnumerable<T> GetAll();
        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
    }

    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create();
    }
}

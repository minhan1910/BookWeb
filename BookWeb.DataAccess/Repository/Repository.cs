using BookWeb.DataAccess.Data;
using BookWeb.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookWeb.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        internal readonly DbSet<T> _db;

        public Repository(ApplicationDbContext context)
        {
            _context = context;

            this._db = _context.Set<T>();                        
        }

        public void Add(T entity)
        {
            _db.Add(entity);
        }

        public T? Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {

            IQueryable<T> query;

            if (tracked)
            {
                query = _db;
            } else
            {
                query = _db.AsNoTracking();
            }

            query = query.Where(filter);

            if (!string.IsNullOrEmpty(includeProperties))
            {
                var includeProps = includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var includeProp in includeProps)
                {
                    query = query.Include(includeProp);
                }
            }

            return query.FirstOrDefault();
        }

        // Category, CategoryId
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<T> query = _db;

            if (filter != null)
            {
                query = query.Where(filter);    
            }

            if (!string.IsNullOrEmpty(includeProperties)) 
            {
                var includeProps = includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var includeProp in includeProps)
                {
                    query = query.Include(includeProp);
                }
            }

            return query.ToList();
        }

        public void Remove(T entity)
        {
            _db.Remove(entity);   
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _db.RemoveRange(entities);
        }
    }
}

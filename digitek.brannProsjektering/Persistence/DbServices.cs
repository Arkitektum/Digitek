using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digitek.brannProsjektering.Models;
using Newtonsoft.Json;

namespace digitek.brannProsjektering.Persistence
{
    public interface IDbServices
    {
        Task<IEnumerable<UseRecord>> GetAll();
        Task<UseRecord> AddUseRecord();

    }

    public class DbServices: IDbServices
    {
        private readonly ApplicationDbContext _context;

        public DbServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UseRecord>> GetAll()
        {
            return _context.UseRecords.ToList();
        }

        public Task<UseRecord> AddUseRecord()
        {
            throw new NotImplementedException();
        }
    }
}

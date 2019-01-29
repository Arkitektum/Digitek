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
        IEnumerable<UseRecord> GetAll();
        UseRecord AddUseRecord(UseRecord useRecord);

    }

    public class DbServices: IDbServices
    {
        private readonly ApplicationDbContext _context;

        public DbServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<UseRecord> GetAll()
        {
            return _context.UseRecords.ToList();
        }

       
        public UseRecord AddUseRecord(UseRecord useRecord)
        {
            _context.UseRecords.Add(useRecord);

            _context.SaveChanges();
            return useRecord;
        }
    }
}

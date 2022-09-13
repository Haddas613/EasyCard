using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public interface IMasavFileService
    {
        IQueryable<MasavFile> GetMasavFiles();

        IQueryable<MasavFileRow> GetMasavFileRows();

        Task<MasavFile> GetMasavFile(long masavFileID);

        Task UpdateMasavFileRow(MasavFileRow data);

        Task CreateMasavFileRow(MasavFileRow data);

        Task CreateMasavFile(MasavFile data);

        Task UpdateMasavFile(MasavFile data);

        Task<long> GenerateMasavFile(Guid? merchantID, Guid? terminalID, string institueName, int? sendingInstitute, string instituteNumber, DateTime? masavFileDate);

        Task SetMasavFilePayed(long masavFileID, long masavFileRowID);
    }
}

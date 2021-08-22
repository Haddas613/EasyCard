using AutoMapper;
using PoalimOnlineBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Transactions.Api.Mapping
{
    public class MasavFileProfile : Profile
    {
        public MasavFileProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterMasavFileMappings();
        }

        private void RegisterMasavFileMappings()
        {
            CreateMap<MasavFile, MasavDataWithdraw>();

            CreateMap<MasavFileRow, TransactionRowWithdraw>();
        }
    }
}

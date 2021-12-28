using AutoMapper;
using PoalimOnlineBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Masav;
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
            CreateMap<MasavFile, MasavDataWithdraw>()
                .ForMember(d => d.Footer, o => o.MapFrom(d => d))
                .ForMember(d => d.Header, o => o.MapFrom(d => d))
                .ForMember(d => d.Transactions, o => o.MapFrom(d => d.Rows));

            CreateMap<MasavFile, Header>()
                .ForMember(d => d.InstituteNumber, o => o.MapFrom(d => d.InstituteNumber))
                .ForMember(d => d.SendingInstitute, o => o.MapFrom(d => d.SendingInstitute))
                .ForMember(d => d.InstitueName, o => o.MapFrom(d => d.InstituteName))
                .ForMember(d => d.PaymentDate, o => o.MapFrom(d => d.MasavFileDate))
                .ForMember(d => d.CreationDate, o => o.MapFrom(d => d.MasavFileDate))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<MasavFile, Footer>()
                .ForMember(d => d.InstituteNumber, o => o.MapFrom(d => d.InstituteNumber))
                .ForMember(d => d.TotalAmount, o => o.MapFrom(d => d.TotalAmount))
                .ForMember(d => d.TransactionsCount, o => o.MapFrom(d => d.Rows.Count))
                .ForMember(d => d.PaymentDate, o => o.MapFrom(d => d.MasavFileDate))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<MasavFileRow, TransactionRowWithdraw>()
                .ForMember(d => d.InstituteNumber, o => o.MapFrom(d => d.InstituteNumber))
                .ForMember(d => d.Bankcode, o => o.MapFrom(d => d.Bankcode))
                .ForMember(d => d.BranchNumber, o => o.MapFrom(d => d.BranchNumber))
                .ForMember(d => d.BeneficiaryNname, o => o.MapFrom(d => d.ConsumerName))
                .ForMember(d => d.BankAccountNumber, o => o.MapFrom(d => d.AccountNumber))
                .ForMember(d => d.Amount, o => o.MapFrom(d => d.Amount))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Business.Entities.MasavFile, MasavFileSummary>();
            CreateMap<Business.Entities.MasavFileRow, MasavFileRowSummary>();
        }
    }
}

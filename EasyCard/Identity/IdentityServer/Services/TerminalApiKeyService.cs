using IdentityServer.Data;
using IdentityServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Business;
using Shared.Business.Exceptions;
using Shared.Business.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public class TerminalApiKeyService : ServiceBase<TerminalApiAuthKey, Guid>, ITerminalApiKeyService
    {
        private readonly ApplicationDbContext context;

        public TerminalApiKeyService(ApplicationDbContext context)
            : base(context)
        {
            this.context = context;
        }

        public async Task Delete(Guid terminalApiAuthKeyID)
        {
            var entity = context.TerminalApiAuthKeys.FirstOrDefaultAsync(t => t.TerminalApiAuthKeyID == terminalApiAuthKeyID);

            if (entity == null)
            {
                throw new EntityNotFoundException(ApiMessages.EntityNotFound, "Key", terminalApiAuthKeyID.ToString());
            }

            context.Entry(entity).State = EntityState.Deleted;
            await context.SaveChangesAsync();
        }

        public IQueryable<TerminalApiAuthKey> GetAuthKeys() => context.TerminalApiAuthKeys;
    }
}

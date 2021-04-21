using Merchants.Business.Data;
using Merchants.Business.Entities.Integration;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Entities.User;
using Merchants.Business.Models.Audit;
using Merchants.Business.Models.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json.Linq;
using Shared.Business;
using Shared.Business.Audit;
using Shared.Business.AutoHistory;
using Shared.Business.Messages;
using Shared.Business.Security;
using Shared.Helpers.Security;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace Merchants.Business.Services
{
    public class TerminalsService : ServiceBase<Terminal, Guid>, ITerminalsService
    {
        private readonly MerchantsContext context;
        private readonly ClaimsPrincipal user;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public TerminalsService(MerchantsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public IQueryable<Terminal> GetTerminals()
        {
            if (user.IsAdmin())
            {
                return context.Terminals.AsNoTracking();
            }
            else if (user.IsTerminal())
            {
                return context.Terminals.AsNoTracking().Where(t => t.TerminalID == user.GetTerminalID());
            }
            else
            {
                return context.Terminals.AsNoTracking().Where(t => t.MerchantID == user.GetMerchantID());
            }
        }

        public async Task<Terminal> GetTerminal(Guid terminalID)
        {
            var terminalQuery = GetTerminals()
                    .Include(t => t.Merchant)
                    .Where(m => m.TerminalID == terminalID);

            var integrationsQuery = await GetTerminalExternalSystems(terminalID);

            var terminal = (await terminalQuery.ToListAsync()).FirstOrDefault();

            if (terminal != null)
            {
                // futureStates is already resolved and contains the result
                terminal.Integrations = integrationsQuery.ToList();
            }

            return terminal;
        }

        //TODO: Add AsNoTracking, but make sure that saving works
        public async Task<IEnumerable<TerminalExternalSystem>> GetTerminalExternalSystems(Guid terminalID)
        {
            IQueryable<TerminalExternalSystem> query;

            if (user.IsAdmin())
            {
                query = context.TerminalExternalSystems;
            }
            else if (user.IsTerminal())
            {
                query = context.TerminalExternalSystems.Where(t => t.TerminalID == user.GetTerminalID());
            }
            else
            {
                query = context.TerminalExternalSystems.Where(t => t.Terminal.MerchantID == user.GetMerchantID());
            }

            var externalSystems = await query.Where(t => t.TerminalID == terminalID).ToListAsync();

            var shvaIntegration = externalSystems.FirstOrDefault(e => e.ExternalSystemID == ExternalSystemHelpers.ShvaExternalSystemID);

            if (shvaIntegration != null)
            {
                var settingsAsShvaTerminal = shvaIntegration.Settings.ToObject<ShvaTerminal>();

                if (settingsAsShvaTerminal != null && !string.IsNullOrWhiteSpace(settingsAsShvaTerminal.MerchantNumber)
                    && !string.IsNullOrWhiteSpace(settingsAsShvaTerminal.Password) && !string.IsNullOrWhiteSpace(settingsAsShvaTerminal.UserName))
                {
                    var shvaTerminal = await context.ShvaTerminals.FirstOrDefaultAsync(t => t.MerchantNumber == settingsAsShvaTerminal.MerchantNumber);

                    if (shvaTerminal != null)
                    {
                        shvaIntegration.Settings = JObject.FromObject(shvaTerminal);
                    }
                }
            }

            return externalSystems;
        }

        // TODO: security
        public async Task LinkUserToTerminal(UserInfo userInfo, Terminal terminal, IDbContextTransaction dbTransaction = null)
        {
            string changesStr = null;

            var existingMapping = await context.UserTerminalMappings.FirstOrDefaultAsync(m => m.UserID == userInfo.UserID && m.TerminalID == terminal.TerminalID);
            if (existingMapping != null)
            {
                existingMapping.OperationDate = DateTime.UtcNow;
                existingMapping.OperationDoneBy = user.GetDoneBy();
                existingMapping.OperationDoneByID = user.GetDoneByID();
                existingMapping.Roles = userInfo.Roles;
                existingMapping.Email = userInfo.Email;
                existingMapping.DisplayName = userInfo.DisplayName;

                List<string> changes = new List<string>();

                // Must ToArray() here for excluding the AutoHistory model.
                var entries = this.context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToArray();
                foreach (var entry in entries)
                {
                    changes.Add(entry.AutoHistory().Changed);
                }

                changesStr = string.Concat("[", string.Join(",", changes), "]");
            }
            else
            {
                context.UserTerminalMappings.Add(new Entities.User.UserTerminalMapping
                {
                    OperationDate = DateTime.UtcNow,
                    OperationDoneBy = user.GetDoneBy(),
                    OperationDoneByID = user.GetDoneByID(),
                    TerminalID = terminal.TerminalID,
                    UserID = userInfo.UserID,
                    Roles = userInfo.Roles,
                    Email = userInfo.Email,
                    DisplayName = userInfo.DisplayName
                });
            }

            var history = new MerchantHistory
            {
                OperationCode = OperationCodesEnum.UserTerminalLinkAdded,
                OperationDate = DateTime.UtcNow,
                OperationDoneBy = user?.GetDoneBy(),
                OperationDoneByID = user?.GetDoneByID(),
                TerminalID = terminal.TerminalID,
                MerchantID = terminal.MerchantID,
                OperationDescription = changesStr,
                SourceIP = httpContextAccessor.GetIP()
            };
            context.MerchantHistories.Add(history);

            if (dbTransaction != null)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }

        // TODO: security
        public async Task UnLinkUserFromTerminal(Guid userID, Guid terminalID, IDbContextTransaction dbTransaction = null)
        {
            // TODO: history

            var terminal = await context.Terminals.FirstOrDefaultAsync(t => t.TerminalID == terminalID);

            var entity = await context.UserTerminalMappings.FirstOrDefaultAsync(m => m.TerminalID == terminalID && m.UserID == userID);

            if (entity != null)
            {
                context.UserTerminalMappings.Remove(entity);

                var history = new MerchantHistory
                {
                    OperationCode = OperationCodesEnum.UserTerminalLinkRemoved,
                    OperationDate = DateTime.UtcNow,
                    OperationDoneBy = user?.GetDoneBy(),
                    OperationDoneByID = user?.GetDoneByID(),
                    TerminalID = terminalID,
                    MerchantID = terminal.MerchantID,
                    SourceIP = httpContextAccessor.GetIP()
                };
                context.MerchantHistories.Add(history);

                if (dbTransaction != null)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    using var transaction = BeginDbTransaction();
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
        }

        public async override Task UpdateEntity(Terminal entity, IDbContextTransaction dbTransaction = null)
        {
            List<string> changes = new List<string>();

            // Must ToArray() here for excluding the AutoHistory model.
            var entries = this.context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToArray();
            foreach (var entry in entries)
            {
                changes.Add(entry.AutoHistory().Changed);
            }

            var changesStr = string.Concat("[", string.Join(",", changes), "]");

            var history = new MerchantHistory
            {
                OperationCode = OperationCodesEnum.TerminalUpdated,
                OperationDate = DateTime.UtcNow,
                OperationDoneBy = user?.GetDoneBy(),
                OperationDoneByID = user?.GetDoneByID(),
                TerminalID = entity.TerminalID,
                MerchantID = entity.MerchantID,
                OperationDescription = changesStr,
                SourceIP = httpContextAccessor.GetIP()
            };
            context.MerchantHistories.Add(history);

            entity.Updated = DateTime.UtcNow;

            if (dbTransaction != null)
            {
                await base.UpdateEntity(entity, dbTransaction);
                await context.SaveChangesAsync();
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await base.UpdateEntity(entity, transaction);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }

        public async override Task CreateEntity(Terminal entity, IDbContextTransaction dbTransaction = null)
        {
            var history = new MerchantHistory
            {
                OperationCode = OperationCodesEnum.TerminalCreated,
                OperationDate = DateTime.UtcNow,
                OperationDoneBy = user?.GetDoneBy(),
                OperationDoneByID = user?.GetDoneByID(),
                MerchantID = entity.MerchantID,
                SourceIP = httpContextAccessor.GetIP()
            };

            if (dbTransaction != null)
            {
                await base.CreateEntity(entity, dbTransaction);
                history.TerminalID = entity.TerminalID;
                context.MerchantHistories.Add(history);
                await context.SaveChangesAsync();
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await base.CreateEntity(entity, dbTransaction);
                history.TerminalID = entity.TerminalID;
                context.MerchantHistories.Add(history);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }

        public async Task SaveTerminalExternalSystem(TerminalExternalSystem entity)
        {
            if (!user.IsAdmin())
            {
                throw new SecurityException("Method acces is not allowed");
            }

            if (entity.ExternalSystemID == ExternalSystemHelpers.ShvaExternalSystemID)
            {
                var settingsAsShvaTerminal = entity.Settings.ToObject<ShvaTerminal>();

                if (settingsAsShvaTerminal != null && !string.IsNullOrWhiteSpace(settingsAsShvaTerminal.MerchantNumber)
                    && !string.IsNullOrWhiteSpace(settingsAsShvaTerminal.Password) && !string.IsNullOrWhiteSpace(settingsAsShvaTerminal.UserName))
                {
                    var shvaTerminal = await context.ShvaTerminals.FirstOrDefaultAsync(t => t.MerchantNumber == settingsAsShvaTerminal.MerchantNumber);
                    if (shvaTerminal != null)
                    {
                        shvaTerminal.UserName = settingsAsShvaTerminal.UserName;
                        shvaTerminal.Password = settingsAsShvaTerminal.Password;
                    }
                    else
                    {
                        context.ShvaTerminals.Add(new ShvaTerminal
                        {
                            MerchantNumber = settingsAsShvaTerminal.MerchantNumber,
                            Password = settingsAsShvaTerminal.Password,
                            UserName = settingsAsShvaTerminal.UserName
                        });
                    }
                }
            }

            var dbEntity = (await GetTerminalExternalSystems(entity.TerminalID)).FirstOrDefault(es => es.ExternalSystemID == entity.ExternalSystemID);

            if (dbEntity == null)
            {
                entity.Created = DateTime.UtcNow;
                context.TerminalExternalSystems.Add(entity);
                await context.SaveChangesAsync();
            }
            else
            {
                dbEntity.UpdateTimestamp = entity.UpdateTimestamp;
                dbEntity.Settings = entity.Settings;
                dbEntity.Type = entity.Type;
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveTerminalExternalSystem(Guid terminalID, long externalSystemID)
        {
            if (!user.IsAdmin())
            {
                throw new SecurityException("Method acces is not allowed");
            }

            var dbEntity = (await GetTerminalExternalSystems(terminalID)).FirstOrDefault(es => es.ExternalSystemID == externalSystemID);

            if (dbEntity != null)
            {
                context.TerminalExternalSystems.Remove(dbEntity);
            }

            await context.SaveChangesAsync();
        }

        public async Task AddAuditEntry(AuditEntryData auditData)
        {
            var history = new MerchantHistory
            {
                OperationCode = auditData.OperationCode,
                OperationDate = DateTime.UtcNow,
                OperationDoneBy = user?.GetDoneBy(),
                OperationDoneByID = user?.GetDoneByID(),
                MerchantID = auditData.MerchantID,
                TerminalID = auditData.TerminalID,
                SourceIP = httpContextAccessor.GetIP(),
            };

            context.MerchantHistories.Add(history);
            await context.SaveChangesAsync();
        }
    }
}
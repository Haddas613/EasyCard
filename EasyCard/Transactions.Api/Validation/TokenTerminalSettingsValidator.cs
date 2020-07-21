using Merchants.Business.Entities.Terminal;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Tokens;
using Transactions.Shared;
using SharedHelpers = Shared.Helpers;

namespace Transactions.Api.Validation
{
    public class TokenTerminalSettingsValidator
    {
        public static void Validate(TerminalSettings terminalSettings, TokenRequest model)
        {
            if (terminalSettings == null)
            {
                throw new ApplicationException("Terminal settings is not present");
            }

            List<SharedHelpers.Error> errors = new List<SharedHelpers.Error>();

            if (terminalSettings.CvvRequired)
            {
                throw new BusinessException(Messages.CvvRequiredButStoredTokenCannotUseCvv);
            }

            if (terminalSettings.NationalIDRequired && string.IsNullOrWhiteSpace(model.CardOwnerNationalID))
            {
                throw new BusinessException(Messages.CardOwnerNationalIDRequiredButNotPresentInToken);
            }
        }
    }
}

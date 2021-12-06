using Merchants.Business.Entities.Terminal;
using Merchants.Shared.Models;
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
        public static void Validate(Terminal terminal, TokenRequest model)
        {
            if (terminal == null || terminal.Settings == null)
            {
                throw new ApplicationException("Terminal settings are not present");
            }

            if (!terminal.EnabledFeatures.Any(i => i == Merchants.Shared.Enums.FeatureEnum.CreditCardTokens))
            {
                throw new BusinessException(Messages.CreditCardTokensFeatureMustBeEnabled);
            }

            List<SharedHelpers.Error> errors = new List<SharedHelpers.Error>();

            //if (terminalSettings.CvvRequired)
            //{
            //    throw new BusinessException(Messages.CvvRequiredButStoredTokenCannotUseCvv);
            //}

            if (terminal.Settings.NationalIDRequired == true && string.IsNullOrWhiteSpace(model.CardOwnerNationalID))
            {
                throw new BusinessException(Messages.CardOwnerNationalIDRequiredButNotPresentInToken);
            }
        }
    }
}

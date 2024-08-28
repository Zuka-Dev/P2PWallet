using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Utilities
{
    public static class GLConstants
    {
        public static class Purpose
        {
            public const string WalletCreationCharges = "WalletCreationCharges";
            public const string NairaFundingWallet = "NairaFundingWallet";
            public const string ForeignWalletFunder = "ForeignWalletFunder";
        }
        public static string GetDescription(string purpose)
        {
            return purpose switch
            {
                Purpose.WalletCreationCharges => "Charges for wallet creation",
                Purpose.NairaFundingWallet => "Naira wallet for funding",
                Purpose.ForeignWalletFunder => "Foreign currency wallet for funding",
                _ => throw new ArgumentException("Invalid purpose", nameof(purpose)),
            };
        }
    }
}

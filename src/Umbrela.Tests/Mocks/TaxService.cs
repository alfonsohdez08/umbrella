using System;
using System.Collections.Generic;
using System.Text;

namespace Umbrella.Tests.Mocks
{
    public class TaxService
    {
        public decimal GetTaxes(int personId) => personId % 2 == 0 ? 5 : 10;

        public bool IsIncomingTaxSeason() => true;

        public static string GetClosestTaxCounselor() => "Redfield Services";

        public static bool IsTaxAvailable(int personId) => personId % 2 != 0;
    }
}

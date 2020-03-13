using System;
using System.Collections.Generic;
using System.Text;

namespace Umbrella.Tests.Mocks
{
    public struct Car
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public bool? IsAvailable { get; set; }
        public decimal? PriceInMarket { get; set; }

    }
}
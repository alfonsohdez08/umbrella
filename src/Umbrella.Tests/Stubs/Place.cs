namespace Umbrella.Tests.Stubs
{
    public class Place
    {
        public int? PlaceId { get; set; }
        public decimal? AverageIncoming { get; set; }
        public bool IsExpensiveArea { get; set; }
        public string Current => GetPlace();

        public string GetPlace(bool callStaticMethod = false)
        {
            if (callStaticMethod)
                return GetPlace();

            return "US";
        }

        public static string GetPlace()
        {
            return "USA";
        }
    }
}
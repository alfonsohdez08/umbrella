namespace Umbrella.Tests.Mocks
{
    // mock class
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

    //public class InvalidRowMappingException: Exception
    //{
    //    public string ObjectSerialized { get; private set; }
    //    public string RowSerialized { get; private set; }

    //    public InvalidRowMappingException(string message, object obj, (List<DataColumn>, DataRow) row): base(message)
    //    {
    //        ObjectSerialized = JsonConvert.SerializeObject(obj);
    //        RowSerialized = SerializeRow(row);
    //    }

    //    public static string SerializeRow((List<DataColumn>, DataRow) row)
    //    {
    //        Dictionary<string, object> rowDic = new Dictionary<string, object>();

    //        foreach (DataColumn c in row.Item1)
    //        {
    //            rowDic.Add(c.ColumnName, row.Item2[c]);
    //        }

    //        return JsonConvert.SerializeObject(rowDic);
    //    }
    //}
}
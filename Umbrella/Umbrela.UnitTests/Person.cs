using System;
using System.Collections.Generic;
using System.Text;

namespace Umbrella.UnitTests
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public short Age => short.Parse((DateTime.Now - DateOfBirth).ToString("yy"));
        public bool IsAlive { get; set; }
    }
}

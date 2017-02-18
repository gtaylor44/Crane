﻿using System.Collections.Generic;

namespace SprocMapperLibrary.TestCommon.Model
{
    public class President
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Fans { get; set; }
        public bool IsHonest { get; set; }
        public List<PresidentAssistant> PresidentAssistantList { get; set; }
    }
}
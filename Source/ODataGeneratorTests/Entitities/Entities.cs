using System;
using System.Collections.Generic;

namespace ODataGeneratorTests
{
    public class EntityBase
    {
        public Guid Id { get; set; }
        public Status Status { get; set; }
    }


    public class Record : EntityBase
    {
        public int Number { get; set; }
        public IList<Language> Languages { get; set; }
        public bool IsActive { get; set; }
    }

    public class Language : EntityBase
    {
        public string LanguageName { get; set; }
        public IList<Term> Terms { get; set; }
    }

    public class Term : EntityBase
    {
        public string Text { get; set; }
    }

    public enum Status
    {
        Active,
        Inactive
    }
}
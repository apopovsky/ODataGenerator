using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ODataGenerator;

namespace ODataGeneratorTests
{
    [TestClass]
    public class FilterGeneratorTests
    {
        private FilterGenerator<Record> _filterGenerator;

        [TestInitialize]
        public void Init()
        {
            _filterGenerator = new FilterGenerator<Record>();
        }

        [TestMethod]
        public void Generate_SimpleExpression_ValidODataGenerated()
        {
            var generated = _filterGenerator.Generate(record => record.Status == Status.Active);
            generated.Should().Be("Status eq 0");
        }
        
        [TestMethod]
        public void Generate_SimpleAndExpression_ValidODataGenerated()
        {
            var generated = _filterGenerator.Generate(record => record.Status == Status.Active && record.Status != Status.Inactive);
            generated.Should().Be("Status eq 0 and Status ne 1");
        }

        [TestMethod]
        public void Generate_SimpleOrExpression_ValidODataGenerated()
        {
            var generated = _filterGenerator.Generate(record => record.Status == Status.Active || record.Status != Status.Inactive);
            generated.Should().Be("Status eq 0 or Status ne 1");
        }

        [TestMethod]
        public void Generate_SimpleExpressionWithObjectField_ValidODataGenerated()
        {
            var obj = new {Number = 1900};
            var generated = _filterGenerator.Generate(record => record.Number == obj.Number);
            generated.Should().Be($"Number eq {obj.Number}");
        }

        [TestMethod]
        public void Generate_SimpleExpressionWithVariable_ValidODataGenerated()
        {
            var guid = new Guid("AE8A3A67-F0EF-4FE0-AECF-DA1B247B7777");
            var generated = _filterGenerator.Generate(record =>
                record.Id == guid);
            generated.Should().Be("Id eq 'ae8a3a67-f0ef-4fe0-aecf-da1b247b7777'");
        }

        [TestMethod]
        public void Generate_SimpleExpressionWithParentheses_ValidODataGenerated()
        {
            var guid = new Guid("AE8A3A67-F0EF-4FE0-AECF-DA1B247B7777");
            var generated = _filterGenerator.Generate(record => 
                (record.Status == Status.Active && record.Id == guid ||
                                                                record.Status == Status.Inactive) &&
                                                               record.Id == Guid.Empty);
            //TODO: ideally this should return 
            //(Status eq 0 and Id eq guid or Status eq 1) and Id eq '00000000-0000-0000-0000-000000000000'
            generated.Should().Be("((Status eq 0 and Id eq 'ae8a3a67-f0ef-4fe0-aecf-da1b247b7777') or Status eq 1) and Id eq '00000000-0000-0000-0000-000000000000'");
        }

        [TestMethod]
        public void Generate_SimpleExpressionWithLiterals_ValidODataGenerated()
        {
            var generated = _filterGenerator.Generate(record => record.Id==Guid.Empty);
            generated.Should().Be("Id eq '00000000-0000-0000-0000-000000000000'");
        }

        [TestMethod]
        public void Generate_CollectionAnyQuery_ValidODataGenerated()
        {
            var generated = _filterGenerator.Generate(record => record.Languages.Any(lang=>lang.LanguageName == "English"));
            generated.Should().Be("Languages/any(lang:lang/LanguageName eq 'English')");
        }
        
        [TestMethod]
        public void Generate_InnerCollectionQuery_ValidODataGenerated()
        {
            var generated = _filterGenerator.Generate(record => 
                record.Languages.Any(lang=>lang.LanguageName == "English" && 
                                           lang.Terms.All(term=>term.Text=="Term1")));
            generated.Should().Be("Languages/any(lang:lang/LanguageName eq 'English' and lang/Terms/all(term:term/Text eq 'Term1'))");
        }

        [TestMethod, Ignore]
        public void Generate_CollectionQueryWithExternalPredicate_ValidODataGenerated()
        {
            Func<Language, bool> predicate = language => language.LanguageName == "English";
            var generated = _filterGenerator.Generate(record =>
                record.Languages.Any(predicate));
            generated.Should().Be("Languages/any(lang:lang/LanguageName eq 'English')");
        }

        [TestMethod]
        public void Generate_SimpleQueryWithVariableAndValueConversion_ValidODataGenerated()
        {
            var statusValue = Status.Active;
            var generated = _filterGenerator.Generate(record => record.Number == (int) statusValue);
            generated.Should().Be("Number eq 0");
        }

        [TestMethod]
        public void Generate_SimpleQueryWithBooleanPropertyUnaryFilter_ValidODataGenerated()
        {
            var generated = _filterGenerator.Generate(record => record.IsActive);
            generated.Should().Be("IsActive eq true");
        }
        
        [TestMethod]
        public void Generate_SimpleQueryWithNegatedBooleanPropertyUnaryFilter_ValidODataGenerated()
        {
            var generated = _filterGenerator.Generate(record => !record.IsActive);
            generated.Should().Be("IsActive eq false");
        }
    }

}
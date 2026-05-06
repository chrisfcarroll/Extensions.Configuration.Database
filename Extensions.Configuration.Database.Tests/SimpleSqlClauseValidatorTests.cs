using TestBase;

namespace Extensions.Configuration.Database.Tests;

[TestClass]
public class SimpleSqlClauseValidatorTests
{
    [TestMethod]
    [DataRow("Archived = 0")]
    [DataRow("Id = 123")]
    [DataRow("  Id\t=\r\n'abc'  ")]
    [DataRow("_id=-42")]
    [DataRow("Amount = +12.34")]
    [DataRow("[Column Name] = 'value'")]
    [DataRow("[Column]]Name] != 'value'")]
    [DataRow("\"Column Name\" = 'value'")]
    [DataRow("\"Column\"\"Name\" > 0")]
    [DataRow("Name = 'O''Brien'")]
    [DataRow("Value < .5")]
    [DataRow("Value > 5.")]
    [DataRow("Id = ''")]
    public void IsSafeEqualsClause_AcceptsValidSimpleClauses(string clause)
    {
        BoolBecause result = SimpleSqlClauseValidator.IsASimpleIdentifierOperatorValueExpression(clause);

        result.Value.ShouldBeTrue(result.ToString());
        result.Reason.ShouldBe("");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("1Id = 123")]
    [DataRow(".Id = 123")]
    [DataRow("[Unclosed = 123")]
    [DataRow("\"Unclosed = 123")]
    public void IsSafeEqualsClause_RejectsInvalidIdentifiers(string clause)
    {
        BoolBecause result = SimpleSqlClauseValidator.IsASimpleIdentifierOperatorValueExpression(clause);

        result.Value.ShouldBeFalse();
        result.Reason.ShouldBe("Invalid identifier");
    }

    [TestMethod]
    [DataRow("Id")]
    [DataRow("Id 'abc'")]
    [DataRow("Id ! 'abc'")]
    [DataRow("[Column]Name = 1")]
    [DataRow("\"Column\"Name = 1")]
    public void IsSafeEqualsClause_RejectsMissingOrUnsupportedComparisonOperator(
        string clause)
    {
        BoolBecause result = SimpleSqlClauseValidator.IsASimpleIdentifierOperatorValueExpression(clause);

        result.Value.ShouldBeFalse();
        result.Reason.ShouldBe("No comparison operator");
    }

    [TestMethod]
    [DataRow("Id =")]
    [DataRow("Id = ")]
    [DataRow("Id = abc")]
    [DataRow("Id = +")]
    [DataRow("Id = -")]
    [DataRow("Id = .")]
    [DataRow("Id = 'unterminated")]
    public void IsSafeEqualsClause_RejectsInvalidValues(string clause)
    {
        BoolBecause result = SimpleSqlClauseValidator.IsASimpleIdentifierOperatorValueExpression(clause);

        result.Value.ShouldBeFalse();
        result.Reason.ShouldBe("Invalid value");
    }

    [TestMethod]
    [DataRow("Id = 1 OR 1 = 1")]
    [DataRow("Id = 1; DROP TABLE Users")]
    [DataRow("Id = 'abc' -- comment")]
    [DataRow("Id = 'abc' /* comment */")]
    [DataRow("Id = 1abc")]
    [DataRow("Id = 'abc' 'def'")]
    public void IsSafeEqualsClause_RejectsUnexpectedTrailingCharacters(string clause)
    {
        BoolBecause result = SimpleSqlClauseValidator.IsASimpleIdentifierOperatorValueExpression(clause);

        result.Value.ShouldBeFalse();
        result.Reason.ShouldBe("unexpected characters after value");
    }

    [TestMethod]
    [DataRow("Id != 1")]
    [DataRow("Id < 1")]
    [DataRow("Id > 1")]
    [DataRow("Id = 1")]
    public void IsSafeEqualsClause_AcceptsSupportedComparisonOperators(string clause)
    {
        BoolBecause result = SimpleSqlClauseValidator.IsASimpleIdentifierOperatorValueExpression(clause);

        result.Value.ShouldBeTrue(result.ToString());
    }
}
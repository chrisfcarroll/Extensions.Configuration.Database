using TestBase;

namespace Extensions.Configuration.Database.Tests;

[TestClass]
public class BoolBecauseTests
{

    [TestMethod]
    public void BoolBecause_ImplicitlyConvertsToBool()
    {
        BoolBecause success = new(true);
        BoolBecause failure = new(false, "failed");

        bool successValue = success;
        bool failureValue = failure;

        successValue.ShouldBeTrue();
        failureValue.ShouldBeFalse();
    }

    [TestMethod]
    public void BoolBecause_ImplicitlyConvertsFromBool()
    {
        BoolBecause success = true;
        BoolBecause failure = false;

        success.Value.ShouldBeTrue();
        success.Reason.ShouldBe("");

        failure.Value.ShouldBeFalse();
        failure.Reason.ShouldBe("");
    }

    [TestMethod]
    [DataRow(true, "", "True")]
    [DataRow(false, "", "False")]
    [DataRow(true, "all good", "True because all good")]
    [DataRow(false, "bad input", "False because bad input")]
    public void BoolBecause_ToStringIncludesReasonOnlyWhenPresent(
        bool value,
        string reason,
        string expected)
    {
        var result = new BoolBecause(value, reason);

        result.ToString().ShouldBe(expected);
    }

    [TestMethod]
    [DataRow(true, "", true, "", true, "")]
    [DataRow(true, "left ok", true, "right ok", true, "left ok and right ok")]
    [DataRow(true, "left ok", false, "right failed", false, "left ok and right failed")]
    [DataRow(false, "left failed", true, "right ok", false, "left failed and right ok")]
    [DataRow(false, "left failed", false, "right failed", false, "left failed and right failed")]
    [DataRow(false, "", false, "right failed", false, "right failed")]
    [DataRow(false, "left failed", false, "", false, "left failed")]
    public void BoolBecause_AndOperatorCombinesValuesAndReasons(
        bool leftValue,
        string leftReason,
        bool rightValue,
        string rightReason,
        bool expectedValue,
        string expectedReason)
    {
        var left = new BoolBecause(leftValue, leftReason);
        var right = new BoolBecause(rightValue, rightReason);

        BoolBecause result = left & right;

        result.Value.ShouldBe(expectedValue);
        result.Reason.ShouldBe(expectedReason);
    }

    [TestMethod]
    [DataRow(true, "", true, "", true, "")]
    [DataRow(true, "left ok", true, "right ok", true, "left ok or right ok")]
    [DataRow(true, "left ok", false, "right failed", true, "left ok or right failed")]
    [DataRow(false, "left failed", true, "right ok", true, "left failed or right ok")]
    [DataRow(false, "left failed", false, "right failed", false, "left failed or right failed")]
    [DataRow(false, "", false, "right failed", false, "right failed")]
    [DataRow(false, "left failed", false, "", false, "left failed")]
    public void BoolBecause_OrOperatorCombinesValuesAndReasons(
        bool leftValue,
        string leftReason,
        bool rightValue,
        string rightReason,
        bool expectedValue,
        string expectedReason)
    {
        var left = new BoolBecause(leftValue, leftReason);
        var right = new BoolBecause(rightValue, rightReason);

        BoolBecause result = left | right;

        result.Value.ShouldBe(expectedValue);
        result.Reason.ShouldBe(expectedReason);
    }

    [TestMethod]
    public void BoolBecause_ConditionalAndShortCircuitsWhenLeftIsFalse()
    {
        var rightWasEvaluated = false;

        BoolBecause left = new(false, "left failed");
        BoolBecause right()
        {
            rightWasEvaluated = true;
            return new BoolBecause(true, "right ok");
        }

        BoolBecause result = left && right();

        rightWasEvaluated.ShouldBeFalse();
        result.Value.ShouldBeFalse();
        result.Reason.ShouldBe("left failed");
    }

    [TestMethod]
    public void BoolBecause_ConditionalAndEvaluatesRightWhenLeftIsTrue()
    {
        var rightWasEvaluated = false;

        BoolBecause left = new(true, "left ok");
        BoolBecause right()
        {
            rightWasEvaluated = true;
            return new BoolBecause(false, "right failed");
        }

        BoolBecause result = left && right();

        rightWasEvaluated.ShouldBeTrue();
        result.Value.ShouldBeFalse();
        result.Reason.ShouldBe("left ok and right failed");
    }

    [TestMethod]
    public void BoolBecause_ConditionalOrShortCircuitsWhenLeftIsTrue()
    {
        var rightWasEvaluated = false;

        BoolBecause left = new(true, "left ok");
        BoolBecause right()
        {
            rightWasEvaluated = true;
            return new BoolBecause(false, "right failed");
        }

        BoolBecause result = left || right();

        rightWasEvaluated.ShouldBeFalse();
        result.Value.ShouldBeTrue();
        result.Reason.ShouldBe("left ok");
    }

    [TestMethod]
    public void BoolBecause_ConditionalOrEvaluatesRightWhenLeftIsFalse()
    {
        var rightWasEvaluated = false;

        BoolBecause left = new(false, "left failed");
        BoolBecause right()
        {
            rightWasEvaluated = true;
            return new BoolBecause(true, "right ok");
        }

        BoolBecause result = left || right();

        rightWasEvaluated.ShouldBeTrue();
        result.Value.ShouldBeTrue();
        result.Reason.ShouldBe("left failed or right ok");
    }    
}
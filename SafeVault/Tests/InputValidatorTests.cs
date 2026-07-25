using Xunit;
using SafeVault.Services;

namespace SafeVault.Tests
{
    // these mostly target the stuff that broke during the debugging pass -
    // sql injection strings and xss payloads getting through
    public class InputValidatorTests
    {
        [Theory]
        [InlineData("'; DROP TABLE Users; --")]
        [InlineData("1' OR '1'='1")]
        [InlineData("admin' --")]
        [InlineData("UNION SELECT * FROM Users")]
        public void ContainsSqlInjectionAttempt_DetectsCommonPayloads(string input)
        {
            Assert.True(InputValidator.ContainsSqlInjectionAttempt(input));
        }

        [Theory]
        [InlineData("just a normal note about groceries")]
        [InlineData("meeting notes for 3/1")]
        [InlineData("password reminder: check the blue folder")]
        public void ContainsSqlInjectionAttempt_AllowsNormalInput(string input)
        {
            Assert.False(InputValidator.ContainsSqlInjectionAttempt(input));
        }

        [Fact]
        public void SanitizeForOutput_EncodesScriptTags()
        {
            var input = "<script>alert('xss')</script>";
            var result = InputValidator.SanitizeForOutput(input);

            Assert.DoesNotContain("<script>", result);
            Assert.Contains("&lt;script&gt;", result);
        }

        [Theory]
        [InlineData("valid_user.123", true)]
        [InlineData("ab", false)] // too short
        [InlineData("user name with spaces", false)]
        [InlineData("<script>", false)]
        public void IsValidUsername_ValidatesCorrectly(string username, bool expected)
        {
            Assert.Equal(expected, InputValidator.IsValidUsername(username));
        }
    }
}

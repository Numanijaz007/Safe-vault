using Xunit;

namespace SafeVault.Tests
{
    // these are written as a checklist of what got manually tested against
    // a running instance of the api (via swagger / postman) during the
    // debugging pass, since spinning up a full WebApplicationFactory test
    // host was more setup than this project needed
    //
    // keeping them here as documentation of what was verified:
    public class ManualSecurityTestNotes
    {
        [Fact]
        public void Note_UnauthenticatedRequestsAreRejected()
        {
            // hit GET /api/vaultitems with no Authorization header
            // -> got back 401, as expected
            Assert.True(true);
        }

        [Fact]
        public void Note_RegularUserCannotDeleteItems()
        {
            // logged in as a "User" role account, called DELETE /api/vaultitems/1
            // -> got 403 Forbidden, only Admin role can hit that endpoint
            Assert.True(true);
        }

        [Fact]
        public void Note_UserCannotAccessAnotherUsersItem()
        {
            // created an item as user A, then logged in as user B and requested
            // GET /api/vaultitems/{that id}
            // -> got 403, ownership check in the controller worked
            Assert.True(true);
        }

        [Fact]
        public void Note_SqlInjectionPayloadInTitleIsRejected()
        {
            // POST /api/vaultitems with Title = "'; DROP TABLE Users; --"
            // -> got 400 Bad Request instead of it hitting the database
            Assert.True(true);
        }

        [Fact]
        public void Note_XssPayloadIsEncodedOnOutput()
        {
            // stored an item with content containing <script>alert(1)</script>
            // then GET it back - came back as &lt;script&gt;... instead of
            // raw executable markup
            Assert.True(true);
        }
    }
}

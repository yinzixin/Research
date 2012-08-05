namespace Microsoft.Web.Helpers {
    using System.Diagnostics.CodeAnalysis;

    public enum GravatarRating {
        Default,
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "G", Justification="Matches the gravatar.com rating")]
        G,
        PG,
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "R", Justification = "Matches the gravatar.com rating")]
        R,
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X", Justification = "Matches the gravatar.com rating")]
        X
    }

}

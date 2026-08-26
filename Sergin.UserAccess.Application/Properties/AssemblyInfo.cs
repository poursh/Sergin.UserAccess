using System.Runtime.CompilerServices;

// The composition root registers ExternalIdentityResolver, which is internal for the same reason every
// handler here is: nothing outside this module should construct it.
[assembly: InternalsVisibleTo("Sergin.UserAccess")]

namespace Lib.Cache.Enums;

//
// Summary:
//     Indicates when this operation should be performed (only some variations are legal
//     in a given context).
public enum When
{
    //
    // Summary:
    //     The operation should occur whether or not there is an existing value.
    Always,
    //
    // Summary:
    //     The operation should only occur when there is an existing value.
    Exists,
    //
    // Summary:
    //     The operation should only occur when there is not an existing value.
    NotExists
}
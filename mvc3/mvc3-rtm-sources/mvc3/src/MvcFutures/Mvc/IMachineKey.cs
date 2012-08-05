namespace Microsoft.Web.Mvc {
    using System;
    using System.Web.Security;

    // Used for mocking out the static MachineKey type

    internal interface IMachineKey {

        byte[] Decode(string encodedData, MachineKeyProtection protectionOption);
        string Encode(byte[] data, MachineKeyProtection protectionOption);

    }
}

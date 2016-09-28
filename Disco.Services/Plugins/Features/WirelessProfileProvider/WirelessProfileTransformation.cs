namespace Disco.Services.Plugins.Features.WirelessProfileProvider
{
    /// <summary>
    /// A transform to be applied to the matching <see cref="Name"/>.
    /// </summary>
    public class WirelessProfileTransformation
    {

        /// <summary>
        /// The name of the wireless profile related to this transformation, typically the SSID
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The regular expression to evaluate against the wireless profile XML
        /// </summary>
        public string RegularExpression { get; set; }

        /// <summary>
        /// The replacement string used when evaluating the regular expression
        /// </summary>
        public string RegularExpressionReplacement { get; set; }

    }
}

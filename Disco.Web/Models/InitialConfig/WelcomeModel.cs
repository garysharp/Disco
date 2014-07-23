using Disco.Services.Interop.VicEduDept;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Disco.Web.Models.InitialConfig
{
    public class WelcomeModel
    {
        [Required(ErrorMessage = "The Organisation Name is required.")]
        public string OrganisationName { get; set; }


        private static string _OrganisationNameCache;
        public bool AutodetectOrganisation()
        {
            if (_OrganisationNameCache != null)
            {
                this.OrganisationName = _OrganisationNameCache;
                return true;
            }

            var whoAmIResult = VicSmart.WhoAmI();

            if (whoAmIResult != null && !string.IsNullOrWhiteSpace(whoAmIResult.Item2))
            {
                this.OrganisationName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(whoAmIResult.Item2);
                _OrganisationNameCache = this.OrganisationName;

                return true;
            }

            return false;
        }

    }
}
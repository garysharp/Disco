using Disco.Models.Services.Searching;
using Disco.Services.Authorization;
using Disco.Services.Searching;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class SearchController : AuthorizedDatabaseController
    {
        [DiscoAuthorizeAny(Claims.Job.Search, Claims.Device.Search, Claims.User.Search)]
        public virtual ActionResult QuickQuery(string Term, int Limit = 15)
        {
            if (string.IsNullOrWhiteSpace(Term))
                throw new ArgumentNullException("Term", "The search query term is required");
            if (Term.Length < 2)
                throw new ArgumentException("The search query term must be at least two characters", "Term");
            if (Limit < 1)
                throw new ArgumentException("The search query limit cannot be less than 1", "Limit");

            IEnumerable<ISearchResultItem> results = Enumerable.Empty<ISearchResultItem>();

            if (Authorization.Has(Claims.Job.Search))
            {
                var jobMatches = Search.SearchJobs(Database, Term, Limit);
                results = results.Concat(jobMatches);
            }

            if (Authorization.Has(Claims.User.Search))
            {
                var userMatches = Search.SearchUsers(Database, Term, Limit);
                results = results.Concat(userMatches);
            }

            if (Authorization.Has(Claims.Device.Search))
            {
                var deviceMatches = Search.SearchDevices(Database, Term, Limit);
                results = results.Concat(deviceMatches);
            }

            results = results.OrderByDescending(i => i.ScoreValue.Score(Term, .5)).Take(Limit);

            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }
}

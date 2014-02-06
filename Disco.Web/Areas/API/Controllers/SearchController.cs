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

            switch (Term[0])
            {
                case '!': // Device Only
                    if (Authorization.Has(Claims.Device.Search))
                        results = results.Concat(Search.SearchDevices(Database, Term.Substring(1), Limit));
                    break;
                case '#': // Job Only
                    if (Authorization.Has(Claims.Job.Search))
                        results = results.Concat(Search.SearchJobs(Database, Term.Substring(1), Limit));
                    break;
                case '@': // User Only
                    if (Authorization.Has(Claims.User.Search))
                        results = results.Concat(Search.SearchUsers(Database, Term.Substring(1), Limit));
                    break;
                default: // Search All
                    if (Authorization.Has(Claims.Job.Search))
                        results = results.Concat(Search.SearchJobs(Database, Term, Limit));
                    if (Authorization.Has(Claims.User.Search))
                        results = results.Concat(Search.SearchUsers(Database, Term, Limit));
                    if (Authorization.Has(Claims.Device.Search))
                        results = results.Concat(Search.SearchDevices(Database, Term, Limit));

                    break;
            }

            results = results.OrderByDescending(i => i.ScoreValue.Score(Term)).Take(Limit);

            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }
}

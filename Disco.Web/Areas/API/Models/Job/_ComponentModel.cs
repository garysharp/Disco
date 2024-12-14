namespace Disco.Web.Areas.API.Models.Job
{
    public class _ComponentModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Cost { get; set; }

        public static _ComponentModel FromJobComponent(Disco.Models.Repository.JobComponent jc)
        {
            return new _ComponentModel
            {
                Id = jc.Id,
                Description = jc.Description,
                Cost = jc.Cost.ToString("C")
            };
        }
    }
}
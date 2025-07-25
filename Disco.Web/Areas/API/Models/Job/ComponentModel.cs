namespace Disco.Web.Areas.API.Models.Job
{
    public class ComponentModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Cost { get; set; }

        public static ComponentModel FromJobComponent(Disco.Models.Repository.JobComponent jc)
        {
            return new ComponentModel
            {
                Id = jc.Id,
                Description = jc.Description,
                Cost = jc.Cost.ToString("C")
            };
        }
    }
}
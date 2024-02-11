using Microsoft.AspNetCore.Mvc.Rendering;

namespace PatientInformation.Models
{
    public enum EpilepsyStatus
    {
        Yes,
        No
    }

    public class PatientViewModel
    {
        public int PatientID { get; set; }
        public string PatientName { get; set; }
        public string NCDID { get; set; }
        public int NCDDetailsId { get; set; }
        public int AllergiesDetailsID { get; set; }
        public string NCDName { get; set; }
        public int DiseaseID { get; set; }
        public string DiseaseName { get; set; }
        public string AllergiesID { get; set; }
        public string AllergiesName { get; set; }
        public string Epilepsy { get; set; }
        public EpilepsyStatus EpilepsyEnum { get; set; }

        public List <SelectListItem> DiseaseList { get; set; }
        public List <SelectListItem> NCDList { get; set; }
        public List <SelectListItem> AllergiesList { get; set; }
       
    }
}

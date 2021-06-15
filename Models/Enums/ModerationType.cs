using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models.Enums
{
    public enum ModerationType
    {
        [Display(Name = "Political propaganda")]
        PoliticalPropaganda,
        [Display(Name = "Offensive language")]
        Language,
        [Display(Name = "Drug references")]
        Drugs,
        [Display(Name = "Threatening speech")]
        Threatening,
        [Display(Name = "Sexual content")]
        Sexual,
        [Display(Name = "Hate speech")]
        HateSpeech,
        [Display(Name = "Targeted shaming")]
        Shaming,
        [Display(Name = "Fraud")]
        Fraud
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Disco.Data.Repository;
using Disco.Models.UI.Job;

namespace Disco.Web.Models.Job
{
    [CustomValidation(typeof(CreateModel), "ValidateCreateModel")]
    public class CreateModel : JobCreateModel
    {
        public string DeviceSerialNumber { get; set; }
        public string UserId { get; set; }

        [Required]
        public string Type { get; set; }
        [Required]
        public List<string> SubTypes { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string Comments { get; set; }

        [Required(ErrorMessage = "Please specify whether the device is held or not")]
        public bool? DeviceHeld { get; set; }

        public string QuickLogDestinationUrl { get; set; }

        [Display(Description = "Automatically close this job")]
        public bool? QuickLog { get; set; }
        public int? QuickLogTaskTimeMinutes { get; set; }
        public int? QuickLogTaskTimeMinutesOther { get; set; }

        #region Helpers & Model Logic
        // View Required Data
        public Disco.Models.Repository.Device Device { get; set; }
        public Disco.Models.Repository.User User { get; set; }
        public List<Disco.Models.Repository.JobType> JobTypes { get; set; }
        public List<Disco.Models.Repository.JobSubType> JobSubTypes { get; set; }
        public void UpdateModel(DiscoDataContext dbContext)
        {
            if (this.JobTypes == null)
                JobTypes = dbContext.JobTypes.ToList();
            if (this.JobSubTypes == null)
                JobSubTypes = dbContext.JobSubTypes.ToList();

            if (!string.IsNullOrEmpty(DeviceSerialNumber))
            {
                this.Device = dbContext.Devices.Include("DeviceModel").Where(d => d.SerialNumber == DeviceSerialNumber).FirstOrDefault();
                if (this.Device == null)
                {
                    throw new ArgumentException("Invalid Device Serial Number Specified", "DeviceSerialNumber");
                }
                if (string.IsNullOrEmpty(this.UserId) && !string.IsNullOrEmpty(this.Device.AssignedUserId))
                {
                    this.UserId = this.Device.AssignedUserId;
                }
                if (string.IsNullOrEmpty(this.Type))
                    this.Type = this.JobTypes.First(jt => jt.Id == Disco.Models.Repository.JobType.JobTypeIds.HWar).Id;

                if (string.IsNullOrEmpty(this.UserId))
                {
                    // No User - Remove User Types
                    foreach (var jobType in JobTypes.ToArray())
                    {
                        switch (jobType.Id)
                        {
                            case Disco.Models.Repository.JobType.JobTypeIds.UMgmt:
                                JobTypes.Remove(jobType);
                                JobSubTypes.RemoveAll(jst => jst.JobType == jobType);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else
            {
                // No Device - Remove Hardware Types
                foreach (var jobType in JobTypes.ToArray())
                {
                    switch (jobType.Id)
                    {
                        case Disco.Models.Repository.JobType.JobTypeIds.HMisc:
                        case Disco.Models.Repository.JobType.JobTypeIds.HNWar:
                        case Disco.Models.Repository.JobType.JobTypeIds.HWar:
                        case Disco.Models.Repository.JobType.JobTypeIds.SImg:
                            JobTypes.Remove(jobType);
                            JobSubTypes.RemoveAll(jst => jst.JobType == jobType);
                            break;
                        default:
                            break;
                    }
                }

                // Set Default Job Type for Users
                if (string.IsNullOrEmpty(this.Type))
                    this.Type = this.JobTypes.First(jt => jt.Id == Disco.Models.Repository.JobType.JobTypeIds.SApp).Id;
            }
            if (!string.IsNullOrEmpty(UserId))
            {
                this.User = dbContext.Users.Find(UserId);
                if (this.User == null)
                {
                    throw new ArgumentException("Invalid User Id Specified", "UserId");
                }
                if (string.IsNullOrEmpty(this.Type))
                    this.Type = Disco.Models.Repository.JobType.JobTypeIds.SApp;
            }
            if (this.User == null && this.Device == null)
            {
                throw new InvalidOperationException("A Job must reference a Device and/or a User");
            }
        }

        // Job Type Helpers
        public Disco.Models.Repository.JobType GetJobType
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Type))
                {
                    return this.JobTypes.FirstOrDefault(m => m.Id == this.Type);
                }
                return null;
            }
        }
        public List<Disco.Models.Repository.JobSubType> GetJobSubTypes
        {
            get
            {
                if (SubTypes != null)
                {
                    var subTypes = this.SubTypes;
                    return this.JobSubTypes.Where(m => subTypes.Contains(String.Format("{0}_{1}", m.JobTypeId, m.Id))).ToList();
                }
                return null;
            }
        }

        public static ValidationResult ValidateCreateModel(CreateModel model)
        {
            // Device && User both can't be null
            if (string.IsNullOrEmpty(model.DeviceSerialNumber) && string.IsNullOrEmpty(model.UserId))
                return new ValidationResult("A Job must reference a Device and/or a User");

            if (!string.IsNullOrEmpty(model.Type) && model.SubTypes != null)
            {
                var typeId = string.Format("{0}_", model.Type);
                model.SubTypes = model.SubTypes.Where(m => m.StartsWith(typeId)).ToList();
                if (model.SubTypes.Count == 0)
                {
                    model.SubTypes = null;
                    return new ValidationResult("At least one Sub Type is required", new string[] { "SubTypes" });
                }
            }

            // Enforce Behaviour
            if (model.DeviceHeld.HasValue && model.DeviceHeld.Value)
            {
                model.QuickLog = false;
            }
            else
            {
                if (model.QuickLog.HasValue && model.QuickLog.Value)
                {
                    if (!model.QuickLogTaskTimeMinutes.HasValue || model.QuickLogTaskTimeMinutes.Value <= 0)
                        if (model.QuickLogTaskTimeMinutesOther.HasValue && model.QuickLogTaskTimeMinutesOther.Value > 0)
                            model.QuickLogTaskTimeMinutes = model.QuickLogTaskTimeMinutesOther.Value;
                        else
                            model.QuickLogTaskTimeMinutes = 10; // Default to 10 Minutes
                }
            }

            return ValidationResult.Success;
        }
        #endregion

    }
}
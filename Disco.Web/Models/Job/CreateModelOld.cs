using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Models;
using System.ComponentModel.DataAnnotations;
using Disco.Data.Repository;

namespace Disco.Web.Models.Job
{
    [CustomValidation(typeof(CreateModelValidation), "ValidateCreateModel")]
    public class CreateModelOld
    {
        private Disco.Models.Repository.Device _Device;
        private Disco.Models.Repository.User _User;

        public Disco.Models.Repository.Device Device
        {
            get
            {
                return _Device;
            }
            set
            {
                _Device = value;
                DeviceSerialNumber = value.SerialNumber;
            }
        }
        public Disco.Models.Repository.User User
        {
            get
            {
                return _User;
            }
            set
            {
                _User = value;
                UserId = value.Id;
            }
        }

        public string DeviceSerialNumber { get; set; }
        public string UserId { get; set; }

        [Required]
        public string Type { get; set; }
        [Required]
        public List<string> SubTypes { get; set; }

        public List<Disco.Models.Repository.JobType> JobTypes { get; set; }
        public List<Disco.Models.Repository.JobSubType> JobSubTypes { get; set; }

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
                    this.Type = this.JobTypes.First(jt => jt.Id == "HWar").Id;
            }
            else
            {
                // No Device - Remove Hardware Types
                foreach (var jobType in JobTypes.ToArray())
                {
                    if (jobType.Id != Disco.Models.Repository.JobType.JobTypeIds.SApp)
                    {
                        JobTypes.Remove(jobType);
                        JobSubTypes.RemoveAll(jst => jst.JobType == jobType);
                    }
                }
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

    }

    public class CreateModelValidation
    {

        public static ValidationResult ValidateCreateModel(CreateModelOld model)
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

            return ValidationResult.Success;
        }

    }

}
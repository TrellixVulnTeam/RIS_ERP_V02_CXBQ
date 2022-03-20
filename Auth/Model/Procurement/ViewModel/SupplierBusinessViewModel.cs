﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Auth.Utility.Procurement.Enum.GlobalEnumList;

namespace Auth.Model.Procurement.ViewModel
{
    public class SupplierBusinessViewModel
    {
        public int SupplierId { get; set; }
        public int BusinessActivityEnumId { get; set; }
        public string BusinessActivityEnumName { get; set; }

        public string ManagementStaffNo { get; set; }
        public string NonmanagementStaffNo { get; set; }
        public string PermanentWorkerNo { get; set; }
        public string CasualWorkerNo { get; set; }

        public static SupplierBusinessViewModel ConvertToSupplierBusinessAllModel(dynamic SupplierBusiness)
        {
            var model = new SupplierBusinessViewModel();
            model.BusinessActivityEnumId = SupplierBusiness.business_activities_enum_id ?? 0;
            model.BusinessActivityEnumName = EnumDisplayBusinessActivity.GetDisplayBusinessActivity((EnumBusinessActivities)SupplierBusiness.business_activities_enum_id);
            model.ManagementStaffNo = SupplierBusiness.management_staff_no ?? "";
            model.NonmanagementStaffNo = SupplierBusiness.nonmanagement_staff_no ?? "";
            model.PermanentWorkerNo = SupplierBusiness.permanent_worker_no ?? "";
            model.CasualWorkerNo = SupplierBusiness.casual_worker_no ?? "";


            return model;

        }

    }

    public static class EnumDisplayBusinessActivity
    {
        public static string GetDisplayBusinessActivity(this Enum enumValue)
        {
            return enumValue.GetType()?
                            .GetMember(enumValue.ToString())?
                            .First()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name;
        }
    }

}

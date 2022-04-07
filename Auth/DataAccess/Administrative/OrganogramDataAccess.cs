﻿using Auth.Model.Administrative.Model;
using Auth.Model.Administrative.ViewModel;
using Auth.Utility;
using Auth.Utility.Administrative.Model;
using Auth.Utility.Administrative.Enum;
using Dapper;
using DataAccess;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.DataAccess.Administrative
{ 
    public class OrganogramDataAccess
    {
        private readonly IDbConnection _dbConnection;

        IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        protected readonly ApplicationDBContext _context;

        public OrganogramDataAccess(ApplicationDBContext context, IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            _context = context;
        }
         
        //Parameter Binding
        public DynamicParameters OrganogramParameterBinding(Organogram organogram, int operationType)
        {
            var currentUserInfoId = _httpContextAccessor.HttpContext.Items["User_Info_Id"];
            var company_id = _httpContextAccessor.HttpContext.Items["company_id"];
            var company_corporate_id = _httpContextAccessor.HttpContext.Items["company_corporate_id"];
            var company_group_id = _httpContextAccessor.HttpContext.Items["company_group_id"];

            DynamicParameters parameters = new DynamicParameters();

            if (operationType == (int)GlobalEnumList.DBOperation.Create || operationType == (int)GlobalEnumList.DBOperation.Update)
            {
                parameters.Add("@param_administrative_organogram_id", organogram.organogram_id, DbType.Int32);
                parameters.Add("@param_administrative_organogram_code", organogram.organogram_code, DbType.String);
                parameters.Add("@param_company_group_id", company_group_id ?? 0, DbType.Int32);
                parameters.Add("@param_company_corporate_id", company_corporate_id ?? 0, DbType.Int32);
                parameters.Add("@param_company_id", company_id ?? 0, DbType.Int32);
                parameters.Add("@param_organogram_location_id", organogram.location_id, DbType.Int32);
                parameters.Add("@param_organogram_department_id", organogram.department_id, DbType.Int32);
                parameters.Add("@param_organogram_parrent_id", organogram.parent_id, DbType.Int32);
                parameters.Add("@param_organogram_sorting_priority", organogram.sorting_priority, DbType.Int32);
                parameters.Add("@param_organogram_is_active", organogram.is_active, DbType.Byte);
                parameters.Add("@param_organogram_approve_user_id", organogram.approve_user_id, DbType.Byte);  
                parameters.Add("@param_created_user_id", currentUserInfoId ?? 0, DbType.Int32);
                parameters.Add("@param_DBOperation", operationType == (int)GlobalEnumList.DBOperation.Create ? GlobalEnumList.DBOperation.Create : GlobalEnumList.DBOperation.Update);
            }
            else if (operationType == (int)GlobalEnumList.DBOperation.Delete)
            {
                parameters.Add("@param_administrative_organogram_id", organogram.organogram_id, DbType.Int32);
                parameters.Add("@param_DBOperation", GlobalEnumList.DBOperation.Delete);
            }
            else if (operationType == (int)GlobalEnumList.DBOperation.Approve)
            {
                parameters.Add("@param_administrative_organogram_id", organogram.organogram_id, DbType.Int32);
                parameters.Add("@param_created_user_id", currentUserInfoId ?? 0, DbType.Int32);
                parameters.Add("@param_DBOperation", GlobalEnumList.DBOperation.Approve);
            }
            return parameters;
        }
        public async Task<dynamic> IUD_Organogram(Organogram Organogram, int dbOperation)
        {
            var message = new CommonMessage();
            var parameters = OrganogramParameterBinding(Organogram, dbOperation);

            if (_dbConnection.State == ConnectionState.Closed)
                _dbConnection.Open();
            try
            {
                dynamic data = await _dbConnection.QueryAsync("[Administrative].[SP_Organogram_IUD]", parameters, commandType: CommandType.StoredProcedure);

                if (dbOperation == (int)GlobalEnumList.DBOperation.Delete)
                {
                    return message = CommonMessage.SetSuccessMessage(CommonMessage.CommonDeleteMessage);
                }
                if (dbOperation == (int)GlobalEnumList.DBOperation.Approve)
                {
                    return message = CommonMessage.SetSuccessMessage("Organogram Approved");
                }
                if (data.Count > 0)
                {
                    message = CommonMessage.SetSuccessMessage(CommonMessage.CommonSaveMessage, data);
                }
                else
                {
                    message = CommonMessage.SetErrorMessage(CommonMessage.CommonErrorMessage);
                }
            }
            catch (Exception ex)
            {
                message = CommonMessage.SetErrorMessage(ex.Message);
            }
            finally
            {
                //DB connection dispose with db connection close
                _dbConnection.Dispose();
            }
            return (message);
        }

        public async Task<dynamic> OrganogramActivity(long Organogram_id)
        {
            var message = new CommonMessage();
            var currentUserInfoId = _httpContextAccessor.HttpContext.Items["User_Info_Id"];

            var result = (dynamic)null;
            if (_dbConnection.State == ConnectionState.Closed)
                _dbConnection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@param_shcema_name", "[Administrative]", DbType.String);
            parameters.Add("@param_table_name", "Organogram", DbType.String);
            parameters.Add("@param_object_id", Organogram_id, DbType.Int32);
            parameters.Add("@param_user_info_id", currentUserInfoId, DbType.Int32);
            parameters.Add("@param_remarks", "Organogram active inactive", DbType.String);
            parameters.Add("@param_created_datetime", DateTime.Now, DbType.DateTime);
            try
            {
                result = await _dbConnection.QueryAsync("[Administrative].[SP_Activity]", parameters, commandType: CommandType.StoredProcedure);

                if (result.Count > 0)
                {
                    message = CommonMessage.SetSuccessMessage(CommonMessage.CommonSaveMessage);
                }
                else
                {
                    message = CommonMessage.SetErrorMessage(CommonMessage.CommonErrorMessage);
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                _dbConnection.Close();
            }

            return message;
        }
        public async Task<dynamic> GetAllOrganogram()
        {
            var message = new CommonMessage();
            var company_group_id = _httpContextAccessor.HttpContext.Items["company_group_id"];
            var result = (dynamic)null;

            if (_dbConnection.State == ConnectionState.Closed)
                _dbConnection.Open();
            try
            {
                string sqlc = @"select c.company_name,c.company_id
from Administrative.Location l left join Administrative.Company c on l.company_id=c.company_id
 where l.company_group_id=@company_group_id group by  c.company_name,c.company_id";
                DynamicParameters parametersCompany = new DynamicParameters();
                parametersCompany.Add("@company_group_id", company_group_id);
                dynamic dataCompany = await _dbConnection.QueryAsync<dynamic>(sqlc, parametersCompany);
               //
                string sqlL = @"select l.location_name,l.location_id,c.company_id
from Administrative.Location l left join Administrative.Company c on l.company_id=c.company_id
 where l.company_group_id=@company_group_id group by  l.location_name,l.location_id,c.company_id";
                DynamicParameters parametersLocation = new DynamicParameters();
                parametersLocation.Add("@company_group_id", company_group_id);
                dynamic dataLocation = await _dbConnection.QueryAsync<dynamic>(sqlL, parametersLocation);
                string sql = @"select c.company_name,location_code,location_code+' - '+location_name location_name,(select d.department_name from Administrative.Department d where d.department_id=og.department_id)as department,isnull(og.department_id,'0') as department_id,l.location_id,c.company_id,isnull(og.organogram_id,'0')organogram_id,isnull(og.is_active,'false')is_active,isnull(og.sorting_priority,0)sorting_priority from Administrative.Location l left join Administrative.Company c on l.company_id=c.company_id
left join Administrative.Organogram og on l.location_id=og.location_id where l.company_group_id=@company_group_id";
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@company_group_id", company_group_id);
                dynamic data = await _dbConnection.QueryAsync<dynamic>(sql, parameters);
                if (data != null)
                {
                    List<dynamic> dataListCompany = dataCompany;
                    List<dynamic> dataListLocations = dataLocation;


                    List<dynamic> dataList = data;                  
                    List<Dtotree> comObj = new List<Dtotree>();                    
                    List<treeLocations> locationtmpObj = new List<treeLocations>();
                    List<Data> Dataobj = new List<Data>();
                
                    foreach (var item in dataList)
                    {
                        if (!string.IsNullOrEmpty(item.department))
                        {
                            Data dt = new Data();
                        dt.location_id = item.location_id;
                        dt.department = item.department;
                        dt.company_id = item.company_id;
                        dt.department_id = item.department_id;
                        dt.location_name = item.location_name;
                        dt.company_name = item.company_name;
                        dt.Organogram_Id = item.organogram_id;
                        Dataobj.Add(dt);
                        }

                    }

                    foreach (var item in dataListLocations)
                    {
                        var Locationsdata = Dataobj.FirstOrDefault(x => x.company_id == item.company_id && x.location_id == item.location_id);
                        var department = Dataobj.Where(x => x.company_id == item.company_id && x.location_id == item.location_id).ToList();
                        treeLocations obj = new treeLocations();

                        Data ldata = new Data();//Locations
                        if (Locationsdata!=null)
                        {
                            ldata.Node_Name = Locationsdata.location_name;
                            ldata.Organogram_Id = Locationsdata.Organogram_Id;
                            ldata.company_id = Locationsdata.company_id;
                        }
                        else
                        {
                            ldata.Node_Name = item.location_name;
                            ldata.Organogram_Id = item.location_id;
                            ldata.company_id = item.company_id;
                        }
                        obj.data = ldata;
                        List<Departmenttree> objdept = new List<Departmenttree>(); 
                        foreach (var itemdp in department)
                        {
                            Departmenttree objd = new Departmenttree();
                            Data ddata = new Data();//department
                            ddata.Node_Name = itemdp.department;
                            ddata.Organogram_Id = itemdp.Organogram_Id;
                            ddata.company_id = itemdp.company_id;
                            ddata.location_id = itemdp.location_id;
                            ddata.department_id = itemdp.department_id;
                            objd.data = ddata;
                            objdept.Add(objd);
                        }
                        obj.children = objdept;
                        locationtmpObj.Add(obj);
                    }
                   
                    foreach (var item in dataListCompany)
                    {       
                        var locations= locationtmpObj.Where(x => x.data.company_id == item.company_id).ToList();
                       
                        Dtotree obj = new Dtotree();//Company                       
                       
                        Data cdata = new Data();
                        cdata.Node_Name = item.company_name;
                        cdata.Organogram_Id = 0;

                        obj.data = cdata;
                        obj.children = locations;
                        comObj.Add(obj);
                    }

                    //Dtotree objtree = new Dtotree();
                    //objtree.data = objCd;
                    //objtree.children = comObj;


                    //  mobj.data = objtree;
                    //AllObj.Add(mobj);

                    //Root objroot = new Root();
                    //objroot.data = comObj;

                    result = comObj;//BuildTrees(0, dtos);
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                _dbConnection.Close();
            }
            return (result);
        }
        //private static IList<Dtotree> BuildLocationsTrees(int? lid, IList<Dtotree> candicates)
        //{
        //    var children = candicates.Where(c => c.data.location_id == lid).ToList();
        //    //var children = candicates.ToList();
        //    if (children == null || children.Count() == 0)
        //    {
        //        return null;
        //    }
        //    foreach (var i in children)
        //    {
        //        i.children = BuildTrees(i.data.location_id, candicates);
        //    }
        //    return children;
        //}
        //private static IList<Dtotree> BuildTrees(int? lid, IList<Dtotree> candicates)
        //{
        //    var children = candicates.Where(c => c.data.location_id == lid).ToList();
        //    //var children = candicates.ToList();
        //    if (children == null || children.Count() == 0)
        //    {
        //        return null;
        //    }
        //    foreach (var i in children)
        //    {
        //        i.children = BuildTrees(i.data.location_id, candicates);
        //    }
        //    return children;
        //}
        //private static IList<Dtotree> BuildDeptTrees(int? lid, IList<Dtotree> candicates)
        //{
        //    var children = candicates.Where(c => c.data.location_id == lid).ToList();
        //    if (children == null || children.Count() == 0)
        //    {
        //        return null;
        //    }
        //    foreach (var i in children)
        //    {
        //        i.children = BuildTrees(i.data.location_id, candicates);
        //    }
        //    return children;
        //}
        public async Task<dynamic> GetOrganogramById(long Organogram_id)
        {
            var result = (dynamic)null;
            if (_dbConnection.State == ConnectionState.Closed)
                _dbConnection.Open();

            try
            {
                string sql = @"select c.company_name,location_code,location_code+' - '+location_name location_name,isnull((select d.department_name from Administrative.Department d where d.department_id=og.department_id),'')as department,isnull(og.department_id,'0') as department_id,l.location_id,c.company_id,isnull(og.organogram_id,'0')organogram_id,isnull(og.is_active,'false')is_active,isnull(og.sorting_priority,0)sorting_priority from Administrative.Location l left join Administrative.Company c on l.company_id=c.company_id
left join Administrative.Organogram og on l.location_id=og.location_id where og.organogram_id=@Organogram_id";
              
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Organogram_id", Organogram_id);

                // result = await _dbConnection.QueryAsync<dynamic>(sql, parameters);
                dynamic data = await _dbConnection.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);
                if (data != null)
                {

                    result = OrganogramViewModel.ConvertToModel(data);
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                _dbConnection.Close();
            }

            return result;
        }
        public async Task<dynamic> GetAllActiveOrganogram()
        {
            var message = new CommonMessage();

            var result = (dynamic)null;

            if (_dbConnection.State == ConnectionState.Closed)
                _dbConnection.Open();
            var company_group_id = _httpContextAccessor.HttpContext.Items["company_group_id"];

            try
            {
                var sql = @"select c.company_name,location_code,location_code+' - '+location_name location_name,isnull((select d.department_name from Administrative.Department d where d.department_id=og.department_id),'')as department,isnull(og.department_id,'0') as department_id,l.location_id,c.company_id,isnull(og.organogram_id,'0')organogram_id,isnull(og.is_active,'false')is_active,isnull(og.sorting_priority,0)sorting_priority from Administrative.Location l left join Administrative.Company c on l.company_id=c.company_id
left join Administrative.Organogram og on l.location_id=og.location_id where l.company_group_id=@company_group_id and og.is_active =1 order by og.organogram_id";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@company_group_id", company_group_id);

                dynamic data = await _dbConnection.QueryAsync<dynamic>(sql, parameters);
                if (data != null)
                {
                    List<dynamic> dataList = data;
                    result = (from dr in dataList select OrganogramViewModel.ConvertToModel(dr)).ToList();

                    //  message = CommonMessage.SetSuccessMessage(CommonSaveMessage,result);

                }

            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {

                _dbConnection.Close();
            }


            return (result);
        }
        
    }
}

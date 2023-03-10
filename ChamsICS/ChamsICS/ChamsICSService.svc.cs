using ChamsICSLib.Data;
using ChamsICSLib.Utilities;
using ChamsICSWebService.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace ChamsICSWebService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public partial class ChamsICSService : iChamsICSService, iChamsICSPortalService
    {
        ServiceHelper ServiceHelper = new ServiceHelper();

        static string DebugLogPath = ConfigurationManager.AppSettings["DebugLoggingPath"];
        static string ErrorLogPath = ConfigurationManager.AppSettings["ErrorLoggingPath"];

        public AuthoriseTerminalRes AuthoriseTerminal(AuthoriseTerminalReq req)
        {
            AuthoriseTerminalRes res = new AuthoriseTerminalRes();
            if (req == null || req.AgentCode == null || req.TerminalName == null || req.TerminalSerialNumber == null)
            {
                throw new ArgumentNullException("Invalid AuthoriseTerminal Request");
            }

            try
            {
                Response response;
                var authorise = ServiceHelper.AuthoriseRequest(req.AgentCode, req.UserName, req.Password, out response);
                if (!authorise)
                {
                    res.ResponseCode = response.ResponseCode;
                    res.ResponseDescription = response.ResponseDescription;
                    return res;
                }

                //Validate AgentCode
                string msg = string.Empty;
                bool ValidateRes = ServiceHelper.ValidateAuthoriseTerminal(req, out msg);
                if (!ValidateRes)
                {
                    string err = msg;
                    res.ResponseCode = ResponseHelper.VALIDATION_ERROR;
                    res.ResponseDescription = err;
                    return res;
                }
                //Generate New TerminalCode
                String TerminalCode = ServiceHelper.GetNewTerminalCode(req);
                if (TerminalCode == null || TerminalCode == String.Empty)
                {
                    res.ResponseCode = ResponseHelper.APPLICATION_ERROR;
                    res.ResponseDescription = "Unable To Authorise Terminal";
                    return res;
                }
                else
                {
                    res.ResponseCode = ResponseHelper.SUCCESS;
                    res.ResponseDescription = "Successful";
                    res.TerminalCode = TerminalCode;
                    res.TerminalSerialNumber = req.TerminalSerialNumber;
                    return res;
                }
            }
            catch (Exception ex)
            {
                Logger.logToFile(ex, ErrorLogPath);
                res.ResponseCode = ResponseHelper.APPLICATION_ERROR;
                res.ResponseDescription = "Application Error";
                return res;
            }

        }

        public VerifyResidentIdRes VerifyResidentId(VerifyResidentIdReq req)
        {
            VerifyResidentIdRes res = new VerifyResidentIdRes();

            try
            {
                if (req == null || req.AgentCode == null || req.TerminalCode == null || req.ResidentId == null)
                {
                    res.ResponseCode = ResponseHelper.FAILED;
                    res.ResponseDescription = "Invalid Requests";
                    return res;
                }
                Response response;
                var authorise = ServiceHelper.AuthoriseRequest(req.AgentCode, req.UserName, req.Password, out response);
                if (!authorise)
                {
                    res.ResponseCode = response.ResponseCode;
                    res.ResponseDescription = response.ResponseDescription;
                    return res;
                }

                //Validate AgentCode
                string msg = string.Empty;
                bool ValidateRes = ServiceHelper.ValidateAgentCode(req.AgentCode, out msg);
                if (!ValidateRes)
                {
                    res.ResponseCode = ResponseHelper.FAILED;
                    res.ResponseDescription = msg;
                    return res;
                }

                IdentityService serviceUrl = ServiceHelper.GetClientIdentityService(req.TerminalCode);
                //Validate serviceURL Before Proceeding...
                //Debug=== Remove before live release build
                Logger.logToFile(DebugLogPath, "ServiceUrl-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", serviceUrl.URL);

                if (serviceUrl == null || serviceUrl.URL.Trim() == "" || serviceUrl.URL == string.Empty)
                {
                    res.ResponseCode = ResponseHelper.FAILED;
                    res.ResponseDescription = "Invalid Terminal Code";
                    return res;
                }
                try
                {
                    int ResidencyIdStatus = 0;
                    VerifyIdResponse resident = ServiceHelper.ValidateResidentID(serviceUrl.URL, serviceUrl.Username, serviceUrl.Password, req.ResidentId, out ResidencyIdStatus);

                    if (resident == null)
                    {
                        res.ResponseCode = ResponseHelper.FAILED;
                        res.ResponseDescription = "Inner Service Error. Unable to Complete Resident ID Validation";
                        return res;
                    }
                    else
                    {
                        res.ResponseCode = resident.ResponseCode;
                        res.ResponseDescription = resident.ResponseDescription;
                        res.ResidentId = req.ResidentId;
                        res.FirstName = resident.FIRSTNAME;
                        res.MiddleName = resident.MIDDLENAME;
                        res.LastName = resident.SURNAME;
                        res.Address = resident.RESIDENTIAL_ADDRESS;
                        res.Email = resident.EMAIL;
                        res.PhoneNumber = resident.MOBILENUMBER;
                        res.DateOfBirth = resident.DOB;
                        res.Gender = resident.GENDER;
                    }
                    return res;
                }
                catch (Exception e)
                {
                    Logger.logToFile(e, ErrorLogPath);
                    res.ResponseCode = ResponseHelper.APPLICATION_ERROR;
                    res.ResponseDescription = "Application Error";
                    return res;
                }                
            }
            catch (Exception ex)
            {
                Logger.logToFile(ex, ErrorLogPath);
                res.ResponseCode = ResponseHelper.APPLICATION_ERROR;
                res.ResponseDescription = "Application Error";
                return res;
            }
        }

        public UploadTransactionRes UploadTransaction(UploadTransactionReq req)
        {
            UploadTransactionRes res = new UploadTransactionRes();
            if (req == null)
            {
                throw new ArgumentNullException("Invalid UploadTransaction Request");
            }

            string msg = string.Empty;
            string tempResId = string.Empty;

            //Validate Upload Data
            try
            {
                bool ValidateRes = ServiceHelper.ValidateUploadTransaction(req, out msg, out tempResId);
                if (!ValidateRes)
                {
                    //Log Failed Upload Request
                    string msgLog = msg + Environment.NewLine + XMLHelper.serializeObjectToXMLString(req);

                    //====Log Failed Upload to File===
                    Logger.logToFile(msgLog, DebugLogPath+"\\Failed_Upload\\",true,req.TransactionCode,"xml",true);

                    //====Log Failed Upload to Database====
                    ServiceHelper.UploadExceptionToDb(msg, req);

                    res.ResponseCode = ResponseHelper.VALIDATION_ERROR;
                    res.ResponseDescription = msg;
                    return res;
                }
                else
                {
                    res.ResponseCode = ResponseHelper.SUCCESS;
                    res.ResponseDescription = "Successful";
                    res.ResidentId = req.ResidentId;
                    res.TempResidentId = tempResId;
                    res.TransactionCode = req.TransactionCode;
                }
                return res;
            }
            catch (Exception ex)
            {
                Logger.logToFile(ex, ErrorLogPath);
                res.ResponseCode = ResponseHelper.APPLICATION_ERROR;
                res.ResponseDescription = "Application Error";
                return res;
            }
        }

        public QueryTransactionRes QueryUploadTransaction(QueryTransactionReq req)
        {
            string msg = string.Empty;
            QueryTransactionRes res = new QueryTransactionRes();
            if (req == null)
            {
                throw new ArgumentNullException("Invalid QueryUploadTransaction Request");
            }

            string TerminalCode = req.TransactionCode.Substring(4, 6);
            string AgentCode = req.TransactionCode.Substring(0, 4);

            //Validate TransactionCode IS Right Length
            if (!ServiceHelper.ValidateTransactionCodeLength(req.TransactionCode, out msg))
            {
                res.ResponseCode = ResponseHelper.VALIDATION_ERROR;
                res.ResponseDescription = msg;
                return res;
            }

            //Validate TerminalCode Section of Transaction Code is Valid

            if (!ServiceHelper.ValidateTerminalCode(TerminalCode, out msg))
            {
                res.ResponseCode = ResponseHelper.VALIDATION_ERROR;
                res.ResponseDescription = msg;
                return res;
            }

            //Validate AgentCode Section of Transaction Code is Valid and Terminal is Assigned to It
            if (!ServiceHelper.ValidateAgentCode(AgentCode, TerminalCode, out msg))
            {
                res.ResponseCode =ResponseHelper.VALIDATION_ERROR;
                res.ResponseDescription = msg;
                return res;
            }

            //Get Transaction Details;
            TransactionLog tLog = ServiceHelper.QueryTransaction(req.TransactionCode);
            if (tLog==null)
            {
                res.ResponseCode = ResponseHelper.UNKNOWN_ERROR;
                res.ResponseDescription = "Unable to retrieve Transaction Details";
                return res;
            }
            else
            {
                res.ResponseCode = ResponseHelper.SUCCESS;
                res.ResponseDescription = "Successful";

                res.TerminalCode = req.TerminalCode;
                res.TransactionCode = tLog.Code;
                res.ResidentId = tLog.ResidentId;
                res.Address = tLog.Address;
                res.Amount = tLog.Amount.ToString();
                res.DateOfBirth = tLog.DateOfBirth.ToString();
                res.Email = tLog.Email;
                res.FirstName = tLog.FirstName;
                res.Gender = tLog.Gender;
                res.LastName = tLog.Lastname;
                res.MiddleName = tLog.MiddleName;
                res.PhoneNumber = tLog.PhoneNumber;
                res.RevenueCode = tLog.RevenueCode;
                res.TransactionDate = tLog.TransactionDate.ToString();
                res.UploadDate = tLog.UploadDate.ToString();
                res.PaymentReference = tLog.PaymentReference;

            }
            return res;
        }

        public GetTerminalsRes GetTerminal(FindTerminalReq req)
        {
            GetTerminalsRes res = new GetTerminalsRes();

            return res;
        }

        public GetTerminalDetailsRes GetTerminalDetails(GetTerminalsReq req) {
            GetTerminalDetailsRes res = new GetTerminalDetailsRes();

            return res;
        }

        public GetTerminalsRes FindTerminal(FindTerminalReq req)
        {
            GetTerminalsRes res = new GetTerminalsRes();
            try
            {
                Response response;
                var authorise = ServiceHelper.AuthoriseRequest(req.AgentCode, req.UserName, req.Password, out response);
                if (!authorise)
                {
                    res.ResponseCode = response.ResponseCode;
                    res.ResponseDescription = response.ResponseDescription;
                    return res;
                }

                res = ServiceHelper.FindServiceTerminal(req.AgentCode,req.TerminalCode);

                return res;
            }
            catch (Exception ex)
            {
                res.ResponseCode = ResponseHelper.APPLICATION_ERROR;
                res.ResponseDescription = ex.Message;
                Logger.logToFile(ex, ErrorLogPath);
                return res;
            }
        }

        public GetTerminalsRes GetTerminals(GetTerminalsReq req)
        {

            GetTerminalsRes res = new GetTerminalsRes();
            try
            {
                Response response;
                var authorise = ServiceHelper.AuthoriseRequest(req.AgentCode, req.UserName, req.Password, out response);
                if (!authorise)
                {
                    res.ResponseCode = response.ResponseCode;
                    res.ResponseDescription = response.ResponseDescription;
                    return res;
                }

                res = ServiceHelper.GetServiceTerminals(req.AgentCode);
                return res;
            }
            catch (Exception ex)
            {
                res.ResponseCode = ResponseHelper.APPLICATION_ERROR;
                res.ResponseDescription = ex.Message;
                Logger.logToFile(ex, ErrorLogPath);
                return res;
            }
        }

        public GetServiceRevenueRes GetRevenue(ServiceRevenueReq req)
        {
            GetServiceRevenueRes res = new GetServiceRevenueRes();
            try
            {
                Response response;
                var authorise = ServiceHelper.AuthoriseRequest(req.AgentCode, req.UserName, req.Password, out response);
                if (!authorise)
                {
                    res.ResponseCode = response.ResponseCode;
                    res.ResponseDescription = response.ResponseDescription;
                    return res;
                }
                res = ServiceHelper.GetServiceRevenue(req.AgentCode);

                return res;
            }
            catch (Exception ex)
            {
                res.ResponseCode = ResponseHelper.APPLICATION_ERROR;
                res.ResponseDescription = ex.Message;
                Logger.logToFile(ex, ErrorLogPath);
                return res;
            }
        }

        public GetServiceRevenueRes FindRevenue(ServiceFindRevenueReq req)
        {
            GetServiceRevenueRes res = new GetServiceRevenueRes();
            try
            {
                Response response;
                var authorise = ServiceHelper.AuthoriseRequest(req.AgentCode, req.UserName, req.Password, out response);
                if (!authorise)
                {
                    res.ResponseCode = response.ResponseCode;
                    res.ResponseDescription = response.ResponseDescription;
                    return res;
                }
                res = ServiceHelper.FindServiceRevenue(req);

                return res;
            }
            catch (Exception ex)
            {
                res.ResponseCode = ResponseHelper.APPLICATION_ERROR;
                res.ResponseDescription = ex.Message;
                Logger.logToFile(ex, ErrorLogPath);
                return res;
            }
        }

        public GetServiceRevenueRes GetLattestRevenue(ServiceRevenueReq req)
        {
            GetServiceRevenueRes res = new GetServiceRevenueRes();
            try
            {
                Response response;
                var authorise = ServiceHelper.AuthoriseRequest(req.AgentCode, req.UserName, req.Password, out response);
                if (!authorise)
                {
                    res.ResponseCode = response.ResponseCode;
                    res.ResponseDescription = response.ResponseDescription;
                    return res;
                }
                res = ServiceHelper.GetServiceRevenue(req.AgentCode);
                return res;
            }
            catch (Exception ex)
            {
                res.ResponseCode = ResponseHelper.APPLICATION_ERROR;
                res.ResponseDescription = ex.Message;
                Logger.logToFile(ex, ErrorLogPath);
                return res;
            }
        }

        public GetAgentLocationsRes GetAgentLocations(GetAgentLocationsReq req)
        {
            GetAgentLocationsRes res = new GetAgentLocationsRes();
            try
            {
                Response response;
                var authorise = ServiceHelper.AuthoriseRequest(req.AgentCode, req.UserName, req.Password, out response);
                if (!authorise)
                {
                    res.ResponseCode = response.ResponseCode;
                    res.ResponseDescription = response.ResponseDescription;
                    return res;
                }

                res = ServiceHelper.GetAgentLocations(req.AgentCode);
                return res;
            }
            catch (Exception ex)
            {
                res.ResponseCode = ResponseHelper.APPLICATION_ERROR;
                res.ResponseDescription = ex.Message;
                Logger.logToFile(ex, ErrorLogPath);
                return res;
            }
        }

    }
}

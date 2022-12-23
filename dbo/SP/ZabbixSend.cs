using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using DbClrZabbixSP;
using Microsoft.SqlServer.Server;

public partial class StoredProcedures
{
    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void ZabbixSend(string sZabbixServer, int sZabbixPort, string ZbxHost, string ZbxKey, string ZbxVal)
    {
        try
        {
            //string sZabbixServer = "zabbix.kxlbxsx-xx.local";
            //int sZabbixPort = 10051;
            //string ZbxHost = "PSQLM02.kxlxxsx-xx.local";
            //string ZbxKey = "integration_terrasoft";
            //string ZbxVal = "ops!";

            ZS_Request x = new ZS_Request(ZbxHost, ZbxKey, ZbxVal);
            ZS_Response resp = x.Send(sZabbixServer, sZabbixPort);
            SqlContext.Pipe.Send($"{resp.response}\n{resp.info}");
        }
        catch (Exception ex)
        {
            SqlContext.Pipe.Send("An error occured" + ex.Message + ex.StackTrace);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SobekCM.Core.Aggregations;
using SobekCM.Core.Configuration.Engine;
using SobekCM.Core.FileSystems;
using SobekCM.Core.Results;
using SobekCM.Engine_Library.ApplicationState;
using SobekCM.Engine_Library.Endpoints;
using SobekCM.Tools;
using System.Web;
using System.Xml;
using System.Security;
using EngineAgnosticLayerDbAccess;
using System.Data;
using SobekCM.Engine_Library.Database;
using System.Xml.Linq;
using System.IO;

namespace AGGCOUNTS
{
    public class AGGCOUNTS : ResultsServices
    {
        /// <summary> Get just the search statistics information for a search or browse </summary>
        /// <param name="Response"></param>
        /// <param name="UrlSegments"></param>
        /// <param name="QueryString"></param>
        /// <param name="Protocol"></param>
        /// <param name="IsDebug"></param>
        public void AGGCOUNTS_Results_XML(HttpResponse Response, List<string> UrlSegments, NameValueCollection QueryString, Microservice_Endpoint_Protocol_Enum Protocol, bool IsDebug)
        {
            double started = DateTimeToUnixTimestamp(DateTime.Now);
            double ended;
            Results_Arguments args = new Results_Arguments(QueryString);
            String url, data,dataaggs,code,name,isActive,isHidden,id,type;
            XmlDocument doc,docaggs;
            XmlNodeList nodes;
            XmlNode solrnode;
            XmlAttributeCollection attrs;
            DataSet tempSet=null,darkSet=null,aliasesSet=null;
            DataTable aggregations,darkAggregations,aliases;
            int idx = 0;
            short mask = 0;
            long count = 0;
            int errorcount = 0;
           
            Response.Output.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>");
            //Response.Output.WriteLine("<!-- connection string is " + SecurityElement.Escape(Engine_Database.Connection_String) + "]. -->");

            url = "http://dis.lib.usf.edu:8080/documents/select/?q=%28*:*%29&rows=0&facet=true&facet.field=aggregation_code&facet.limit=-1";
            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] raw = wc.DownloadData(url);
            dataaggs = System.Text.Encoding.UTF8.GetString(raw);
            //dataaggs = dataaggs.Replace("<response>", "<response xmlns=\"http://dummy.solr.org/dummy\">");
            //Response.Output.WriteLine("<!-- got " + dataaggs.Length + " bytes from solr -->");
            docaggs = new XmlDocument();

            try
            {
                docaggs.LoadXml(dataaggs);
                //Response.Output.WriteLine("<!-- loaded xml into docaggs successfully. -->");
            }
            catch (XmlException e)
            {
                docaggs = null;
                //Response.Output.WriteLine("<!-- xmlexception trying to load solr xml into docaggs-->");
            }

            Dictionary<String, String> solraggs = new Dictionary<String, String>();

            byte[] byteArray = Encoding.ASCII.GetBytes(dataaggs);
            //Response.Output.WriteLine("<!-- " + byteArray.Length + " elements in byteArray -->");
            MemoryStream stream = new MemoryStream(byteArray);
            //Response.Output.WriteLine("<!-- " + stream.Length + " bytes in stream -->");

            XElement root = XElement.Load(stream);

            if (root.HasElements)
            {
                //Response.Output.WriteLine("<!-- root has elements -->");
            }
            else
            {
                //Response.Output.WriteLine("<!-- root has NO elements -->");
            }

            IEnumerable<XElement> aggs =
                from agg in root.Descendants("int")
                select agg;

            //Response.Output.WriteLine("<!-- aggCount=" + aggs.Count() + " -->");
            String myvalue;

            if (aggs.Count() > 0)
            {
                idx = 0;

                foreach (XElement agg in aggs)
                {
                    idx++;

                    try
                    {
                        myvalue = agg.Value;
                    }
                    catch (Exception e)
                    {
                        myvalue = "0";
                    }

                    try
                    {
                        //Response.Output.WriteLine("<!-- agg " + idx + ". " + agg.Attribute("name").Value.ToLower() + "=" + myvalue + " -->");

                        solraggs.Add(agg.Attribute("name").Value.ToString().ToLower(), myvalue);
                    }
                    catch (Exception e)
                    {
                        //Response.Output.WriteLine("<!-- exception trying to display or add to solraggs [" + e.Message + "] -->");
                    }
                }

                //Response.Output.WriteLine("<!-- solraggs has " + solraggs.Count + "  entries -->");
            }

            /*
            try
            {
                int idxelements = 0;

                foreach (XElement myelement in XElement.Load(stream).Elements("lst"))
                {
                    idxelements++;

                    Response.Output.WriteLine("<!-- myelement@name=" + myelement.Attribute("name").Value + " -->");

                    if (myelement.Attribute("name").Value == "facet_counts")
                    {
                        foreach (XElement mysubelement in myelement.Elements("int"))
                        {
                            Response.Output.WriteLine("<!-- mysubelement: " + mysubelement.Attribute("name").Value + " -->");

                            solraggs.Add(new KeyValuePair<String, String>(mysubelement.Attribute("name").Value.ToString(), mysubelement.Value.ToString()));

                        }
                    }
                }

                Response.Output.WriteLine("<!-- idxelements=" + idxelements + " -->");
            }
            catch (Exception e)
            {
                Response.Output.WriteLine("<!-- exception trying to get solraggs [" + e.Message + "]:[" + e.StackTrace + "] -->");
                solraggs = null;
            }

            if (solraggs==null)
            {
                Response.Output.WriteLine("<!-- solraggs was null -->");
            }
            else
            {
                Response.Output.WriteLine("<!-- solraggs count=" + solraggs.Count + " -->");
            }

            if (solraggs!=null && solraggs.Count>0)
            {
                Response.Output.WriteLine("<!-- there are " + solraggs.Count + " in solraggs list -->");

                foreach  (KeyValuePair<String,String> agg in solraggs)
                {
                    Response.Output.WriteLine("<!-- " + agg.Key + "=" + agg.Value + " -->");
                }
            }
            else
            {
                Response.Output.WriteLine("<!-- solraggs was null or had no count -->");
            }
            
            */

            try
            {
                EalDbParameter[] paramList = new EalDbParameter[2];
                tempSet = EalDbAccess.ExecuteDataset(EalDbTypeEnum.MSSQL, Engine_Database.Connection_String, CommandType.StoredProcedure, "SobekCM_Get_Item_Aggregation_Status_Counts", paramList);

                //Response.Output.WriteLine("<!-- tempSet has " + tempSet.Tables.Count + " tables. -->");
                //Response.Output.WriteLine("<!-- tempSet table 0 has " + tempSet.Tables[0].Rows.Count + " rows -->");

                aggregations = tempSet.Tables[0];
            }
            catch (Exception e)
            {
                aggregations = null;
                Response.Output.WriteLine("<!-- " + e.Message + " -->");
            }

            // darkSet
            try
            {
                EalDbParameter[] paramList = new EalDbParameter[2];
                darkSet = EalDbAccess.ExecuteDataset(EalDbTypeEnum.MSSQL, Engine_Database.Connection_String, CommandType.StoredProcedure, "SobekCM_Get_Item_Aggregation_Dark_Counts", paramList);

                //Response.Output.WriteLine("<!-- tempSet has " + tempSet.Tables.Count + " tables. -->");
                //Response.Output.WriteLine("<!-- tempSet table 0 has " + tempSet.Tables[0].Rows.Count + " rows -->");

                darkAggregations = darkSet.Tables[0];
            }
            catch (Exception e)
            {
                darkAggregations = null;
                Response.Output.WriteLine("<!-- " + e.Message + " -->");
            }

            //aliasSet
            try
            {
                EalDbParameter[] paramList = new EalDbParameter[2];
                aliasesSet = EalDbAccess.ExecuteDataset(EalDbTypeEnum.MSSQL, Engine_Database.Connection_String, CommandType.StoredProcedure, "SobekCM_Get_Item_Aggregation_Aliases", paramList);
                aliases = aliasesSet.Tables[0];
            }
            catch (Exception e)
            {
                aliases = null;
                Response.Output.WriteLine("<!-- exception trying to get aliasesSet [" + e.Message + "] -->");
            }

            url = Engine_ApplicationCache_Gateway.Settings.Servers.Application_Server_URL + "/engine/aggregations/all/xml";

            wc = new System.Net.WebClient();
            raw = wc.DownloadData(url);
            data = System.Text.Encoding.UTF8.GetString(raw);
            doc = new XmlDocument();
            doc.LoadXml(data);

            nodes = doc.SelectNodes("//Item_Aggregation_Related_Aggregations");
            idx = 0;

            Response.Output.WriteLine("<results date=\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\" count=\"" + nodes.Count + "\">");

            foreach (XmlNode node in nodes)
            {
                idx++;

                attrs = node.Attributes;
                code = attrs["code"].Value.ToLower();
                name = attrs["name"].Value;
                isActive = attrs["isActive"].Value;
                isHidden = attrs["isHidden"].Value;
                id = attrs["id"].Value;
                type = attrs["type"].Value;

                Response.Output.WriteLine("<result rownum=\"" + idx + "\" code=\"" + code + "\" name=\"" + SecurityElement.Escape(name) + "\" id=\"" + id + "\" type=\"" + type + "\" isActive=\"" + isActive + "\" isHidden=\"" + isHidden + "\" ");
                
                IEnumerable<DataRow> query =
                   from aggregation in aggregations.AsEnumerable()
                   select aggregation;

                IEnumerable<DataRow> thisagg =
                    query.Where(p => p.Field<string>("code").Equals(code));
               
                foreach (DataRow p in thisagg)
                {
                    //Response.Output.WriteLine("<counts code=\"" + p.Field<String>("code") + "\">");
                   
                    try
                    {
                        try
                        {
                            mask = p.Field<short>("mask");
                        }
                        catch (Exception e)
                        {
                            mask = 999;
                        }

                        try
                        {
                            //Response.Output.WriteLine(" mycount=\"" + p["count"].ToString() + "\" ");
                            count = long.Parse(p["count"].ToString());
                        }
                        catch (Exception e)
                        {
                            count = 32767;
                            errorcount++;
                            Response.Output.WriteLine(" error" + errorcount + "=\"" + e.Message + " [" + SecurityElement.Escape(e.StackTrace) + "]\"");
                        }

                        if (mask==0)
                        {
                            Response.Output.WriteLine(" count_public=\"" + count + "\" ");
                        }
                        else if (mask==-1)
                        {
                            Response.Output.WriteLine(" count_private=\"" + count + "\" ");
                        }
                        else
                        {
                            Response.Output.WriteLine(" count_mask" + Math.Abs(mask) + "=\"" + count + "\" ");
                        }
                    }
                    catch (Exception e)
                    {
                        errorcount++;
                        Response.Output.WriteLine(" error" + errorcount + "=\"mask or count retrieval issue - " + e.Message + " [" + SecurityElement.Escape(e.StackTrace ) + "]\"");
                    }
                }

                // dark
                query =
                   from dark in darkAggregations.AsEnumerable()
                   select dark;

                thisagg = query.Where(p => p.Field<string>("code").Equals(code));

                foreach (DataRow p in thisagg)
                {
                    Response.Output.WriteLine("count_dark=\"" + p["count"] + "\" ");
                }

                try
                {
                    // aliases
                    query =
                      from alias in aliases.AsEnumerable()
                      select alias;

                    IEnumerable<DataRow> thisaliases = query.Where(p => p.Field<string>("Code").Equals(code));

                    String cids = "";

                    foreach (DataRow p in thisaliases)
                    {
                        cids += p["AggregationAlias"].ToString().ToUpper() + ",";
                    }

                    if (cids.Length > 0)
                    {
                        cids = cids.Substring(0, cids.Length - 1);

                        if (cids.Length>3)
                        {
                            Response.Output.WriteLine("aliases=");
                        }
                        else
                        {
                            Response.Output.WriteLine("alias=");
                        }

                        Response.Output.WriteLine("\"" + cids + "\" ");
                    }
                    else
                    {
                        Response.Output.WriteLine(" alias=\"NONE\" ");
                    }
                }
                catch (Exception e)
                {
                    Response.Output.WriteLine("<!-- exception trying to get cids [" + e.Message + "] -->");
                }

                try
                {
                    // >xpath("//lst[@name='aggregation_code']/int");

                    /*
                    solrnode = docaggs.SelectSingleNode("//results/lst[@name='facet_counts']/lst[@name='facet_fields']/lst[@name='aggregation_code']/int[@name='" + code.ToUpper() + "']/text()");

                    if (solrnode != null)
                    {
                        Response.Output.WriteLine(" indexed=\"" + solrnode.Value + "\" ");
                    }
                    else
                    {
                        errorcount++;
                        Response.Output.WriteLine(" error" + errorcount + "=\"solrnode was null\" "); 
                    }
                    */


                    String indexedcount = solraggs[code];

                    Response.Output.WriteLine(" indexed_count=\"" + indexedcount + "\" ");
                }
                catch (Exception e)
                {
                    errorcount++;
                    Response.Output.WriteLine(" indexed_count=\"0\"");
                }

                Response.Output.WriteLine(" />");
            }

            Response.Output.WriteLine("</results>");
            ended = DateTimeToUnixTimestamp(DateTime.Now);

            Response.Output.WriteLine("<!-- generated in " + (ended - started) + " seconds. -->");
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
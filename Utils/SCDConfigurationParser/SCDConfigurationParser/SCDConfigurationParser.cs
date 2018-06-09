using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SCDConfigurationParser
{
    public class SCDConfigurationParser
    {
        private XmlDocument doc { get; set; }
        private IDbConnection scdDbConnection { get; set; }
        private string createTableTemplate_Hardware { get; set; }
        private string addColumnTemplate_Hardware { get; set; }
        private string createTableTemplate_Software { get; set; }
        private string addColumnTemplate_Software { get; set; }
        private string createTableTemplate_Solution { get; set; }
        private string addColumnTemplate_Solution { get; set; }
        private string addCostBlockCofigTemplate { get; set; }
        private string createTableTemplate_CostBlockConfig { get; set; }
        private string addCostElementConfiguration { get; set; }

        public SCDConfigurationParser(string configPath, IDbConnection dbConnection)
        {
            doc = new XmlDocument();
            doc.Load(configPath);
            scdDbConnection = dbConnection;
            createTableTemplate_Hardware = System.IO.File.ReadAllText(@"ScriptTemplates\CreateTable_Hardware.sql");
            addColumnTemplate_Hardware = System.IO.File.ReadAllText(@"ScriptTemplates\AddColumn_Hardware.sql");
            createTableTemplate_Software = System.IO.File.ReadAllText(@"ScriptTemplates\CreateTable_SoftwareAndSolution.sql");
            addColumnTemplate_Software = System.IO.File.ReadAllText(@"ScriptTemplates\AddColumn_SoftwareAndSolution.sql");
            addCostBlockCofigTemplate = System.IO.File.ReadAllText(@"ScriptTemplates\AddCostBlocksConfiguration.sql");
            createTableTemplate_CostBlockConfig = System.IO.File.ReadAllText(@"ScriptTemplates\CreateTable_CostBlockConfig.sql");
            addCostElementConfiguration = System.IO.File.ReadAllText(@"ScriptTemplates\AddCostElementConfiguration.sql");
        }

        public void CreateDBStructure()
        {
            scdDbConnection.Open();
            XmlNode blocks = doc.DocumentElement.SelectSingleNode("/SCDConfiguration/Blocks");
           
            foreach (XmlNode block in blocks.ChildNodes)
            {
                AddCostBlocksConfiguration(block);
                AddCostBlockConfiguration(block);
                AddHardwareCostBlocks(block);
                AddSoftwareAndSolutionCostBlocks(block);
            }
            scdDbConnection.Close();
        }

        private void AddCostBlocksConfiguration(XmlNode block)
        {
            if (block.Attributes["Application"] == null) return;
            if (block.Attributes["Name"] == null) return;
            string cbName = block.Attributes["Name"].InnerText;
            
            List<string> applications=block.Attributes["Application"].InnerText.Split(',').ToList();
            foreach (string cbApplication in applications)
            {
                string addCostBlockConfig = addCostBlockCofigTemplate.Replace("{CostBlock}", cbName);
                addCostBlockConfig = addCostBlockConfig.Replace("{Application}", cbApplication);
                addCostBlockConfig = addCostBlockConfig.Replace("{CostBlock_Caption}", cbName);
                addCostBlockConfig = addCostBlockConfig.Replace("{CostBlock_ConfigTable}", String.Format(
                    "{0}.{1}_Config", cbApplication, cbName));
                RunCommand(addCostBlockConfig);
            }
        }

        private void AddCostBlockConfiguration(XmlNode block)
        {
            if (block.Attributes["Application"] == null) return;
            if (block.Attributes["Name"] == null) return;
            string cbName = block.Attributes["Name"].InnerText;
            string applicationName = block.Attributes["Application"].InnerText;

            string createTable = createTableTemplate_CostBlockConfig.Replace("{Application}", applicationName);
            createTable = createTable.Replace("{CostBlock}", cbName);
            RunCommand(createTable);
            XmlNode elements = block.SelectSingleNode("Elements");
         
            
            foreach (XmlNode element in elements.ChildNodes)
            {
                   string addElementConfig = addCostElementConfiguration.Replace("{Application}", applicationName);
                   addElementConfig = addElementConfig.Replace("{CostBlock}", cbName);
                   addElementConfig = addElementConfig.Replace("{CostElementName}", getAttributeInnerText(element, "Name"));

                   addElementConfig = addElementConfig.Replace("{DataEntry}", getAttributeInnerText(element, "DataEntry"));
                   addElementConfig = addElementConfig.Replace("{LowestInputLevel}", getAttributeInnerText(element, "LowestInputLevel"));
                   addElementConfig = addElementConfig.Replace("{DefaultInputLevel}", getAttributeInnerText(element, "DefaultInputLevel"));
                   addElementConfig = addElementConfig.Replace("{HighestInputLevel}", getAttributeInnerText(element, "HighestInputLevel"));
                   addElementConfig = addElementConfig.Replace("{Unit}", getAttributeInnerText(element, "Unit"));
                   addElementConfig = addElementConfig.Replace("{Domain}", getAttributeInnerText(element, "Domain"));
                   addElementConfig = addElementConfig.Replace("{ApplicationPart}", getAttributeInnerText(element, "ApplicationPart"));
                   addElementConfig = addElementConfig.Replace("{ManualChangeAllowed}", getAttributeInnerText(element, "ManualChangeAllowed"));



                   foreach (XmlNode dependency in element.ChildNodes)
                   {
                       if (dependency.Name != "Dependency") continue;
                       addElementConfig = addElementConfig.Replace("{DependencyName}", getInnerTextOrNull(dependency, "Name"));
                       addElementConfig = addElementConfig.Replace("{DependencyTable}", getInnerTextOrNull(dependency, "Table"));
                       addElementConfig = addElementConfig.Replace("{DependencyRelation}", getInnerTextOrNull(dependency, "DependencyRelation"));
                       addElementConfig = addElementConfig.Replace("{HasRelationToInputParameter}", getInnerTextOrNull(dependency, "HasRelationToInputParameter"));
                       addElementConfig = addElementConfig.Replace("{RelationToInputParameterTable}", getInnerTextOrNull(dependency, "RelationToInputParameterTable"));
                       addElementConfig = addElementConfig.Replace("{RelationToInputParameter}", getInnerTextOrNull(dependency, "RelationToInputParameter"));
                      
                   }

                   addElementConfig = addElementConfig.Replace("{DependencyName}", "NULL");
                   addElementConfig = addElementConfig.Replace("{DependencyTable}", "NULL");
                   addElementConfig = addElementConfig.Replace("{DependencyRelation}", "NULL");
                   addElementConfig = addElementConfig.Replace("{HasRelationToInputParameter}", "NULL");
                   addElementConfig = addElementConfig.Replace("{RelationToInputParameterTable}", "NULL");
                   addElementConfig = addElementConfig.Replace("{RelationToInputParameter}", "NULL");

                  

                RunCommand(addElementConfig);
            }
        }

        private string getInnerTextOrNull(XmlNode node, string attrName)
        {
            string attributeInnerText = getAttributeInnerText(node, attrName);
            if (attributeInnerText == String.Empty)
            return "NULL";
            return attributeInnerText;
            
        }

        private void AddHardwareCostBlocks(XmlNode block)
        { 
             if (!block.Attributes["Application"].InnerText.Contains("Hardware")) return;
             string createTable = createTableTemplate_Hardware.Replace("{TableName}", block.Attributes["Name"].InnerText);
                RunCommand(createTable);
                
                XmlNode elements =block.SelectSingleNode("Elements");
            foreach (XmlNode element in elements.ChildNodes)
            {
                string addElementColumn = addColumnTemplate_Hardware.Replace("{TableName}", block.Attributes["Name"].InnerText);
                addElementColumn = addElementColumn.Replace("{ColumnName}", getAttributeInnerText(element, "Name"));
                addElementColumn = addElementColumn.Replace("{ColumnType}", "float");
                RunCommand(addElementColumn);
                //approved column
                addElementColumn = addColumnTemplate_Hardware.Replace("{TableName}", block.Attributes["Name"].InnerText);
                addElementColumn = addElementColumn.Replace("{ColumnName}", String.Format(
                "{0}_Approved",getAttributeInnerText(element, "Name")));
                addElementColumn = addElementColumn.Replace("{ColumnType}", "float");
                RunCommand(addElementColumn);

                string addDependencyColumnTemplate = addColumnTemplate_Hardware.Replace("{TableName}",
                      block.Attributes["Name"].InnerText);
                addDependencyColumns(element, addDependencyColumnTemplate);
            }
        }

        private void AddSoftwareAndSolutionCostBlocks(XmlNode block)
        {
            if (!block.Attributes["Application"].InnerText.Contains("SoftwareAndSolution")) return;
            string createTable = createTableTemplate_Software.Replace("{TableName}", block.Attributes["Name"].InnerText);
            RunCommand(createTable);

            XmlNode elements = block.SelectSingleNode("Elements");
            foreach (XmlNode element in elements.ChildNodes)
            {
                string addElementColumn = addColumnTemplate_Software.Replace("{TableName}", block.Attributes["Name"].InnerText);
                addElementColumn = addElementColumn.Replace("{ColumnName}", getAttributeInnerText(element, "Name"));
                addElementColumn = addElementColumn.Replace("{ColumnType}", "float");
                RunCommand(addElementColumn);
                //approved column
                addElementColumn = addColumnTemplate_Software.Replace("{TableName}", block.Attributes["Name"].InnerText);
                addElementColumn = addElementColumn.Replace("{ColumnName}", String.Format(
                "{0}_Approved",getAttributeInnerText(element, "Name")));
                addElementColumn = addElementColumn.Replace("{ColumnType}", "float");
                RunCommand(addElementColumn);
                string addDependencyColumnTemplate = addColumnTemplate_Software.Replace("{TableName}",
                        block.Attributes["Name"].InnerText);
                addDependencyColumns(element, addDependencyColumnTemplate);
            }
        }
       


        private void addDependencyColumns(XmlNode element, string addDependencyColumnCommand)
        {
            foreach (XmlNode dependency in element.ChildNodes)
            {
                if (dependency.Name != "Dependency") continue;
                if (getAttributeInnerText(dependency, "DependencyRelation") != "Table") continue;
                if (String.IsNullOrEmpty(getAttributeInnerText(dependency, "Table"))) continue;

                //foreach pk column add column from dependency
                IEnumerable<Tuple<string, string>> pkList = getTablePK(getAttributeInnerText(dependency, "Table"));
                List<string> addDependencyColumnsList = new List<string>();
                foreach (Tuple<string, string> pkColumn in pkList)
                {
                    addDependencyColumnCommand = addDependencyColumnCommand.Replace("{ColumnName}", pkColumn.Item1);
                    addDependencyColumnCommand = addDependencyColumnCommand.Replace("{ColumnType}", pkColumn.Item2);
                    addDependencyColumnsList.Add(addDependencyColumnCommand);
                }

                foreach (var command in addDependencyColumnsList)
                {
                    RunCommand(command);
                }
            }
        }

        private string getAttributeInnerText(XmlNode node, string attrName)
        {
            string result = String.Empty;
            if (node == null) return result;
            if (node.Attributes[attrName] == null) return result;
            return node.Attributes[attrName].InnerText;
        }

        private void RunCommand(string command)
        {
            using (var scdbcommand = new SqlCommand(command, (SqlConnection)scdDbConnection))
            {
                scdbcommand.ExecuteNonQuery();
            }
        }

        private IEnumerable<Tuple<string, string>> getTablePK(string tableName)
        {
            string getPKColumnsTemplate = System.IO.File.ReadAllText(@"ScriptTemplates\GetPrimaryKeyColumns.sql");
            string getPKColumns = getPKColumnsTemplate.Replace("{TableName}", tableName);
            using (var scdbcommand = new SqlCommand(getPKColumns, (SqlConnection)scdDbConnection))
            {
                using (SqlDataReader reader = scdbcommand.ExecuteReader())
                {                   
                    while (reader.Read())
                    {
                        yield return new Tuple<string, string>(reader[0].ToString(), reader[1].ToString());                       
                    }
                }
            }
            
        }     


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnLive_Reports
{
	class xmlFileHeaders
	{
		public static string start { get { return
"<?xml version=\"1.0\"?>\r\n"
+ "<?mso-application progid=\"Excel.Sheet\"?>\r\n"
+ "<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"\r\n"
+ " xmlns:o=\"urn:schemas-microsoft-com:office:office\"\r\n"
+ " xmlns:x=\"urn:schemas-microsoft-com:office:excel\"\r\n"
+ " xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"\r\n"
+ " xmlns:html=\"http://www.w3.org/TR/REC-html40\">\r\n"
+ " <DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">\r\n"
+ "  <Author>TurnLive_Report</Author>\r\n"
+ "  <LastAuthor>TurnLive_Report</LastAuthor>\r\n"
+ "  <Created>" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ") +"</Created>\r\n"
+ "  <Version>12.00</Version>\r\n"
+ " </DocumentProperties>\r\n"
+ " <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">\r\n"
+"  <WindowHeight>7995</WindowHeight>\r\n"
+ "  <WindowWidth>20115</WindowWidth>\r\n"
+ "  <WindowTopX>240</WindowTopX>\r\n"
+ "  <WindowTopY>120</WindowTopY>\r\n"
+ "  <RefModeR1C1/>\r\n"
+ "  <ProtectStructure>False</ProtectStructure>\r\n"
+ "  <ProtectWindows>False</ProtectWindows>\r\n"
+ " </ExcelWorkbook>\r\n"
+ " <Styles>\r\n"
+ "  <Style ss:ID=\"Default\" ss:Name=\"Normal\">\r\n"
+ "   <Alignment ss:Vertical=\"Bottom\"/>\r\n"
+ "   <Borders/>\r\n"
+ "   <Font ss:FontName=\"Calibri\" x:CharSet=\"204\" x:Family=\"Swiss\" ss:Size=\"11\"\r\n"
+ "    ss:Color=\"#000000\"/>\r\n"
+ "   <Interior/>\r\n"
+ "   <NumberFormat/>\r\n"
+ "   <Protection/>\r\n"
+ "  </Style>\r\n"
+ "  <Style ss:ID=\"s63\">\r\n"
+ "   <NumberFormat ss:Format=\"@\"/>\r\n"
+ "  </Style>\r\n"
+ " </Styles>\r\n"
+ " <Worksheet ss:Name=\"TurnLive_Report\">\r\n"
+ "  <Table x:FullColumns=\"1\"\r\n" //ss:ExpandedColumnCount=\"5\" ss:ExpandedRowCount=\"11\"
+ "   x:FullRows=\"1\" ss:DefaultRowHeight=\"15\">\r\n";
} }
		public static string end = 	
"  </Table>\r\n"
+ "  <WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">\r\n"
+ "   <PageSetup>\r\n"
+ "    <Header x:Margin=\"0.3\"/>\r\n"
+ "    <Footer x:Margin=\"0.3\"/>\r\n"
+ "    <PageMargins x:Bottom=\"0.75\" x:Left=\"0.7\" x:Right=\"0.7\" x:Top=\"0.75\"/>\r\n"
+ "   </PageSetup>\r\n"
+ "   <Selected/>\r\n"
+ "   <Panes>\r\n"
+ "    <Pane>\r\n"
+ "     <Number>3</Number>\r\n"
+ "     <ActiveRow>4</ActiveRow>\r\n"
+ "     <ActiveCol>2</ActiveCol>\r\n"
+ "    </Pane>\r\n"
+ "   </Panes>\r\n"
+ "   <ProtectObjects>False</ProtectObjects>\r\n"
+ "   <ProtectScenarios>False</ProtectScenarios>\r\n"
+ "  </WorksheetOptions>\r\n"
+ " </Worksheet>\r\n"
+ "</Workbook>\r\n";
		public static string RowS = "   <Row>\r\n";
		public static string RowE = "   </Row>\r\n";
		public static string CellS = "    <Cell ss:StyleID=\"s63\"><Data ss:Type=\"String\">";
		public static string CellE = "</Data></Cell>\r\n";
	}
}

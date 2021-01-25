using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace marvelyus
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                CheckBoxList1.DataSource = dataquery("SELECT * FROM sysobjects sobjects WHERE sobjects.xtype = 'U' order by name");
                CheckBoxList1.DataTextField = "name";
                CheckBoxList1.DataValueField = "id";
                CheckBoxList1.DataBind();
            }
        }

        public DataTable dataquery(string sorgu)
        {
            DataTable ds = new DataTable();
            using (SqlConnection baglanti = new SqlConnection(ConfigurationManager.ConnectionStrings["eczane"].ConnectionString))
            using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
            using (SqlDataAdapter adapter = new SqlDataAdapter(komut))
            {
                baglanti.Open();
                adapter.Fill(ds);
            }
            return ds;
        }
        public string stringquery(string sorgu)
        {
            string sonuc = "bulunamadi";
            using (SqlConnection baglanti = new SqlConnection(ConfigurationManager.ConnectionStrings["eczane"].ConnectionString))
            using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
            {
                baglanti.Open();
                try
                {
                    sonuc = komut.ExecuteScalar().ToString();
                }
                catch
                {
                    sonuc = "bulunamadi";
                }
                finally
                {
                    baglanti.Close();
                }
                baglanti.Close();
                return sonuc;
            }

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            List<ListItem> selected = new List<ListItem>();
            foreach (ListItem item in CheckBoxList1.Items)
                if (item.Selected)
                {
                    selected.Add(item);
                    olustur(item.Text);
                }
        }

        public void olustur(string table_name)
        {
            string query = "SELECT COLUMN_NAME,DATA_TYPE,TABLE_NAME,IS_NULLABLE,CHARACTER_MAXIMUM_LENGTH FROM IlacTakip.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + table_name + "'";
            DataTable dt_tableName = dataquery("SELECT TABLE_NAME FROM IlacTakip.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + table_name + "'");
            string tableName = dt_tableName.Rows[0]["TABLE_NAME"].ToString();
            printEntity(table_name);

            printDataAccessAbstract(table_name);
            printDataAccessConcrete(table_name);
            printBusinessAbstract(table_name);
            printBusinessConcrete(table_name);
            printMapping(table_name);
            Label1.Text = "işlem tamamlandı.";
        }

        public string getIlkHarfiLower(string tableName)
        {
            string lower = tableName.Substring(0, tableName.Length);
            string ilk_harf = lower.Substring(0, 1);
            string gerisi = lower.Substring(1, lower.Length - 1);
            ilk_harf = ilk_harf.ToLower();
            string yenisi = ilk_harf + gerisi;
            return yenisi;
        }
        public string getVirtualFromFK(string str)
        {
            string sonuc = "";

            int yer = str.LastIndexOf("_");
            int yer1 = str.LastIndexOf(".");
            sonuc = str.Substring(yer1 + 1, str.Length - yer - 3);
            return sonuc;
        }
        public string getVirtualFromFK_ler(string str)
        {
            string sonuc = "";

            int yer = str.LastIndexOf("_");
            int yer1 = str.LastIndexOf(".");
            sonuc = str.Substring(yer1 + 1, str.Length - yer);
            return sonuc;
        }
        public string getVirtualFromFKforList(string str)
        {
            string sonuc = "";

            int yer = str.IndexOf("_");
            int yer1 = str.IndexOf(".");
            sonuc = str.Substring(yer1 + 1);
            yer = sonuc.IndexOf("_");
            sonuc = sonuc.Substring(0, yer);

            return sonuc;
        }
        public string getProp(string tableName)
        {
            string bosluk = "";
            bosluk = bosluk.PadRight(8);
            string sonuc = "";

            DataTable dt_tableName_columns = dataquery("SELECT COLUMN_NAME,DATA_TYPE,TABLE_NAME,IS_NULLABLE,CHARACTER_MAXIMUM_LENGTH FROM IlacTakip.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + tableName + "'");
            for (int i = 0; i < dt_tableName_columns.Rows.Count; i++)
            {
                string str = "";
                if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "nvarchar")
                {
                    str = "string";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "datetime")
                {
                    str = "DateTime";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "int")
                {
                    str = "int";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "float")
                {
                    str = "double";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "money")
                {
                    str = "decimal";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "bit")
                {
                    str = "bool";
                }
                if (dt_tableName_columns.Rows[i]["IS_NULLABLE"].ToString() == "YES")
                {
                    str = str + "?";
                }

                sonuc = sonuc + bosluk + "public" + " " + str + " " +
                    dt_tableName_columns.Rows[i]["COLUMN_NAME"].ToString() + " " + "{ get; set; }" + System.Environment.NewLine;

            }
            //sonuc = sonuc + System.Environment.NewLine;
            DataTable dt_tableName_fk = dataquery("SELECT CONSTRAINT_NAME FROM IlacTakip.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE  TABLE_NAME = N'" + tableName + "'");
            for (int i = 0; i < dt_tableName_fk.Rows.Count; i++)
            {
                if (dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString().Contains("FK"))
                {
                    string str = getVirtualFromFK(dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString());
                    sonuc = sonuc + bosluk + "public virtual" + " " + str + " " +
                        str + " " +
                         "{ get; set; }" + System.Environment.NewLine;
                }
            }

            sonuc = sonuc + System.Environment.NewLine;

            DataTable dt_tableName_fk_for_list = dataquery(" SELECT CONSTRAINT_NAME FROM IlacTakip.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE  CONSTRAINT_NAME like '%" + tableName + "%' and TABLE_NAME <>  N'" + tableName + "'");

            for (int i = 0; i < dt_tableName_fk_for_list.Rows.Count; i++)
            {
                if (dt_tableName_fk_for_list.Rows[i]["CONSTRAINT_NAME"].ToString().Contains("FK"))
                {
                    string str = getVirtualFromFKforList(dt_tableName_fk_for_list.Rows[i]["CONSTRAINT_NAME"].ToString());
                    sonuc = sonuc + bosluk + "public virtual" + " " + "List<" + str.Substring(0, str.Length - 3) + ">" + " " +
                        str + " " +
                         "{ get; set; }" + System.Environment.NewLine;
                }
            }

            return sonuc;
        }
        public string getPropComplexType(string tableName)
        {
            string bosluk = "";
            bosluk = bosluk.PadRight(8);
            string sonuc = "";

            DataTable dt_tableName_columns = dataquery("SELECT COLUMN_NAME,DATA_TYPE,TABLE_NAME,IS_NULLABLE,CHARACTER_MAXIMUM_LENGTH FROM IlacTakip.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + tableName + "'");
            for (int i = 0; i < dt_tableName_columns.Rows.Count; i++)
            {
                string str = "";
                if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "nvarchar")
                {
                    str = "string";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "datetime")
                {
                    str = "DateTime";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "int")
                {
                    str = "int";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "float")
                {
                    str = "double";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "money")
                {
                    str = "decimal";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "bit")
                {
                    str = "bool";
                }
                if (dt_tableName_columns.Rows[i]["IS_NULLABLE"].ToString() == "YES")
                {
                    str = str + "?";
                }

                sonuc = sonuc + bosluk + "public" + " " + str + " " +
                    dt_tableName_columns.Rows[i]["COLUMN_NAME"].ToString() + " " + "{ get; set; }" + System.Environment.NewLine;

            }
            DataTable dt_tableName_fk_forAdi = dataquery("SELECT COLUMN_NAME,DATA_TYPE,TABLE_NAME,IS_NULLABLE,CHARACTER_MAXIMUM_LENGTH " +
                " FROM IlacTakip.INFORMATION_SCHEMA.COLUMNS WHERE " +
                " COLUMN_NAME <> 'Id' " +
                " and substring(COLUMN_NAME, LEN(COLUMN_NAME) - 1, 2) = 'Id' " +
                " and TABLE_NAME = N'" + tableName + "'");


            for (int i = 0; i < dt_tableName_fk_forAdi.Rows.Count; i++)
            {
                string str = dt_tableName_fk_forAdi.Rows[i]["COLUMN_NAME"].ToString();
                sonuc = sonuc + bosluk + "public string " +
                    str.Remove(str.Length - 2) + "Adi " + "{ get; set; }" + System.Environment.NewLine;
            }


            return sonuc;
        }
        public string getPropForMapping(string tableName)
        {
            string bosluk = "";
            string bosluk_2 = "";
            bosluk = bosluk.PadRight(12);
            bosluk_2 = bosluk.PadRight(2);

            string sonuc = bosluk + "this.HasKey(t => t.Id);" + System.Environment.NewLine;

            //    this.ToTable("IlacTakipGruplar");
            sonuc = sonuc + bosluk + "this.ToTable(\"" + tableName + "\");" + System.Environment.NewLine + System.Environment.NewLine;

            DataTable dt_tableName_columns = dataquery("SELECT COLUMN_NAME,DATA_TYPE,TABLE_NAME,IS_NULLABLE,CHARACTER_MAXIMUM_LENGTH FROM IlacTakip.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + tableName + "'");

            //column
            sonuc = sonuc + bosluk + "#region columns" + System.Environment.NewLine;
            for (int i = 0; i < dt_tableName_columns.Rows.Count; i++)
            {
                string str_COLUMN_NAME = dt_tableName_columns.Rows[i]["COLUMN_NAME"].ToString();
                sonuc = sonuc + bosluk + "this.Property(t => t." + str_COLUMN_NAME + ").HasColumnName(\"" + str_COLUMN_NAME + "\");" + System.Environment.NewLine;
            }
            sonuc = sonuc + bosluk + "#endregion" + System.Environment.NewLine + System.Environment.NewLine;

            //properties
            sonuc = sonuc + bosluk + "#region properties" + System.Environment.NewLine;
            sonuc = sonuc + bosluk + "this.Property(t => t.Id)" + System.Environment.NewLine +
              bosluk + bosluk_2 + ".HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity); " + System.Environment.NewLine;
            for (int i = 0; i < dt_tableName_columns.Rows.Count; i++)
            {
                string str_COLUMN_NAME = dt_tableName_columns.Rows[i]["COLUMN_NAME"].ToString();
                string str_IS_NULLABLE = dt_tableName_columns.Rows[i]["IS_NULLABLE"].ToString();
                string str_CHARACTER_MAXIMUM_LENGTH = dt_tableName_columns.Rows[i]["CHARACTER_MAXIMUM_LENGTH"].ToString();
                if (str_IS_NULLABLE == "YES")
                {
                    sonuc = sonuc + bosluk + "this.Property(t => t." + str_COLUMN_NAME + ").IsOptional();" + System.Environment.NewLine;
                }
                else if (str_IS_NULLABLE == "NO")
                {
                    sonuc = sonuc + bosluk + "this.Property(t => t." + str_COLUMN_NAME + ").IsRequired();" + System.Environment.NewLine;
                }

                if (str_CHARACTER_MAXIMUM_LENGTH != "")
                {
                    sonuc = sonuc.Remove(sonuc.TrimEnd().Length - 1);
                    sonuc = sonuc + System.Environment.NewLine + bosluk + bosluk_2 +  ".HasMaxLength(" + str_CHARACTER_MAXIMUM_LENGTH + ");" + System.Environment.NewLine;
                }
                else { }


                DataTable dt_tableName_index = dataquery("SELECT ind.name ,IndexName = ind.name, IndexId = ind.index_id, ColumnId = ic.index_column_id,   ColumnName = col.name,  ind.*, ic.*,  col.*  " +
" FROM sys.indexes ind INNER JOIN   sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id" +
" INNER JOIN  sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id" +
" INNER JOIN sys.tables t ON ind.object_id = t.object_id" +
" WHERE t.name = '" + tableName + "'");
                try
                {
                    string str_INDEX = dt_tableName_index.Rows[i]["IndexName"].ToString();
                    if (str_INDEX.Contains("UN_"))
                    {
                        sonuc = sonuc + bosluk + bosluk_2 + ".HasColumnAnnotation(\"Index\"," + System.Environment.NewLine +
                        bosluk + bosluk_2 + " new IndexAnnotation(" + System.Environment.NewLine +
                        bosluk + bosluk_2 + bosluk_2 + " new IndexAttribute(\"" + str_INDEX + "\")" + System.Environment.NewLine +
                        bosluk + bosluk_2 + bosluk_2 + "  {" + System.Environment.NewLine +
                        bosluk + bosluk_2 + bosluk_2 + bosluk_2 + "  IsUnique = true," + System.Environment.NewLine +
                        bosluk + bosluk_2 + bosluk_2 + bosluk_2 + "  Order = " + i + System.Environment.NewLine +
                        bosluk + bosluk_2 + bosluk_2 + "  }));" + System.Environment.NewLine;

                    }
                }
                catch { }

            }
            sonuc = sonuc + bosluk + "#endregion" + System.Environment.NewLine + System.Environment.NewLine;

            // Relationship

            DataTable dt_tableName_fk = dataquery("SELECT CONSTRAINT_NAME FROM IlacTakip.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE  TABLE_NAME = N'" + tableName + "'");

            if (dt_tableName_fk.Rows.Count > 0)
            {
                sonuc = sonuc + bosluk + "#region relationship" + System.Environment.NewLine;
                for (int i = 0; i < dt_tableName_fk.Rows.Count; i++)
                {
                    if (dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString().Contains("FK"))
                    {
                        string str = getVirtualFromFK(dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString());
                        // string str_ler = getVirtualFromFK_ler(dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString());

                        sonuc = sonuc + bosluk + "this.HasRequired(t => t." + str + ")" + System.Environment.NewLine +
                        bosluk + bosluk_2 + ".WithMany(et => et." + tableName + ")" + System.Environment.NewLine +
                        bosluk + bosluk_2 + ".HasForeignKey(t =>t." + str + "Id)" + System.Environment.NewLine +
                        bosluk + bosluk_2 + ".WillCascadeOnDelete(false);" + System.Environment.NewLine;

                    }
                }
                sonuc = sonuc + bosluk + "#endregion" + System.Environment.NewLine;
            }

            return sonuc;
        }
        public string getPropForMigrationUp(string tableName)
        {
            string bosluk = "";
            string bosluk_2 = "";
            bosluk = bosluk.PadRight(12);
            bosluk_2 = bosluk.PadRight(2);

            DataTable dt_tableName_columns = dataquery("SELECT COLUMN_NAME,DATA_TYPE,TABLE_NAME,IS_NULLABLE,CHARACTER_MAXIMUM_LENGTH FROM IlacTakip.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + tableName + "'");
            string sonuc = "";
            sonuc = sonuc + bosluk + "#region columns" + System.Environment.NewLine;

            sonuc = sonuc + @"CreateTable(" + System.Environment.NewLine +
            bosluk + tableName + ","  + System.Environment.NewLine +
            bosluk + "c => new" + System.Environment.NewLine +
            bosluk +   " { " + System.Environment.NewLine;


            string nullable = "";
            string CHARACTER_MAXIMUM_LENGTH = "";
            string data_type = "";
            for (int i = 0; i < dt_tableName_columns.Rows.Count; i++)
            {
                if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "nvarchar")
                {
                    data_type = "string";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "datetime")
                {
                    data_type = "DateTime";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "int")
                {
                    data_type = "int";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "float")
                {
                    data_type = "double";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "money")
                {
                    data_type = "decimal";
                }
                else if (dt_tableName_columns.Rows[i]["DATA_TYPE"].ToString() == "bit")
                {
                    data_type = "bool";
                }
                string str_COLUMN_NAME = dt_tableName_columns.Rows[i]["COLUMN_NAME"].ToString();
                if (dt_tableName_columns.Rows[i]["IS_NULLABLE"].ToString() == "YES")
                {
                    nullable = ")";
                }
                else
                {
                    nullable = "nullable: false)";
                }
                sonuc = sonuc + bosluk + str_COLUMN_NAME + " =c."+ data_type + "(" + nullable;
                DataTable dt_tableName_fk_for_list = dataquery(" SELECT CONSTRAINT_NAME FROM IlacTakip.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE  CONSTRAINT_NAME like '%" + tableName + "%' ");

                for (int it = 0; it < dt_tableName_fk_for_list.Rows.Count; it++)
                {
                    if (dt_tableName_fk_for_list.Rows[it]["CONSTRAINT_NAME"].ToString().Contains("PK"))
                    {
                        string str = getVirtualFromFKforList(dt_tableName_fk_for_list.Rows[it]["CONSTRAINT_NAME"].ToString());
                        sonuc = sonuc + ", identity: true)";
                    }
                    else
                    {
                        sonuc = sonuc + ")";
                    }
                }
                if (dt_tableName_columns.Rows[i]["CHARACTER_MAXIMUM_LENGTH"].ToString() != "NULL")
                {
                    CHARACTER_MAXIMUM_LENGTH = ",  maxLength:" + dt_tableName_columns.Rows[i]["CHARACTER_MAXIMUM_LENGTH"].ToString() + ")";
                }
                else
                {
                    CHARACTER_MAXIMUM_LENGTH = "";
                }

                sonuc = sonuc + System.Environment.NewLine;
            }
            sonuc = sonuc +  "})" + System.Environment.NewLine;
            //.PrimaryKey(t => t.Id)
            sonuc = sonuc + ".PrimaryKey(t => t.Id)" + System.Environment.NewLine;
           
            //  .ForeignKey("EczaneNobet.NobetUstGruplar", t => t.NobetUstGrupId, cascadeDelete: true)
           
            DataTable dt_tableName_fk = dataquery("SELECT CONSTRAINT_NAME FROM IlacTakip.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE  TABLE_NAME = N'" + tableName + "'");
            for (int i = 0; i < dt_tableName_fk.Rows.Count; i++)
            {
                if (dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString().Contains("FK"))
                {
                    string str = getVirtualFromFK(dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString());
                    sonuc = sonuc + ".ForeignKey(/" + dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString().Substring(
                        dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString().IndexOf("_"), dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString().IndexOf("_",0,2))
                        + "/, t." + str + ", cascadeDelete: true)" + System.Environment.NewLine; 
          
                }
            }
            dt_tableName_columns = dataquery("SELECT COLUMN_NAME,DATA_TYPE,TABLE_NAME,IS_NULLABLE,CHARACTER_MAXIMUM_LENGTH FROM IlacTakip.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + tableName + "'");
            for (int i = 0; i < dt_tableName_columns.Rows.Count; i++)
            {
                //  .Index(t => t.Adi, unique: true, name: "UN_EczaneGrupTanimAdi")
                DataTable dt_tableName_index = dataquery("SELECT ind.name ,IndexName = ind.name, IndexId = ind.index_id, ColumnId = ic.index_column_id,   ColumnName = col.name,  ind.*, ic.*,  col.*  " +
" FROM sys.indexes ind INNER JOIN   sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id" +
" INNER JOIN  sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id" +
" INNER JOIN sys.tables t ON ind.object_id = t.object_id" +
" WHERE t.name = '" + tableName + "'");
                try
                {
                    string str_INDEX_ind_name = dt_tableName_index.Rows[i]["ind.name"].ToString();
                    string str_INDEX = dt_tableName_index.Rows[i]["IndexName"].ToString();
                    
                    if (str_INDEX.Contains("UN_"))
                    {
                        sonuc = sonuc + ".Index(t => t."+ str_INDEX_ind_name + ", unique: true, name: /"+ str_INDEX + "/)";

                    }
                }
                catch { }
            }
            dt_tableName_fk = dataquery("SELECT CONSTRAINT_NAME FROM IlacTakip.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE  TABLE_NAME = N'" + tableName + "'");

            if (dt_tableName_fk.Rows.Count > 0)
            {
                for (int i = 0; i < dt_tableName_fk.Rows.Count; i++)
                {
                    if (dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString().Contains("FK"))
                    {
                        string str = getVirtualFromFK(dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString());
                        // string str_ler = getVirtualFromFK_ler(dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString());

                        sonuc = sonuc + ".Index(t => t." + str + ");" +  System.Environment.NewLine;
                    }
                }
                sonuc = sonuc + bosluk + "#endregion" + System.Environment.NewLine;
            }
            //  .Index(t => t.NobetUstGrupId);
            return sonuc;
        }
        public string getPropForMigrationDown(string tableName)
        {

            string bosluk = "";
            string bosluk_2 = "";
            bosluk = bosluk.PadRight(12);
            bosluk_2 = bosluk.PadRight(2);
            string sonuc = "";

            return sonuc;
        }

        public string getPropForcokaCok(string tableName)
        {
            string sonuc = "";
            DataTable dt_tableName_fk = dataquery("SELECT CONSTRAINT_NAME FROM IlacTakip.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE  TABLE_NAME = N'" + tableName + "'");
            for (int i = 0; i < dt_tableName_fk.Rows.Count; i++)
            {
                if (dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString().Contains("FK"))
                {
                    sonuc = ", IEntityDetayRepository<%CLASSNAME%Detay>";
                    printEntityComplextype(tableName);
                }
            }
            return sonuc;
        }

        public string getPropForcokaCokDetayicin(string tableName)
        {
            string properties = "";
            string bosluk = "";
            bosluk = bosluk.PadRight(24);
            DataTable dt_tableName_fk = dataquery("SELECT COLUMN_NAME,DATA_TYPE,TABLE_NAME,IS_NULLABLE,CHARACTER_MAXIMUM_LENGTH " +
              " FROM IlacTakip.INFORMATION_SCHEMA.COLUMNS WHERE " +
              " COLUMN_NAME <> 'Id' " +
              " and substring(COLUMN_NAME, LEN(COLUMN_NAME) - 1, 2) = 'Id' " +
              " and TABLE_NAME = N'" + tableName + "'");

            DataTable dt_tableName_fk_forAdi = dataquery("SELECT CONSTRAINT_NAME FROM IlacTakip.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE  TABLE_NAME = N'" + tableName + "'");
            for (int i = 0; i < dt_tableName_fk.Rows.Count; i++)
            {
                string str = dt_tableName_fk.Rows[i]["COLUMN_NAME"].ToString();
                properties = properties + str + " = s." + str + "," + System.Environment.NewLine + bosluk;

                string str2 = dt_tableName_fk_forAdi.Rows[i]["CONSTRAINT_NAME"].ToString();
                str2 = getVirtualFromFK(str2);
                properties = properties + str2 + "Adi = s." + str2 + ".Adi," + System.Environment.NewLine + bosluk;
            }

            string output =
@"        public %CLASSNAME%Detay GetDetay(Expression<Func<%CLASSNAME%Detay, bool>> filter)
        {
            using (var ctx = new IlacTakipContext())
            {
                return ctx.%CLASSNAME_ler%
                    .Select(s => new %CLASSNAME%Detay
                    {" + System.Environment.NewLine
                    + bosluk + properties +
//Id = s.Id,
//EczaneAdi = s.IlacTakipGrup.Eczane.Adi,
//IlacTakipGrupBaslamaTarihi = s.IlacTakipGrup.BaslangicTarihi,
//IlacTakipGrupId = s.IlacTakipGrupId,
//NobetGorevTipAdi = s.NobetGorevTip.Adi,
//NobetGorevTıpId = s.NobetGorevTıpId,
//NobetGrupAdi = s.IlacTakipGrup.NobetGrup.Adi,
//TakvimId = s.TakvimId,
@"
                    }).SingleOrDefault(filter);
            }
        }
        public List<%CLASSNAME%Detay> GetDetayList(Expression<Func<%CLASSNAME%Detay, bool>> filter = null)
        {
            using (var ctx = new IlacTakipContext())
            {
                var liste = ctx.%CLASSNAME_ler%
                    .Select(s => new %CLASSNAME%Detay
                    {" + System.Environment.NewLine
                    + bosluk + properties +
//Id = s.Id,
//EczaneAdi = s.IlacTakipGrup.Eczane.Adi,
//IlacTakipGrupBaslamaTarihi = s.IlacTakipGrup.BaslangicTarihi,
//IlacTakipGrupId = s.IlacTakipGrupId,
//NobetGorevTipAdi = s.NobetGorevTip.Adi,
//NobetGorevTıpId = s.NobetGorevTıpId,
//NobetGrupAdi = s.IlacTakipGrup.NobetGrup.Adi,
//TakvimId = s.TakvimId,
//Tarih = s.Takvim.Tarih,
@"
                    });

                return filter == null
                    ? liste.ToList()
                    : liste.Where(filter).ToList();
            }
        }"
;


            output = output.Replace("%CLASSNAME%", tableName.Substring(0, tableName.Length - 3));
            output = output.Replace("%CLASSNAME_ler%", tableName);
            return output;
        }

        public void printEntityComplextype(string tableName)
        {
            string properties = getPropComplexType(tableName);
            string output =
@"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Core.Entities;
using WM.Northwind.Entities.Concrete.IlacTakip;


namespace %NAMESPACE%
{
    public class %CLASSNAME%Detay: IComplexType" + System.Environment.NewLine +
   " { " + System.Environment.NewLine
       + properties
       // public int Id { get; set; }
       // public string Adi { get; set; }

       // public virtual List<NobetUstGrup> NobetUstGruplar { get; set; }
       + System.Environment.NewLine +
   "    } " + System.Environment.NewLine +
"} ";
            string class_name = tableName.Substring(0, tableName.Length - 3);
            output = output.Replace("%NAMESPACE%", "WM.Northwind.Entities.ComplexTypes.IlacTakip");
            output = output.Replace("%CLASSNAME%", tableName.Substring(0, tableName.Length - 3));
            // output = output.Replace("%CONNECTIONSTRING%", "conn" + application);
            string cOutputDir = @"d:/Uygulamalar/IlacTakip/WorkingModels/WM.Northwind.Entities/ComplexTypes/IlacTakip";// + tableName + "\\" + "Entitiy" ;
                                                                                                            //D:\Uygulamalar/IlacTakip\WorkingModels
                                                                                                            // System.IO.Directory.CreateDirectory(cOutputDir);
            System.IO.File.WriteAllText(cOutputDir + "\\" + class_name + "Detay.cs", output, System.Text.Encoding.UTF8);


        }
        public void printEntity(string tableName)
        {
            string properties = getProp(tableName);
            string output =
@"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Core.Entities;

namespace %NAMESPACE%
{
    public class %CLASSNAME% : IEntity
    {" + System.Environment.NewLine
       + properties
       // public int Id { get; set; }
       // public string Adi { get; set; }

       // public virtual List<NobetUstGrup> NobetUstGruplar { get; set; }
       +
   "    }" + System.Environment.NewLine +
"}";
            string class_name = tableName.Substring(0, tableName.Length - 3);
            output = output.Replace("%NAMESPACE%", "WM.Northwind.Entities.Concrete.IlacTakip");
            output = output.Replace("%CLASSNAME%", tableName.Substring(0, tableName.Length - 3));
            // output = output.Replace("%CONNECTIONSTRING%", "conn" + application);
            string cOutputDir = @"d:/Uygulamalar/IlacTakip/WorkingModels/WM.Northwind.Entities/Concrete/IlacTakip";// + tableName + "\\" + "Entitiy" ;
                                                                                                        //D:\Uygulamalar/IlacTakip\WorkingModels
                                                                                                        // System.IO.Directory.CreateDirectory(cOutputDir);
            System.IO.File.WriteAllText(cOutputDir + "\\" + class_name + ".cs", output, System.Text.Encoding.UTF8);


        }
        public void printDataAccessAbstract(string tableName)
        {
            string bosluk = "";
            bosluk = bosluk.PadRight(8);
            string cokaCok = getPropForcokaCok(tableName);
            string output =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Core.DAL;
using System.Linq.Expressions;
using WM.Northwind.Entities.ComplexTypes.IlacTakip;
using WM.Northwind.Entities.Concrete.IlacTakip;

namespace WM.Northwind.DataAccess.Abstract.IlacTakip
{
    public interface I%CLASSNAME%Dal : IEntityRepository<%CLASSNAME%> " + cokaCok + System.Environment.NewLine +
  "    {  " + System.Environment.NewLine +

   "    } " + System.Environment.NewLine +
"} ";
            string class_name = tableName.Substring(0, tableName.Length - 3);
            // output = output.Replace("%NAMESPACE%", "WM.Northwind.Entities.Concrete.IlacTakip");
            output = output.Replace("%CLASSNAME%", tableName.Substring(0, tableName.Length - 3));
            // output = output.Replace("%CONNECTIONSTRING%", "conn" + application);
            // string cOutputDir = @"d:/NobetYaz/" + tableName + "\\" + "DataAccess" + "\\" + "Abstract";
            string cOutputDir = @"d:/Uygulamalar/IlacTakip/WorkingModels/WM.Northwind.DataAccess/Abstract/IlacTakip";// + tableName + "\\" + "Entitiy" ;

            // System.IO.Directory.CreateDirectory(cOutputDir);
            System.IO.File.WriteAllText(cOutputDir + "\\I" + class_name + "Dal.cs", output, System.Text.Encoding.UTF8);


        }
        public void printDataAccessConcrete(string tableName)
        {
            string properties = "";
            DataTable dt_tableName_fk = dataquery("SELECT CONSTRAINT_NAME FROM IlacTakip.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE  TABLE_NAME = N'" + tableName + "'");
            for (int i = 0; i < dt_tableName_fk.Rows.Count; i++)
            {
                if (dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString().Contains("FK"))
                {
                    properties = getPropForcokaCokDetayicin(tableName);
                }
            }
             
            string output =
@"using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WM.Core.DAL.EntityFramework;
using WM.Northwind.DataAccess.Abstract.IlacTakip;
using WM.Northwind.DataAccess.Concrete.EntityFramework.Contexts;
using WM.Northwind.Entities.ComplexTypes.IlacTakip;
using WM.Northwind.Entities.Concrete.IlacTakip;

namespace WM.Northwind.DataAccess.Concrete.EntityFramework.IlacTakip
{
    public class Ef%CLASSNAME%Dal : EfEntityRepositoryBase<%CLASSNAME%, IlacTakipContext>, I%CLASSNAME%Dal
    {" + System.Environment.NewLine
       + properties +

//prop Detay

@"
    }
}"
;
            string class_name = tableName.Substring(0, tableName.Length - 3);
            // output = output.Replace("%NAMESPACE%", "WM.Northwind.Entities.Concrete.IlacTakip");
            output = output.Replace("%CLASSNAME%", tableName.Substring(0, tableName.Length - 3));
            // output = output.Replace("%CONNECTIONSTRING%", "conn" + application);
            // string cOutputDir = @"d:/NobetYaz/" + tableName + "\\" + "DataAccess" + "\\" + "Concrete";
            // System.IO.Directory.CreateDirectory(cOutputDir);
            string cOutputDir = @"d:/Uygulamalar/IlacTakip/WorkingModels/WM.Northwind.DataAccess/Concrete/EntityFramework/IlacTakip";// + tableName + "\\" + "Entitiy" ;

            System.IO.File.WriteAllText(cOutputDir + "\\Ef" + class_name + "Dal.cs", output, System.Text.Encoding.UTF8);


        }
        public void printBusinessAbstract(string tableName)
        {
            string bosluk = "";
            bosluk = bosluk.PadRight(24);
            string properties = "";
            DataTable dt_tableName_fk = dataquery("SELECT CONSTRAINT_NAME FROM IlacTakip.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE  TABLE_NAME = N'" + tableName + "'");
            for (int i = 0; i < dt_tableName_fk.Rows.Count; i++)
            {
                if (dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString().Contains("FK"))
                {
                    properties = @"%CLASSNAME%Detay GetDetayById(int %CLASSNAME_lower%Id);
                                   List <%CLASSNAME%Detay> GetDetaylar();";
                }
            }
            string output =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Northwind.Entities.ComplexTypes.IlacTakip;
using WM.Northwind.Entities.Concrete.IlacTakip;
//using WM.Northwind.Entities.Concrete.Optimization.IlacTakip;

namespace WM.Northwind.Business.Abstract.IlacTakip
{
    public interface I%CLASSNAME%Service
    {
        %CLASSNAME% GetById(int %CLASSNAME_lower%Id);
        List<%CLASSNAME%> GetList();
        //List<%CLASSNAME%> GetByCategory(int categoryId);
        void Insert(%CLASSNAME% %CLASSNAME_lower%);
        void Update(%CLASSNAME% %CLASSNAME_lower%);
        void Delete(int %CLASSNAME_lower%Id);"+ System.Environment.NewLine +
          bosluk +   properties
        + System.Environment.NewLine +



@"    }
} ";

            // output = output.Replace("%NAMESPACE%", "WM.Northwind.Entities.Concrete.IlacTakip");
            string class_name = tableName.Substring(0, tableName.Length - 3);
            output = output.Replace("%CLASSNAME%", class_name);
            string yeni_class_name = getIlkHarfiLower(class_name);
            output = output.Replace("%CLASSNAME_lower%", yeni_class_name);
            // output = output.Replace("%CONNECTIONSTRING%", "conn" + application);
            //string cOutputDir = @"d:/NobetYaz/" + tableName + "\\" + "Businesss" + "\\" + "Abstract";
            string cOutputDir = @"d:/Uygulamalar/IlacTakip/WorkingModels/WM.Northwind.Business/Abstract/IlacTakip";// + tableName + "\\" + "Entitiy" ;

            // System.IO.Directory.CreateDirectory(cOutputDir);
            System.IO.File.WriteAllText(cOutputDir + "\\I" + class_name + "Service.cs", output, System.Text.Encoding.UTF8);

        }
        public void printBusinessConcrete(string tableName)
        {
            string bosluk = "";
            bosluk = bosluk.PadRight(24);
            string properties = "";
            DataTable dt_tableName_fk = dataquery("SELECT CONSTRAINT_NAME FROM IlacTakip.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE  TABLE_NAME = N'" + tableName + "'");
            for (int i = 0; i < dt_tableName_fk.Rows.Count; i++)
            {
                if (dt_tableName_fk.Rows[i]["CONSTRAINT_NAME"].ToString().Contains("FK"))
                {
                    //sem_24012018
                    properties = 
@"          public %CLASSNAME%Detay GetDetayById(int %CLASSNAME_lower%Id)
            {
                return _%CLASSNAME_lower%Dal.GetDetay(x => x.Id == %CLASSNAME_lower%Id);
            }
            
            [CacheAspect(typeof(MemoryCacheManager))]
            public List<%CLASSNAME%Detay> GetDetaylar()
            {
                return _%CLASSNAME_lower%Dal.GetDetayList();
            }";
                }
            }
            string output =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Northwind.Business.Abstract;
using WM.Northwind.Business.Abstract.IlacTakip;
using WM.Northwind.DataAccess.Abstract.IlacTakip;
using WM.Core.Aspects.PostSharp.CacheAspects;
using WM.Core.CrossCuttingConcerns.Caching.Microsoft;
using WM.Northwind.Entities.ComplexTypes.IlacTakip;
using WM.Northwind.Entities.Concrete.IlacTakip;
//using WM.Northwind.Entities.Concrete.Optimization.IlacTakip;
//using WM.Optimization.Abstract.Samples;

namespace WM.Northwind.Business.Concrete.Managers.IlacTakip
{
    public class %CLASSNAME%Manager : I%CLASSNAME%Service
    {
        private I%CLASSNAME%Dal _%CLASSNAME_lower%Dal;

        public %CLASSNAME%Manager(I%CLASSNAME%Dal %CLASSNAME_lower%Dal)
        {
            _%CLASSNAME_lower%Dal = %CLASSNAME_lower%Dal;
        }
        [CacheRemoveAspect(typeof(MemoryCacheManager))]
        public void Delete(int %CLASSNAME_lower%Id)
        {
            _%CLASSNAME_lower%Dal.Delete(new %CLASSNAME% { Id = %CLASSNAME_lower%Id });
        }

        public %CLASSNAME% GetById(int %CLASSNAME_lower%Id)
        {
            return _%CLASSNAME_lower%Dal.Get(x => x.Id == %CLASSNAME_lower%Id);
        }
         [CacheAspect(typeof(MemoryCacheManager))]
        public List<%CLASSNAME%> GetList()
        {
            return _%CLASSNAME_lower%Dal.GetList();
        }
        [CacheRemoveAspect(typeof(MemoryCacheManager))]
        public void Insert(%CLASSNAME% %CLASSNAME_lower%)
        {
            _%CLASSNAME_lower%Dal.Insert(%CLASSNAME_lower%);
        }
        [CacheRemoveAspect(typeof(MemoryCacheManager))]
        public void Update(%CLASSNAME% %CLASSNAME_lower%)
        {
            _%CLASSNAME_lower%Dal.Update(%CLASSNAME_lower%);
        }"
        + System.Environment.NewLine +
          bosluk +   properties
        + System.Environment.NewLine +
        
@"
    } 
}";

            string class_name = tableName.Substring(0, tableName.Length - 3);
            output = output.Replace("%CLASSNAME%", class_name);
            string yeni_class_name = getIlkHarfiLower(class_name);
            output = output.Replace("%CLASSNAME_lower%", yeni_class_name);
            // output = output.Replace("%CONNECTIONSTRING%", "conn" + application);
            // string cOutputDir = @"d:/NobetYaz/" + tableName + "\\" + "Businesss" + "\\" + "Concrete";
            // System.IO.Directory.CreateDirectory(cOutputDir);
            string cOutputDir = @"d:/Uygulamalar/IlacTakip/WorkingModels/WM.Northwind.Business/Concrete/Managers/IlacTakip";// + tableName + "\\" + "Entitiy" ;

            System.IO.File.WriteAllText(cOutputDir + "\\" + class_name + "Manager.cs", output, System.Text.Encoding.UTF8);
        }
        public void printMapping(string tableName)
        {
            string properties = getPropForMapping(tableName);
            string output =
@"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Data.Entity.Infrastructure.Annotations;
using System.Text;
using System.Threading.Tasks;
using WM.Northwind.Entities.Concrete;
using WM.Northwind.Entities.Concrete.IlacTakip;

namespace WM.Northwind.DataAccess.Concrete.EntityFramework.Mapping.IlacTakip
{
    public class %CLASSNAME%Map : EntityTypeConfiguration<%CLASSNAME%>
    {
        public %CLASSNAME%Map()
        {" + System.Environment.NewLine
      + properties
            //// Primary Key
            //    this.HasKey(t => t.Id);

            //    // Table & Column Mappings
            //    this.ToTable("IlacTakipGruplar");
            //    this.Property(t => t.EczaneId).HasColumnName("EczaneId");
            //    this.Property(t => t.NobetGrupId).HasColumnName("NobetGrupId");
            //    this.Property(t => t.Id).HasColumnName("Id");
            //    this.Property(t => t.BaslangicTarihi).HasColumnName("BaslangicTarihi");
            //    this.Property(t => t.BitisTarihi).HasColumnName("BitisTarihi");
            //    this.Property(t => t.Aciklama).HasColumnName("Aciklama");

            //    // Properties
            //    this.Property(t => t.Id)
            //        .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            //    this.Property(t => t.EczaneId).IsRequired();
            //    this.Property(t => t.NobetGrupId).IsOptional();
            //    this.Property(t => t.BaslangicTarihi).IsRequired();
            //    this.Property(t => t.BitisTarihi).IsOptional();
            //    this.Property(t => t.Aciklama)
            //        .IsRequired()
            //        .HasMaxLength(50);

            //    // Relationship
            //    this.HasRequired(t => t.Eczane)
            //        .WithMany(et => et.IlacTakipGruplar)
            //        .HasForeignKey(t => t.EczaneId);

            //    this.HasRequired(t => t.NobetGrup)
            //        .WithMany(et => et.IlacTakipGruplar)
            //        .HasForeignKey(t => t.NobetGrupId);
            + 
       "        }" + System.Environment.NewLine +
   "    }" + System.Environment.NewLine +
"}"
;
            string class_name = tableName.Substring(0, tableName.Length - 3);
            output = output.Replace("%CLASSNAME%", class_name);
            string yeni_class_name = getIlkHarfiLower(class_name);
            output = output.Replace("%CLASSNAME_lower%", yeni_class_name);
            // output = output.Replace("%CONNECTIONSTRING%", "conn" + application);
            // string cOutputDir = @"d:/NobetYaz/" + tableName + "\\" + "DataAccess" + "\\" + "Mapping";
            string cOutputDir = @"d:/Uygulamalar/IlacTakip/WorkingModels/WM.Northwind.DataAccess/Concrete/EntityFramework/Mapping/IlacTakip";// + tableName + "\\" + "Entitiy" ;

            //System.IO.Directory.CreateDirectory(cOutputDir);
            System.IO.File.WriteAllText(cOutputDir + "\\" + class_name + "Map.cs", output, System.Text.Encoding.UTF8);
        }
//        public void printMaigration(string tableName)
//        {
//            string properties_up = getPropForMigrationUp(tableName);
//            string properties_down = getPropForMigrationDown(tableName);
//            string output =
//@"namespace WM.Northwind.DataAccess.Migrations
//{
//    using System;
//    using System.Data.Entity.Migrations;
    
//    public partial class initial : DbMigration
//    {
//        public override void Up()
//        {" + System.Environment.NewLine
//      + properties_up
//            //// Primary Key
//            //    this.HasKey(t => t.Id);

//            //    // Table & Column Mappings
//            //    this.ToTable("IlacTakipGruplar");
//            //    this.Property(t => t.EczaneId).HasColumnName("EczaneId");
//            //    this.Property(t => t.NobetGrupId).HasColumnName("NobetGrupId");
//            //    this.Property(t => t.Id).HasColumnName("Id");
//            //    this.Property(t => t.BaslangicTarihi).HasColumnName("BaslangicTarihi");
//            //    this.Property(t => t.BitisTarihi).HasColumnName("BitisTarihi");
//            //    this.Property(t => t.Aciklama).HasColumnName("Aciklama");

//            //    // Properties
//            //    this.Property(t => t.Id)
//            //        .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
//            //    this.Property(t => t.EczaneId).IsRequired();
//            //    this.Property(t => t.NobetGrupId).IsOptional();
//            //    this.Property(t => t.BaslangicTarihi).IsRequired();
//            //    this.Property(t => t.BitisTarihi).IsOptional();
//            //    this.Property(t => t.Aciklama)
//            //        .IsRequired()
//            //        .HasMaxLength(50);

//            //    // Relationship
//            //    this.HasRequired(t => t.Eczane)
//            //        .WithMany(et => et.IlacTakipGruplar)
//            //        .HasForeignKey(t => t.EczaneId);

//            //    this.HasRequired(t => t.NobetGrup)
//            //        .WithMany(et => et.IlacTakipGruplar)
//            //        .HasForeignKey(t => t.NobetGrupId);
//            +
//       "        }" + System.Environment.NewLine +
//       " public override void Down(){ " + System.Environment.NewLine +

//       properties_down +

//   "    }" + System.Environment.NewLine +

//   "    }" + System.Environment.NewLine +
//"}"
//;
//            string class_name = tableName.Substring(0, tableName.Length - 3);
//            output = output.Replace("%CLASSNAME%", class_name);
//            string yeni_class_name = getIlkHarfiLower(class_name);
//            output = output.Replace("%CLASSNAME_lower%", yeni_class_name);
//            // output = output.Replace("%CONNECTIONSTRING%", "conn" + application);
//            // string cOutputDir = @"d:/NobetYaz/" + tableName + "\\" + "DataAccess" + "\\" + "Mapping";
//            string cOutputDir = @"d:/Uygulamalar/IlacTakip/WorkingModels/WM.Northwind.DataAccess/Migrations/201801131747416_initial";// + tableName + "\\" + "Entitiy" ;

//            //System.IO.Directory.CreateDirectory(cOutputDir);
//            System.IO.File.WriteAllText(cOutputDir + "\\" + class_name + "2018011317474161_initial.cs", output, System.Text.Encoding.UTF8);
//        }

        protected void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListItem item in CheckBoxList1.Items)
                if (item.Selected)
                {
                    item.Selected = false;
                }
                else
                {
                    item.Selected = true;
                }
        }
    }
}
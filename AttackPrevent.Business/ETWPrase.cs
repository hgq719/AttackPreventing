using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class ETWPrase
    {
        private bool debug = false;
        public byte[] PayLoad { get; }
        public ETWPrase(byte[] payload)
        {
#if DEBUG
            debug = true;
#endif
            PayLoad = payload;
            if (debug)
            {
                string[] hosts = new string[] {
                    "ent.comm100.com",
                    "ent7.comm100.com",
                    "hosted.comm100.com",
                    "g2live.comm100.com",
                };
                string[] ips = new string[] {
                    "192.168.0.1",
                    "192.168.0.2",
                    "192.168.0.3",
                    "192.168.0.4",
                    "192.168.0.5",
                };
                string[] urls = new string[] {
                    "livechatdashboard/Dashboard.aspx",
                    "adminmanage/login.aspx",
                    "botadmin/Dashboard",
                    "botadmin/bot/intents",
                    "botadmin/bot/entities"
                };
                string[] querys = new string[] {
                    "siteId=1000490&botId=177&selectCategory=632&searchValue=&active=",
                    "siteId=1000490&botId=177&searchValue=&active=",
                    "siteId=1000490&botId=177",
                    "siteId=1000490",
                    "IfEditPlan=true&codePlanId=4005&siteId=1000490"
                };

                Random random = new Random(DateTime.Now.GetHashCode());
                int index = random.Next(4);

                index = 0;

                this.Cs_host = hosts[index];
                this.C_ip = ips[index];
                this.Cs_uri_stem = urls[index];
                this.cs_uri_query = querys[index];

            }
            else
            {
                Prase();
            }
        }
        private void Prase()
        {
            int length = PayLoad.Length;
            int fieldIndex = 0;
            List<byte> buf = new List<byte>();
            for (int offset = 0; offset < length;)
            {
                switch (fieldIndex)
                {
                    case 0:
                        EnabledFieldsFlags = BitConverter.ToInt32(PayLoad, offset);
                        offset += 4;
                        fieldIndex++;
                        break;
                    case 1:
                        Date = PraseUnicode(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 2:
                        Time = PraseUnicode(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 3:
                        C_ip = PraseUnicode(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 4:
                        Cs_username = PraseUnicode(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 5:
                        S_sitename = PraseUnicode(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 6:
                        s_computername = PraseUnicode(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 7:
                        S_ip = PraseUnicode(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 8:
                        Cs_method = PraseUTF8(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 9:
                        Cs_uri_stem = PraseUnicode(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 10:
                        cs_uri_query = PraseUTF8(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 11:
                        Sc_status = BitConverter.ToInt16(PayLoad, offset);
                        offset += 2;
                        fieldIndex++;
                        break;
                    case 12:
                        Sc_win32_status = BitConverter.ToInt32(PayLoad, offset);
                        offset += 4;
                        fieldIndex++;
                        break;
                    case 13:
                        Sc_bytes = BitConverter.ToInt64(PayLoad, offset);
                        offset += 8;
                        fieldIndex++;
                        break;
                    case 14:
                        Cs_bytes = BitConverter.ToInt64(PayLoad, offset);
                        offset += 8;
                        fieldIndex++;
                        break;
                    case 15:
                        Time_taken = BitConverter.ToInt64(PayLoad, offset);
                        offset += 8;
                        fieldIndex++;
                        break;
                    case 16:
                        S_port = BitConverter.ToInt16(PayLoad, offset);
                        offset += 2;
                        fieldIndex++;
                        break;
                    case 17:
                        CsUser_agent = PraseUTF8(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 18:
                        CsCookie = PraseUTF8(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 19:
                        CsReferer = PraseUTF8(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 20:
                        Cs_version = PraseUnicode(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 21:
                        Cs_host = PraseUTF8(ref offset, ref buf, ref fieldIndex);
                        break;
                    case 22:
                        Sc_substatus = BitConverter.ToInt16(PayLoad, offset);
                        offset += 2;
                        fieldIndex++;
                        break;
                    case 23:
                        CustomFields = PraseUnicode(ref offset, ref buf, ref fieldIndex);
                        break;
                    default:
                        offset = length;
                        break;
                }
            }
        }
        private string PraseUnicode(ref int offset, ref List<byte> buf, ref int fieldIndex)
        {
            string str = string.Empty;
            if (BitConverter.ToInt16(PayLoad, offset) != 0)
            {
                buf.Add(PayLoad[offset]);
                buf.Add(PayLoad[offset + 1]);
            }
            else
            {
                str = UnicodeEncoding.Unicode.GetString(buf.ToArray());
                buf = new List<byte>();
                fieldIndex++;
            }
            offset += 2;
            return str;
        }
        private string PraseUTF8(ref int offset, ref List<byte> buf, ref int fieldIndex)
        {
            string str = string.Empty;

            if (PayLoad[offset] != 0)
            {
                buf.Add(PayLoad[offset]);
            }
            else
            {
                str = Encoding.UTF8.GetString(buf.ToArray());
                buf = new List<byte>();
                fieldIndex++;
            }
            offset++;
            return str;
        }
        public int EnabledFieldsFlags
        {
            //get { return GetInt32At(GetOffsetForField(0)); }
            get;
            set;
        }
        public String Date
        {
            //get { return GetUnicodeStringAt(GetOffsetForField(1)); }
            get;
            set;
        }
        public String Time
        {
            //get { return GetUnicodeStringAt(GetOffsetForField(2)); }
            get; set;
        }
        public String C_ip
        {
            //get { return GetUnicodeStringAt(GetOffsetForField(3)); }
            get;
            set;
        }
        public String Cs_username
        {
            //get { return GetUnicodeStringAt(GetOffsetForField(4)); }
            get; set;
        }
        public String S_sitename
        {
            //get { return GetUnicodeStringAt(GetOffsetForField(5)); }
            get; set;
        }
        public string s_computername
        {
            //get { return getunicodestringat(getoffsetforfield(6)); }
            get; set;
        }
        public String S_ip
        {
            //get { return GetUnicodeStringAt(GetOffsetForField(7)); }
            get; set;
        }
        public String Cs_method
        {
            //get { return GetUTF8StringAt(GetOffsetForField(8)); }
            get; set;
        }
        public String Cs_uri_stem
        {
            //get { return GetUnicodeStringAt(GetOffsetForField(9)); }
            get; set;
        }
        public string cs_uri_query
        {
            //get { return getutf8stringat(getoffsetforfield(10)); }
            get; set;
        }
        public int Sc_status
        {
            //get { return GetInt16At(GetOffsetForField(11)); }
            get; set;
        }
        public int Sc_win32_status
        {
            //get { return GetInt32At(GetOffsetForField(12)); }
            get; set;
        }
        public long Sc_bytes
        {
            //get { return GetInt64At(GetOffsetForField(13)); }
            get; set;
        }
        public long Cs_bytes
        {
            //get { return GetInt64At(GetOffsetForField(14)); }
            get; set;
        }
        public long Time_taken
        {
            //get { return GetInt64At(GetOffsetForField(15)); }
            get; set;
        }
        public int S_port
        {
            //get { return GetInt16At(GetOffsetForField(16)); }
            get; set;
        }
        public String CsUser_agent
        {
            //get { return GetUTF8StringAt(GetOffsetForField(17)); }
            get; set;
        }
        public String CsCookie
        {
            //get { return GetUTF8StringAt(GetOffsetForField(18)); }
            get; set;
        }
        public String CsReferer
        {
            //get { return GetUTF8StringAt(GetOffsetForField(19)); }
            get; set;
        }
        public String Cs_version
        {
            //get { return GetUnicodeStringAt(GetOffsetForField(20)); }
            get; set;
        }
        public String Cs_host
        {
            //get { return GetUTF8StringAt(GetOffsetForField(21)); }
            get; set;
        }
        public int Sc_substatus
        {
            //get { return GetInt16At(GetOffsetForField(22)); }
            get; set;
        }
        public String CustomFields
        {
            //get { return GetUnicodeStringAt(GetOffsetForField(23)); }
            get; set;
        }

        enum Types
        {
            UInt16,
            UInt32,
            UInt64,
            UnicodeString,
            AnsiString
        }

        private static readonly Types[] FieldTypes = 
            new Types[] {
                Types.UInt32,
                Types.UnicodeString,
                Types.UnicodeString,
                Types.UnicodeString,
                Types.UnicodeString,
                Types.UnicodeString,
                Types.UnicodeString,
                Types.UnicodeString,
                Types.AnsiString,
                Types.UnicodeString,
                Types.AnsiString,
                Types.UInt16,
                Types.UInt32,
                Types.UInt64,
                Types.UInt64,
                Types.UInt64,
                Types.UInt16,
                Types.AnsiString,
                Types.AnsiString,
                Types.AnsiString,
                Types.UnicodeString,
                Types.AnsiString,
                Types.UInt16,
                Types.UnicodeString
            };
        
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.SqlTypes;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace APIServices.Common
{
    public static class DataHelper
    {
        #region BinaryData

        /// <summary>
        /// Chuyển một danh sách các guid thành binary.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static byte[] ToBinary(this IList<Guid> values)
        {
            if (values == null)
            {
                return null;
            }

            return values.SelectMany(d => d.ToByteArray()).ToArray();
        }

        /// <summary>
        /// Chuyển một binary thành danh sách guid.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<Guid> ToGuids(this byte[] values)
        {
            List<Guid> result = new List<Guid>();
            int length = Guid.Empty.ToByteArray().Length;

            if (values == null)
            {
                return result;
            }

            for (int i = 0; i < values.Length; i += length)
            {
                result.Add(new Guid(values.Skip(i).Take(length).ToArray()));
            }

            return result;
        }

        /// <summary>
        /// Chuyển một danh sách các số nguyên thành binary.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static byte[] ToBinary(this IList<int> values)
        {
            if (values == null)
            {
                return null;
            }

            if (values.Count() == 0)
            {
                return new byte[0];
            }

            int maxOrder = values.Max();
            int byteNumbers = (maxOrder / 8) + 1;
            byte[] bytes = new byte[byteNumbers];

            foreach (int value in values)
            {
                int index = value / 8; int offset = value % 8;
                bytes[index] = (byte)(bytes[index] | (1 << offset));
            }

            return bytes;
        }

        /// <summary>
        /// Chuyển một binary thành danh sách số nguyên.
        /// </summary>
        /// <param name="values">Danh sách số nguyên được chuyển thành binary.</param>
        /// <returns></returns>
        public static List<int> ToNumbers(this byte[] values)
        {
            List<int> result = new List<int>();

            if (values == null)
            {
                return result;
            }

            for (int i = 0; i < values.Length; i++)
            {
                byte temp = values[i];
                int count = 0;

                while (temp > 0)
                {
                    if (temp % 2 == 1)
                    {
                        result.Add(count + i * 8);
                    }

                    temp = (byte)(temp >> 1);
                    count++;
                }
            }

            return result;
        }

        /// <summary>
        /// Kiểm tra sự tồn tại của một số nguyên trong danh sách được chuyển thành binary.
        /// </summary>
        /// <param name="numbers">Danh sách số nguyên được chuyển thành binary.</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsContains(this byte[] numbers, int value)
        {
            if (value < 0 || numbers == null)
            {
                return false;
            }

            int index = value / 8;
            int offset = value % 8;

            if (index >= numbers.Length)
            {
                return false;
            }

            byte temp = (byte)((byte)1 << offset);
            return (numbers[index] & temp) > 0;
        }

        /// <summary>
        /// Sử dụng hàm này trong phân quyền người dùng.
        /// </summary>
        /// <param name="sumValue"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Contains(this int sumValue, int value)
        {
            //Mỗi value phải là kiểu số nguyên và có giá trị gấp đôi giá trị trước.
            //sumValue là tổng của các giá trị value được lưu trong database.
            //sumValue = 6 thì có quyền Delete (= 2) và quyền Modify (= 4).

            //ValueA chứa ValueB -> ValueA & ValueB = ValueB
            //ValueA chứa ValueB -> ValueA | ValueB = ValueA

            return (sumValue & value) == value;
        }

        #endregion

        #region SerializeData

        /// <summary>
        /// Chuyển một đối tượng bất kỳ thàng mảng bytes. 
        /// Có thể dùng trong việc gửi dữ liệu qua socket.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] SerializeData(this Object data)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, data);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Chuyển 1 mảng bytes đã được Serialize trước đó thành đối tượng gốc.
        /// Có thể dùng trong việc nhận dữ liệu từ socket.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static object DeserializeData(this byte[] bytes)
        {
            MemoryStream memoryStream = new MemoryStream(bytes);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            memoryStream.Position = 0;//tro ve vi tri dau tien cua bytes
            return binaryFormatter.Deserialize(memoryStream);
        }

        //Chuyển 1 string json đã được JSON.stringify() trên view sang Dictionary
        public static Dictionary<string, string> ConvertToDictionary(string jform)
        {
            dynamic collection = Newtonsoft.Json.JsonConvert.DeserializeObject(jform);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var c in collection)
            {
                string key = c.name.ToString();
                string value = c.value.ToString();
                dict[key] = dict.ContainsKey(key) ? dict[key] + "," + value : value;
            }

            return dict;
        }

        //Chuyển 1 list đối tượng sang DataTable, Column header được giữ nguyên upperCase/lowerCase của thuộc tính đối tượng
        public static DataTable ToDataTable<TEntity>(this IList<TEntity> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(TEntity));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                Type t = prop.PropertyType;
                t = Nullable.GetUnderlyingType(t) ?? t;
                table.Columns.Add(prop.Name, t);
            }
            object[] values = new object[props.Count];
            foreach (TEntity item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    var v = props[i].GetValue(item);
                    if (v != null && !Convert.IsDBNull(v))
                        values[i] = v;
                }
                table.Rows.Add(values);
            }
            return table;
        }

        #endregion

        #region String methods
        /// <summary>
        /// Chuyển đổi cuổi có dầu thành không dấu.
        /// </summary>
        /// <param name="strSrc"></param>
        /// <returns></returns>
        //public static string FilterVietkey(this string strSrc)
        //{
        //    if (!strSrc.IsNullOrEmpty())
        //    {
        //        return Globals.FilterVietkey(strSrc);
        //    }
        //    return strSrc;
        //}
        #endregion

        #region Object methods

        /// <summary>
        /// ToString một đối tượng không cần kiểm tra null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetString(this object value)
        {
            return value != null ? value.ToString() : string.Empty;
        }

        /// <summary>
        /// Trả về false nếu value bị null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetBoolean(this bool? value)
        {
            return value != null ? value.Value : false;
        }

        /// <summary>
        /// Trả về 0 nếu value bị null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetInteger(this int? value)
        {
            return value != null ? value.Value : 0;
        }

        /// <summary>
        /// Dùng cho kiểu Int16 và short.
        /// Trả về 0 nếu value bị null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static short GetShort(this short? value)
        {
            return value != null ? value.Value : (short)0;
        }

        /// <summary>
        /// Dùng cho kiểu Int64 và long.
        /// Trả về 0 nếu value bị null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long GetLong(this long? value)
        {
            return value != null ? value.Value : 0;
        }

        /// <summary>
        /// Trả về 0 nếu value bị null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal GetDecimal(this decimal? value)
        {
            return value != null ? value.Value : 0M;
        }

        /// <summary>
        /// Trả về 0 nếu value bị null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double GetDouble(this double? value)
        {
            return value != null ? value.Value : 0D;
        }

        /// <summary>
        /// Trả về 0 nếu value bị null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float GetFloat(this float? value)
        {
            return value != null ? value.Value : 0F;
        }

        /// <summary>
        /// Trả về Guid.Empty nếu value bị null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Guid GetGuid(this Guid? value)
        {
            return value != null ? value.Value : Guid.Empty;
        }

        /// <summary>
        /// Chuyển .net guid thành oracle guid hoặc ngược lại.
        /// Vì .net và oracle có cách đọc giá trị guid khác nhau.
        /// </summary>
        /// <param name="guidValue"></param>
        /// <returns></returns>
        public static Guid ReverseGuid(this Guid guidValue)
        {
            var guidBytes = guidValue.ToByteArray();
            var result = BitConverter.ToString(guidBytes);
            result = result.Replace("-", string.Empty);
            return new Guid(result);
        }

        /// <summary>
        /// Chuyển .net guid thành oracle guid hoặc ngược lại.
        /// Vì .net và oracle có cách đọc giá trị guid khác nhau.
        /// </summary>
        /// <param name="guidString"></param>
        /// <returns></returns>
        public static Guid ReverseGuid(this string guidString)
        {
            var guidValue = guidString.TryGetValue<Guid>();
            return guidValue.ReverseGuid();
        }

        public static TimeSpan GetTimeSpan(this TimeSpan? value)
        {
            return value != null ? value.Value : TimeSpan.Zero;
        }

        /// <summary>
        /// Trả về ngày hiện tại nếu value bị null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(this DateTime? value)
        {
            return value != null ? value.Value : GetEmptyDate();
        }

        /// <summary>
        /// Trả về ngày hiện tại nếu value bị null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeOrNow(this DateTime? value)
        {
            return value != null ? value.Value : DateTime.Now;
        }

        /// <summary>
        /// Trả về ngày nhỏ nhất nếu value bị null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeOrMin(this DateTime? value)
        {
            return value != null ? value.Value : SqlDateTime.MinValue.Value;
        }

        /// <summary>
        /// Trả về ngày lớn nhất nếu value bị null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeOrMax(this DateTime? value)
        {
            return value != null ? value.Value : SqlDateTime.MaxValue.Value;
        }

        public static DateTime GetEmptyDate()
        {
            return new DateTime(((long)(0)));
        }

        public static object RandomValue(this Type propertyType)
        {
            object result = null;

            if (propertyType.IsInteger() || propertyType.IsShort()
                || propertyType.IsLong() || propertyType.IsFloat()
                || propertyType.IsDouble() || propertyType.IsDecimal())
            {
                Random random = new Random();
                result = random.Next(0, int.MaxValue);
            }
            else if (propertyType.IsGuid())
            {
                result = Guid.NewGuid();
            }
            else if (propertyType.IsDateTime())
            {
                result = DateTime.Now;
            }
            else
            {
                result = TryGetValue(null, propertyType);
            }

            return result;
        }

        public static T TryGetValue<T>(this object value)
        {
            object resultValue = TryGetValue(value, typeof(T));
            return resultValue != null ? (T)resultValue : default(T);
        }

        /// <summary>
        /// Validate dữ liệu theo T để có dữ liệu đúng trước khi sử dụng.
        /// Ví dụ: value là chuỗi ký tự 13, T là int => result = số nguyên 13
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T TryGetValue<T>(this object value, out bool invalidFormat)
        {
            object resultValue = TryGetValue(value, typeof(T), out invalidFormat);
            return resultValue != null ? (T)resultValue : default(T);
        }

        /// <summary>
        /// Validate dữ liệu theo propertyType để có dữ liệu đúng trước khi sử dụng.
        /// Ví dụ: value là chuỗi ký tự 13, propertyType là int => result = số nguyên 13
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        public static object TryGetValue(this object value, Type propertyType)
        {
            bool invalidFormat = false;//kiểm tra đúng định dạng
            return TryGetValue(value, propertyType, out invalidFormat);
        }

        /// <summary>
        /// Validate dữ liệu theo propertyType để có dữ liệu đúng trước khi sử dụng.
        /// Ví dụ: value là chuỗi ký tự 13, propertyType là int => result = số nguyên 13
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyType"></param>
        /// <param name="invalidFormat"></param>
        /// <returns></returns>
        public static object TryGetValue(this object value,
            Type propertyType, out bool invalidFormat)
        {
            object resultValue = null;
            invalidFormat = false;

            if (value.GetString().ToLower().Equals("null"))
            {
                resultValue = null;//Dữ liệu bằng null
            }
            else if (propertyType != null)
            {
                if (value != null && value.GetType().GetRealPropertyType()
                    == propertyType.GetRealPropertyType())
                {
                    resultValue = value;//Cùng kiểu dữ liệu
                }
                else if (propertyType.IsEnum)
                {
                    resultValue = Enum.Parse(propertyType, value.GetString());
                }
                else if (propertyType.IsBoolean())
                {
                    if (value.GetString().TrimAll() == "1")
                    {
                        value = "true";
                    }
                    else if (value.GetString().TrimAll() == "0")
                    {
                        value = "false";
                    }

                    bool currentValue = false;//value không phải bool hoặc type là bool
                    if (bool.TryParse(value.GetString().TrimAll(), out currentValue))
                    {
                        resultValue = currentValue;
                    }
                    else
                    {
                        if (propertyType == typeof(bool))
                        {
                            resultValue = currentValue;
                        }

                        invalidFormat = true;
                    }
                }
                else if (propertyType.IsInteger())
                {
                    int currentValue = 0;//value không phải int hoặc type là int
                    if (int.TryParse(value.GetString().TrimAll(), out currentValue))
                    {
                        resultValue = currentValue;
                    }
                    else
                    {
                        if (propertyType == typeof(int))
                        {
                            resultValue = currentValue;
                        }

                        invalidFormat = true;
                    }
                }
                else if (propertyType.IsShort())
                {
                    short currentValue = 0;//value không phải int hoặc type là int
                    if (short.TryParse(value.GetString().TrimAll(), out currentValue))
                    {
                        resultValue = currentValue;
                    }
                    else
                    {
                        if (propertyType == typeof(short))
                        {
                            resultValue = currentValue;
                        }

                        invalidFormat = true;
                    }
                }
                else if (propertyType.IsLong())
                {
                    long currentValue = 0;//value không phải int hoặc type là int
                    if (long.TryParse(value.GetString().TrimAll(), out currentValue))
                    {
                        resultValue = currentValue;
                    }
                    else
                    {
                        if (propertyType == typeof(long))
                        {
                            resultValue = currentValue;
                        }

                        invalidFormat = true;
                    }
                }
                else if (propertyType.IsDecimal())
                {
                    decimal currentValue = 0;//value không phải decimal hoặc type là decimal
                    if (decimal.TryParse(value.GetString().TrimAll(), out currentValue))
                    {
                        resultValue = currentValue;
                    }
                    else
                    {
                        if (propertyType == typeof(decimal))
                        {
                            resultValue = currentValue;
                        }

                        invalidFormat = true;
                    }
                }
                else if (propertyType.IsFloat())
                {
                    float currentValue = 0;//value không phải float hoặc type là float
                    if (float.TryParse(value.GetString().TrimAll(), out currentValue))
                    {
                        resultValue = currentValue;
                    }
                    else
                    {
                        if (propertyType == typeof(float))
                        {
                            resultValue = currentValue;
                        }

                        invalidFormat = true;
                    }
                }
                else if (propertyType.IsDouble())
                {
                    double currentValue = 0;//value không phải double hoặc type là double
                    if (double.TryParse(value.GetString().TrimAll(), out currentValue))
                    {
                        resultValue = currentValue;
                    }
                    else
                    {
                        if (propertyType == typeof(double))
                        {
                            resultValue = currentValue;
                        }

                        invalidFormat = true;
                    }
                }
                else if (propertyType.IsGuid())
                {
                    Guid currentValue = Guid.Empty;//value không phải Guid hoặc type là Guid
                    if (Guid.TryParse(value.GetString().TrimAll(), out currentValue))
                    {
                        resultValue = currentValue;
                    }
                    else
                    {
                        if (propertyType == typeof(Guid))
                        {
                            resultValue = currentValue;
                        }

                        invalidFormat = true;
                    }
                }
                else if (propertyType.IsDateTime())
                {
                    //value không phải DateTime hoặc type là DateTime
                    DateTime currentValue = DateTime.MinValue;

                    if (DateTime.TryParse(value.GetString(), out currentValue))
                    {
                        resultValue = currentValue;
                    }
                    else
                    {
                        if (propertyType == typeof(DateTime))
                        {
                            resultValue = currentValue;
                        }

                        invalidFormat = true;
                    }
                }
                else if (propertyType.IsTimeSpan())
                {
                    //value không phải TimeSpan hoặc type là TimeSpan
                    TimeSpan currentValue = DateTime.Now.TimeOfDay;

                    if (TimeSpan.TryParse(value.GetString(), out currentValue))
                    {
                        resultValue = currentValue;
                    }
                    else
                    {
                        if (propertyType == typeof(TimeSpan))
                        {
                            resultValue = currentValue;
                        }

                        invalidFormat = true;
                    }
                }
                else if (propertyType == typeof(string))
                {
                    if (value != null)
                    {
                        resultValue = value.GetString();
                    }
                }
                else
                {
                    resultValue = value;
                }
            }
            else
            {
                resultValue = value;
            }

            return resultValue;
        }

        public static DateTime TryGetValue(this object value, string dateFormat)
        {
            bool invalidFormat = false;//kiểm tra đúng định dạng
            return TryGetValue(value, dateFormat, out invalidFormat);
        }

        public static DateTime TryGetValue(this object value, string dateFormat, out bool invalidFormat)
        {
            DateTime resultValue = DateTime.Now;

            if (string.IsNullOrWhiteSpace(dateFormat))
            {
                resultValue = value.TryGetValue<DateTime>(out invalidFormat);
            }
            else
            {
                invalidFormat = !DateTime.TryParseExact(value.GetString(), dateFormat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out resultValue);
            }

            return resultValue;
        }

        /// <summary>
        /// Kiểm tra hai giá trị có khác nhau hay không?
        /// </summary>
        /// <param name="originalValue">Gí trị trước đó.</param>
        /// <param name="currentValue">Giá trị hiện tại.</param>
        /// <returns></returns>
        public static bool HasChanged(this object originalValue, object currentValue)
        {
            return HasChanged(originalValue, currentValue, true);
        }

        /// <summary>
        /// Kiểm tra hai giá trị có khác nhau hay không?
        /// </summary>
        /// <param name="originalValue"></param>
        /// <param name="currentValue"></param>
        /// <param name="nullToZero">Bao gồm null to zero</param>
        /// <returns></returns>
        public static bool HasChanged(this object originalValue, object currentValue, bool nullToZero)
        {
            bool result = false;

            if (originalValue == null && (currentValue == null
                || (!nullToZero && currentValue.TryGetValue<decimal>() == 0M)))
            {
                result = false;
            }
            else if (originalValue == null && currentValue != null)
            {
                result = currentValue.GetType() == typeof(string) ?
                    !string.IsNullOrWhiteSpace(currentValue.GetString()) : true;
            }
            else if (originalValue != null && currentValue == null)
            {
                result = originalValue.GetType() == typeof(string) ?
                    !string.IsNullOrWhiteSpace(originalValue.GetString()) : true;
            }
            else if (!originalValue.Equals(currentValue) &&
                originalValue.GetHashCode() != currentValue.GetHashCode())
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// So sánh 2 mảng đối tượng
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static bool IsEquals(this Array array1, Array array2)
        {
            if (array1 == null && array2 == null)
                return true;

            if (array1 == null || array2 == null)
                return false;

            if (array1.GetType() != array2.GetType())
                return false;

            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1.GetValue(i).HasChanged(array2.GetValue(i)))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsOverlap(DateTime? dateFrom1, DateTime? dateTo1, DateTime? dateFrom2, DateTime? dateTo2)
        {
            dateFrom1 = dateFrom1.HasValue ? dateFrom1 : SqlDateTime.MinValue.Value;
            dateFrom2 = dateFrom2.HasValue ? dateFrom2 : SqlDateTime.MinValue.Value;

            dateTo1 = dateTo1.HasValue ? dateTo1 : SqlDateTime.MaxValue.Value;
            dateTo2 = dateTo2.HasValue ? dateTo2 : SqlDateTime.MaxValue.Value;

            return dateFrom1.Value <= dateTo2.Value && dateTo1.Value >= dateFrom2.Value;
        }

        /// <summary>
        /// So sánh hai đối tượng bất kỳ với nhau.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns>Nhỏ hơn 0 nếu value1 nhỏ hơn value2</returns>
        public static int Compare(this object value1, object value2)
        {
            if (value1 == value2)
            {
                return 0;//value1 = value2
            }
            else if (value1 == null && value2 != null)
            {
                return -1;//value1 < value2
            }
            else if (value1 != null && value2 == null)
            {
                return 1;//value1 > value2
            }
            else if (value1 != null && value2 != null)
            {
                Type propertyType = value1.GetType();

                if (propertyType == typeof(string) || propertyType == typeof(char))
                {
                    return value1.TryGetValue<string>().CompareTo(value2);
                }
                else if (propertyType == typeof(int) || propertyType == typeof(int?))
                {
                    return value1.TryGetValue<int>().CompareTo(value2);
                }
                else if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
                {
                    return value1.TryGetValue<decimal>().CompareTo(value2);
                }
                else if (propertyType == typeof(double) || propertyType == typeof(double?))
                {
                    return value1.TryGetValue<double>().CompareTo(value2);
                }
                else if (propertyType == typeof(float) || propertyType == typeof(float?))
                {
                    return value1.TryGetValue<float>().CompareTo(value2);
                }
                else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                {
                    return value1.TryGetValue<DateTime>().CompareTo(value2);
                }
                else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                {
                    return value1.TryGetValue<bool>().CompareTo(value2);
                }
                else if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                {
                    return value1.TryGetValue<Guid>().CompareTo(value2);
                }
                else if (propertyType == typeof(byte) || propertyType == typeof(byte?))
                {
                    return Convert.ToByte(value1.ToString()).CompareTo(value2);
                }
                else if (propertyType == typeof(sbyte) || propertyType == typeof(sbyte?))
                {
                    return Convert.ToSByte(value1.ToString()).CompareTo(value2);
                }
            }

            return 0;
        }

        #endregion

        #region GetPositiveValue

        /// <summary>
        /// Luôn trả về số dương cho dù value có âm hay không.
        /// Nếu value là số âm thì trả về số nguyên dương của nó.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetPositiveInteger(this int value)
        {
            return value < 0 ? -1 * value : value;
        }

        /// <summary>
        /// Luôn trả về số dương cho dù value có âm hay không.
        /// Nếu value là số âm thì trả về số nguyên dương của nó.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long GetPositiveLong(this long value)
        {
            return value < 0 ? -1L * value : value;
        }

        /// <summary>
        /// Luôn trả về số dương cho dù value có âm hay không.
        /// Nếu value là số âm thì trả về số nguyên dương của nó.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal GetPositiveDecimal(this decimal value)
        {
            return value < 0 ? -1M * value : value;
        }

        /// <summary>
        /// Luôn trả về số dương cho dù value có âm hay không.
        /// Nếu value là số âm thì trả về số nguyên dương của nó.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double GetPositiveDouble(this double value)
        {
            return value < 0 ? -1D * value : value;
        }

        /// <summary>
        /// Luôn trả về số dương cho dù value có âm hay không.
        /// Nếu value là số âm thì trả về số nguyên dương của nó.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float GetPositiveFloat(this float value)
        {
            return value < 0 ? -1F * value : value;
        }

        #endregion

        #region Validate Methods

        /// <summary>
        /// Kiểm tra giá trị value là null hay không?
        /// Nếu value là null, thì trả về true; Ngược lại, trả về false.
        /// </summary>
        /// <param name="value">Giá trị cần kiểm tra.</param>
        /// <returns>Nếu value là null, thì trả về true; Ngược lại, trả về false.</returns>
        public static bool IsNull(this object value)
        {
            return value == null || value == DBNull.Value;
        }

        public static bool IsBoolean(this Type value)
        {
            return value != null ? value == typeof(bool) || value == typeof(bool?) : false;
        }

        public static bool IsInteger(this Type value)
        {
            return value != null ? value == typeof(int) || value == typeof(int?) : false;
        }

        public static bool IsShort(this Type value)
        {
            return value != null ? value == typeof(short) || value == typeof(short?)
                || value == typeof(Int16) || value == typeof(Int16?) : false;
        }

        public static bool IsLong(this Type value)
        {
            return value != null ? value == typeof(long) || value == typeof(long?)
                || value == typeof(Int64) || value == typeof(Int64?) : false;
        }

        public static bool IsDecimal(this Type value)
        {
            return value != null ? value == typeof(decimal) || value == typeof(decimal?) : false;
        }

        public static bool IsDouble(this Type value)
        {
            return value != null ? value == typeof(double) || value == typeof(double?) : false;
        }

        public static bool IsFloat(this Type value)
        {
            return value != null ? value == typeof(float) || value == typeof(float?) : false;
        }

        public static bool IsGuid(this Type value)
        {
            return value != null ? value == typeof(Guid) || value == typeof(Guid?) : false;
        }

        public static bool IsDateTime(this Type value)
        {
            return value != null ? value == typeof(DateTime) || value == typeof(DateTime?) : false;
        }

        public static bool IsTimeSpan(this Type value)
        {
            return value != null ? value == typeof(TimeSpan) || value == typeof(TimeSpan?) : false;
        }

        public static bool IsNullable(this Type value)
        {
            try
            {
                return value.IsGenericType && value.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Property methods

        /// <summary>
        /// Kiểm tra một type có chứa thuộc tính propertyName?
        /// propertyName có thể là 2 thuộc tính: Parent.Field.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        private static bool HasProperty(this Type entityType,
            string propertyName, out Type propertyType)
        {
            bool result = false;
            propertyType = null;

            if (entityType != null && !string.IsNullOrWhiteSpace(propertyName))
            {
                string[] listPropertyName = propertyName.Split('.');

                foreach (string fieldName in listPropertyName)
                {
                    PropertyInfo propertyInFo = entityType.GetProperty(fieldName);
                    propertyType = propertyInFo != null ? propertyInFo.PropertyType : null;
                    entityType = propertyInFo != null ? propertyInFo.PropertyType : null;
                    result = propertyInFo != null;

                    if (!result)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Kiểm tra một type có chứa thuộc tính propertyName?
        /// propertyName có thể là 2 thuộc tính: Parent.Field.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool HasProperty(this Type entityType, string propertyName)
        {
            Type propertyType = null;//Kiểu dữ liệu của propertyName
            return entityType.HasProperty(propertyName, out propertyType);
        }

        /// <summary>
        /// Kiểm tra một đối tượng có chứa thuộc tính propertyName?
        /// propertyName có thể là 2 thuộc tính: Parent.Field
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool HasProperty(this object entity, string propertyName)
        {
            if (entity != null && !string.IsNullOrWhiteSpace(propertyName))
            {
                return entity.GetType().HasProperty(propertyName);
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra kiểu dữ liệu của thuộc tính propertyName.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Type GetPropertyType(this Type entityType, string propertyName)
        {
            Type propertyType = null;//Kiểu dữ liệu của propertyName
            entityType.HasProperty(propertyName, out propertyType);
            return propertyType;
        }

        /// <summary>
        /// Kiểm tra kiểu dữ liệu của thuộc tính propertyName.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Type GetPropertyType(this object entity, string propertyName)
        {
            if (entity != null && !string.IsNullOrWhiteSpace(propertyName))
            {
                Type propertyType = null;//Kiểu dữ liệu của propertyName
                entity.GetType().HasProperty(propertyName, out propertyType);
                return propertyType;
            }

            return null;
        }

        /// <summary>
        /// Kiểm tra kiểu dữ liệu thực của đối tượng propertyName.
        /// Ví dụ: propertyName là DateTime? => return DateTime.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Type GetRealPropertyType(this object entity, string propertyName)
        {
            return GetRealPropertyType(entity.GetPropertyType(propertyName));
        }

        /// <summary>
        /// Kiểm tra kiểu dữ liệu thực của đối tượng propetyType.
        /// Ví dụ: propetyType là DateTime? => return DateTime.
        /// </summary>
        /// <param name="propetyType"></param>
        /// <returns></returns>
        public static Type GetRealPropertyType(this Type propetyType)
        {
            if (propetyType.IsGuid())
            {
                return typeof(Guid);
            }
            else if (propetyType.IsInteger())
            {
                return typeof(int);
            }
            else if (propetyType.IsShort())
            {
                return typeof(Int16);
            }
            else if (propetyType.IsLong())
            {
                return typeof(Int64);
            }
            else if (propetyType.IsDouble())
            {
                return typeof(Double);
            }
            else if (propetyType.IsDecimal())
            {
                return typeof(Decimal);
            }
            else if (propetyType.IsFloat())
            {
                return typeof(float);
            }
            else if (propetyType.IsDateTime())
            {
                return typeof(DateTime);
            }
            else if (propetyType.IsBoolean())
            {
                return typeof(Boolean);
            }
            else
            {
                return propetyType;
            }
        }

        public static Type GetNullablePropertyType(this object entity, string propertyName)
        {
            return GetNullablePropertyType(entity.GetPropertyType(propertyName));
        }

        /// <summary>
        /// Trả về kiểu cho phép null.
        /// </summary>
        /// <param name="propetyType"></param>
        /// <returns></returns>
        public static Type GetNullablePropertyType(this Type propetyType)
        {
            if (propetyType.IsGuid())
            {
                return typeof(Guid?);
            }
            else if (propetyType.IsInteger())
            {
                return typeof(int?);
            }
            else if (propetyType.IsShort())
            {
                return typeof(Int16?);
            }
            else if (propetyType.IsLong())
            {
                return typeof(Int64?);
            }
            else if (propetyType.IsDouble())
            {
                return typeof(Double?);
            }
            else if (propetyType.IsDecimal())
            {
                return typeof(Decimal?);
            }
            else if (propetyType.IsFloat())
            {
                return typeof(float?);
            }
            else if (propetyType.IsDateTime())
            {
                return typeof(DateTime?);
            }
            else if (propetyType.IsBoolean())
            {
                return typeof(Boolean?);
            }
            else
            {
                return propetyType;
            }
        }

        /// <summary>
        /// Lấy giá trị của thuộc tính fieldName.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static object GetFieldValue(this object entity, string fieldName)
        {
            return GetFieldValue(entity, fieldName, BindingFlags.Default);
        }

        /// <summary>
        /// Lấy giá trị của thuộc tính fieldName.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fieldName"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        public static object GetFieldValue(this object entity, string fieldName, BindingFlags bindingAttr)
        {
            object result = null;

            if (entity != null && !string.IsNullOrWhiteSpace(fieldName))
            {
                FieldInfo fieldInfo = null;

                if (bindingAttr == BindingFlags.Default)
                {
                    fieldInfo = entity.GetType().GetField(fieldName);
                }
                else
                {
                    fieldInfo = entity.GetType().GetField(fieldName, bindingAttr);
                }

                if (fieldInfo != null)
                {
                    result = fieldInfo.GetValue(entity);
                }
            }

            return result;
        }

        /// <summary>
        /// Lấy giá trị của thuộc tính propertyName.
        /// propertyName có thể là 2 thuộc tính: Parent.Field
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetPropertyValue(this object entity, string propertyName)
        {
            return entity.GetPropertyValue(propertyName, BindingFlags.Default);
        }

        public static object GetPropertyValue(this object entity, string propertyName, BindingFlags bindingAttr)
        {
            object result = null;

            if (entity != null && !string.IsNullOrWhiteSpace(propertyName))
            {
                string[] listPropertyName = propertyName.Split('.');

                foreach (string fieldName in listPropertyName)
                {
                    PropertyInfo propertyInFo = null;

                    if (bindingAttr == BindingFlags.Default)
                    {
                        propertyInFo = entity.GetType().GetProperty(fieldName);
                    }
                    else
                    {
                        propertyInFo = entity.GetType().GetProperty(fieldName, bindingAttr);
                    }

                    if (propertyInFo != null && propertyInFo.CanRead == true)
                    {
                        result = entity = propertyInFo.GetValue(entity, null);
                    }
                    else
                    {
                        result = null;
                        break;
                    }

                    if (entity == null)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gán giá trị cho thuộc tính propertyName.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(this object entity, string propertyName, object value)
        {
            SetPropertyValue(entity, propertyName, value, BindingFlags.Default);
        }

        /// <summary>
        /// Gán giá trị cho thuộc tính propertyName.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <param name="bindingAttr"></param>
        public static void SetPropertyValue(this object entity, string propertyName, object value, BindingFlags bindingAttr)
        {
            if (entity != null && !string.IsNullOrWhiteSpace(propertyName))
            {
                PropertyInfo propertyInfo = null;

                if (bindingAttr == BindingFlags.Default)
                {
                    propertyInfo = entity.GetType().GetProperty(propertyName);
                }
                else
                {
                    propertyInfo = entity.GetType().GetProperty(propertyName, bindingAttr);
                }

                entity.SetPropertyValue(propertyInfo, value);
            }
        }

        /// <summary>
        /// Gán giá trị cho thuộc tính propertyName.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(this object entity, PropertyInfo propertyInfo, object value)
        {
            if (entity != null && propertyInfo != null && propertyInfo.CanWrite)
            {
                if (value != null && !string.IsNullOrEmpty(value.ToString())
                    && propertyInfo.PropertyType == typeof(XElement))
                {
                    value = XElement.Parse(value.ToString());
                }

                if (propertyInfo.PropertyType.IsEnum)
                {
                    value = Enum.ToObject(propertyInfo.PropertyType, value);
                }

                value = value == DBNull.Value ? null : value;
                propertyInfo.SetValue(entity, value, null);
            }
        }

        /// <summary>
        /// Gán giá trị cho thuộc tính fieldName.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public static void SetFieldValue(this object entity, string fieldName, object value)
        {
            SetFieldValue(entity, fieldName, value, BindingFlags.Default);
        }

        /// <summary>
        /// Gán giá trị cho thuộc tính fieldName.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="bindingAttr"></param>
        public static void SetFieldValue(this object entity, string fieldName, object value, BindingFlags bindingAttr)
        {
            if (entity != null && !string.IsNullOrWhiteSpace(fieldName))
            {
                FieldInfo fieldInfo = null;

                if (bindingAttr == BindingFlags.Default)
                {
                    fieldInfo = entity.GetType().GetField(fieldName);
                }
                else
                {
                    fieldInfo = entity.GetType().GetField(fieldName, bindingAttr);
                }

                entity.SetFieldValue(fieldInfo, value);
            }
        }

        /// <summary>
        /// Gán giá trị cho thuộc tính fieldName.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fieldInfo"></param>
        /// <param name="value"></param>
        public static void SetFieldValue(this object entity, FieldInfo fieldInfo, object value)
        {
            if (entity != null && fieldInfo != null)
            {
                if (value != null && !string.IsNullOrEmpty(value.ToString())
                    && fieldInfo.FieldType == typeof(XElement))
                {
                    value = XElement.Parse(value.ToString());
                }

                if (fieldInfo.FieldType.IsEnum)
                {
                    value = Enum.ToObject(fieldInfo.FieldType, value);
                }

                value = value == DBNull.Value ? null : value;
                fieldInfo.SetValue(entity, value);
            }
        }

        #endregion

        #region Invoke methods

        public static object InvokeMethod(this object entity, string methodName, params object[] parameters)
        {
            return InvokeMethod(entity, methodName, BindingFlags.Default, parameters);
        }

        public static object InvokeMethod(this object entity, string methodName,
            BindingFlags bindingAttr, params object[] parameters)
        {
            object result = null;

            if (entity != null && !string.IsNullOrWhiteSpace(methodName))
            {
                MethodInfo methodInfo = null;

                if (bindingAttr == BindingFlags.Default)
                {
                    methodInfo = entity.GetType().GetMethod(methodName);
                }
                else
                {
                    methodInfo = entity.GetType().GetMethod(methodName, bindingAttr);
                }

                result = methodInfo.Invoke(entity, parameters);
            }

            return result;
        }

        public static bool HasMethod(this Type entityType, string methodName)
        {
            bool result = false;

            if (entityType != null && !string.IsNullOrWhiteSpace(methodName))
            {
                MethodInfo methodInfo = entityType.GetMethod(methodName);
                result = methodInfo != null;
            }

            return result;
        }

        #endregion

        #region Instance methods

        /// <summary>
        /// Khởi tạo danh sách cho một kiểu dữ liệu elementType.
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        public static IList CreateList(this Type elementType)
        {
            Type listGenericType = typeof(List<>).MakeGenericType(elementType);
            return (IList)Activator.CreateInstance(listGenericType);
        }

        /// <summary>
        /// Tạo mới đối tượng có kiểu elementType.
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        public static object CreateInstance(this Type elementType)
        {
            return Activator.CreateInstance(elementType);
        }

        /// <summary>
        /// Tạo mới đối tượng có kiểu elementType và các tham số của nó.
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object CreateInstance(this Type elementType, params object[] args)
        {
            return Activator.CreateInstance(elementType, args);
        }

        /// <summary>
        /// Kiểm tra 1 đối tượng có phải được thừa kế từ 1 type nào đó?
        /// </summary>
        /// <param name="elementType">Đối tượng cần kiểm tra.</param>
        /// <param name="interfaceType">Type để kiểm tra.</param>
        /// <returns></returns>
        public static bool IsMemberOf(this Type elementType, Type parentType)
        {
            return elementType.GetInterface(parentType.Name) != null
                || elementType.IsSubclassOf(parentType);
        }

        /// <summary>
        /// Kiểm tra 1 đối tượng có phải là kiểu dữ liệu nào đó hay không?
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static bool IsTypeOf(this Type elementType, Type objectType)
        {
            return elementType == objectType || elementType.IsMemberOf(objectType);
        }

        public static bool IsTypeOf(this object value, Type objectType)
        {
            return value != null ? IsTypeOf(value.GetType(), objectType) : false;
        }

        /// <summary>
        /// Kiểm tra kiểu dữ liệu thực của một danh sách.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Type GetElementType(this IEnumerable list)
        {
            Type result = null;

            if (list != null)
            {
                foreach (var item in list)
                {
                    if (item != null)
                    {
                        result = item.GetType();
                        break;
                    }
                }

                if (result == null)
                {
                    result = list.AsQueryable().ElementType;
                }
            }

            return result;
        }

        /// <summary>
        /// Kiểu entity thực tế khi dùng entity framework có DynamicProxies.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static Type GetRealEntityType(this Type entityType)
        {
            if (entityType != null)
            {
                if (entityType.Namespace.EndsWith("DynamicProxies"))
                {
                    entityType = entityType.BaseType;
                }
            }

            return entityType;
        }

        #endregion

        #region Translate methods

        /// <summary>
        /// Convert data reader thành danh sách TEntity.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<TEntity> Translate<TEntity>(this DbDataReader reader, params string[] excludedFields)
        {
            IList listEntity = reader.Translate(typeof(TEntity), excludedFields);
            return listEntity.OfType<TEntity>().ToList();
        }

        public static IList Translate(this DbDataReader reader, params string[] excludedFields)
        {
            DataTable dataTable = reader.GetSchemaTable();
            DynamicTypeBuilder typeBuilder = new DynamicTypeBuilder(string.Empty);

            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                string propertyName = dataColumn.ColumnName;

                if (excludedFields == null || !excludedFields.Contains(propertyName))
                {
                    typeBuilder.AddProperty(propertyName, dataColumn.DataType);
                }
            }

            Type objectType = typeBuilder.BuildType();
            return Translate(reader, objectType, excludedFields);
        }

        /// <summary>
        /// Convert data reader thành danh sách objectType.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static IList Translate(this DbDataReader reader, Type objectType, params string[] excludedFields)
        {
            IList listEntity = objectType.CreateList();

            while (reader != null && reader.Read())
            {
                var entity = objectType.CreateInstance();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string propertyName = reader.GetName(i);//reader column name

                    if (excludedFields == null || !excludedFields.Contains(propertyName))
                    {
                        PropertyInfo propertyInfo = objectType.GetProperty(propertyName);

                        if (propertyInfo != null && propertyInfo.CanWrite && !reader.IsDBNull(i))
                        {
                            entity.SetPropertyValue(propertyInfo, reader.GetValue(i));
                        }
                    }
                }

                listEntity.Add(entity);
            }

            return listEntity;
        }

        /// <summary>
        /// Convert data reader thành danh sách TEntity.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static List<TEntity> Translate<TEntity>(this DataTable dataTable, params string[] excludedFields)
        {
            IList listEntity = dataTable.Translate(typeof(TEntity), excludedFields);
            return listEntity.OfType<TEntity>().ToList();
        }

        /// <summary>
        /// Convert data reader thành danh sách dynamic type.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="excludedFields"></param>
        /// <returns></returns>
        public static IList Translate(this DataTable dataTable, params string[] excludedFields)
        {
            DynamicTypeBuilder typeBuilder = new DynamicTypeBuilder(string.Empty);

            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                string propertyName = dataColumn.ColumnName;

                if (excludedFields == null || !excludedFields.Contains(propertyName))
                {
                    typeBuilder.AddProperty(propertyName, dataColumn.DataType);
                }
            }

            Type objectType = typeBuilder.BuildType();
            return Translate(dataTable, objectType, excludedFields);
        }

        /// <summary>
        /// Convert data reader thành danh sách objectType.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static IList Translate(this DataTable dataTable, Type objectType, params string[] excludedFields)
        {
            IList listEntity = objectType.CreateList();
            ParallelOptions options = new ParallelOptions();


            //Parallel.For(0, dataTable.Rows.Count, options, d =>
            //{
            //    DataRow dataRow = dataTable.Rows[d];
            //    var entity = objectType.CreateInstance();

            //    foreach (DataColumn dataColumn in dataTable.Columns)
            //    {
            //        string propertyName = dataColumn.ColumnName;

            //        if (excludedFields == null || !excludedFields.Contains(propertyName))
            //        {
            //            PropertyInfo propertyInfo = objectType.GetProperty(propertyName);

            //            if (propertyInfo != null && propertyInfo.CanWrite)
            //            {
            //                try
            //                {
            //                    entity.SetPropertyValue(propertyInfo, dataRow[dataColumn]);
            //                }
            //                catch (Exception ex)
            //                {
            //                    throw new Exception(ex.Message + " - " + dataColumn.ColumnName);
            //                }

            //            }
            //        }
            //    }

            //    lock (listEntity)
            //    {
            //        //Lock để khỏi xung đột
            //        listEntity.Add(entity);
            //    }
            //});

            //List<PropertyInfo> lstPropertyInfo = objectType.GetProperties().ToList();            

            foreach (DataRow dataRow in dataTable.Rows)
            {
                //DataRow dataRow = dataTable.Rows[d];
                var entity = objectType.CreateInstance();

                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    string propertyName = dataColumn.ColumnName;

                    if (excludedFields == null || !excludedFields.Contains(propertyName))
                    {

                        PropertyInfo propertyInfo = objectType.GetProperty(propertyName);
                        //PropertyInfo propertyInfo = lstPropertyInfo.FirstOrDefault(x => x.Name.ToUpper().Equals(propertyName));


                        if (propertyInfo != null && propertyInfo.CanWrite)
                        {
                            try
                            {
                                entity.SetPropertyValue(propertyInfo, dataRow[dataColumn]);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message + " - " + dataColumn.ColumnName);
                            }
                        }
                    }
                }

                lock (listEntity)
                {
                    //Lock để khỏi xung đột
                    listEntity.Add(entity);
                }
            }

            return listEntity;
        }

        /// <summary>
        /// Convert 1 danh sách bất kỳ thành danh sách objectType.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="listObject"></param>
        /// <returns></returns>
        public static List<TEntity> Translate<TEntity>(this IList listObject, params string[] excludedFields)
        {
            IList listEntity = listObject.Translate(typeof(TEntity), excludedFields);
            return listEntity.OfType<TEntity>().ToList();
        }

        /// <summary>
        /// Convert 1 danh sách bất kỳ thành danh sách objectType.
        /// </summary>
        /// <param name="listObject"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static IList Translate(this IList listObject, Type objectType, params string[] excludedFields)
        {
            IList listEntity = objectType.CreateList();
            ParallelOptions options = new ParallelOptions();

            if (listObject.GetElementType() == typeof(DbDataRecord))
            {
                Parallel.For(0, listObject.Count, options, d =>
                {
                    var dataRecord = (DbDataRecord)listObject[d];
                    var entity = objectType.CreateInstance();

                    for (int i = 0; i < dataRecord.FieldCount; i++)
                    {
                        string propertyName = dataRecord.GetName(i);//reader column name

                        if (excludedFields == null || !excludedFields.Contains(propertyName))
                        {
                            PropertyInfo propertyInfo = objectType.GetProperty(propertyName);

                            if (propertyInfo != null && propertyInfo.CanWrite && !dataRecord.IsDBNull(i))
                            {
                                entity.SetPropertyValue(propertyInfo, dataRecord.GetValue(i));
                            }
                        }
                    }

                    lock (listEntity)
                    {
                        //Lock để khỏi xung đột
                        listEntity.Add(entity);
                    }
                });
            }
            else
            {
                Parallel.For(0, listObject.Count, options, d =>
                {
                    var objectValue = listObject[d];
                    var entity = objectType.CreateInstance();
                    objectValue.CopyData(entity, excludedFields);

                    lock (listEntity)
                    {
                        //Lock để khỏi xung đột
                        listEntity.Add(entity);
                    }
                });
            }

            return listEntity;
        }

        public static DataTable Translate(this List<DataRow> listDataRow, params string[] excludedFields)
        {
            ParallelOptions options = new ParallelOptions();

            var dataTable = listDataRow.Select(d => d.Table).FirstOrDefault();
            DataTable dtResult = dataTable.Clone();//Sao chép cấu trúc

            Parallel.For(0, listDataRow.Count, options, d =>
            {
                DataRow dataRow = listDataRow[d];
                DataRow resultRow = null;

                lock (dtResult)
                {
                    //Lock để khỏi xung đột
                    resultRow = dtResult.NewRow();
                    dtResult.Rows.Add(resultRow);
                }

                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    string propertyName = dataColumn.ColumnName;

                    if (excludedFields == null || !excludedFields.Contains(propertyName))
                    {
                        resultRow[propertyName] = dataRow[dataColumn];
                    }
                }
            });

            return dtResult;
        }

        /// <summary>
        /// Convert danh sách đối tượng thành datatable.
        /// </summary>
        /// <param name="listObject">Dữ liệu cần convert.</param>
        /// <param name="fieldNames">Danh sách column mong muốn.</param>
        /// <returns></returns>
        public static DataTable Translate(this IList listObject, params string[] fieldNames)
        {
            return Translate(listObject, DataRowState.Added, fieldNames);
        }

        /// <summary>
        /// Convert danh sách đối tượng thành datatable.
        /// </summary>
        /// <param name="listObject">Dữ liệu cần convert.</param>
        /// <param name="isModifiedState"></param>
        /// <param name="fieldNames">Danh sách column mong muốn.</param>
        /// <returns></returns>
        public static DataTable Translate(this IList listObject,
            DataRowState rowState, params string[] fieldNames)
        {
            DataTable result = new DataTable();
            Type entityType = listObject.GetElementType();
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 1;//1 core

            if (listObject != null && entityType != null)
            {
                List<PropertyInfo> listColumn = new List<PropertyInfo>();
                PropertyInfo[] listProperty = entityType.GetProperties();
                listProperty = listProperty.OrderBy(d => d.MetadataToken).ToArray();

                //foreach (PropertyInfo propertyInfo in listProperty)
                if (fieldNames != null && fieldNames.Count() > 0)
                {
                    foreach (var field in fieldNames)
                    {
                        if (!string.IsNullOrEmpty(field))
                        {
                            var propertyInfo = listProperty.Where(s => s.Name == field).FirstOrDefault();
                            if (propertyInfo != null)
                            {
                                if (!propertyInfo.PropertyType.IsTypeOf(typeof(RelatedEnd))
                               && !propertyInfo.PropertyType.IsTypeOf(typeof(EntityState))
                               && !propertyInfo.PropertyType.IsTypeOf(typeof(EntityKey)))
                                {
                                    Type realType = propertyInfo.PropertyType.GetRealPropertyType();
                                    result.Columns.Add(propertyInfo.Name, realType);
                                    listColumn.Add(propertyInfo);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (PropertyInfo propertyInfo in listProperty)
                    {
                        if (!propertyInfo.PropertyType.IsTypeOf(typeof(RelatedEnd))
                            && !propertyInfo.PropertyType.IsTypeOf(typeof(EntityState))
                            && !propertyInfo.PropertyType.IsTypeOf(typeof(EntityKey)))
                        {
                            Type realType = propertyInfo.PropertyType.GetRealPropertyType();
                            result.Columns.Add(propertyInfo.Name, realType);
                            listColumn.Add(propertyInfo);
                        }
                    }
                }

                Parallel.For(0, listObject.Count, options, d =>
                {
                    var objectValue = listObject[d];
                    DataRow row = null;

                    lock (result)
                    {
                        //Lock để khỏi xung đột
                        row = result.NewRow();
                        result.Rows.Add(row);

                        if (rowState == DataRowState.Modified
                            || rowState == DataRowState.Deleted)
                        {
                            //Trước khi SetField
                            row.AcceptChanges();
                        }
                    }

                    foreach (PropertyInfo propertyInfo in listColumn)
                    {
                        object value = objectValue.GetPropertyValue(propertyInfo.Name);

                        if (value != null)
                        {
                            row.SetField(propertyInfo.Name, value);
                        }
                    }

                    if (rowState == DataRowState.Unchanged)
                    {
                        //Sau khi SetField
                        row.AcceptChanges();
                    }
                    else if (rowState == DataRowState.Detached
                        || rowState == DataRowState.Deleted)
                    {
                        row.Delete();
                    }
                });
            }

            return result;
        }

        #region DynamicTypeBuilder

        public class DynamicTypeBuilder
        {
            #region Properties

            private string _dynamicTypeName = string.Empty;
            private string _dynamicAssemblyName = string.Empty;
            private string _dynamicModuleName = string.Empty;
            Dictionary<string, Type> dynamicProperties;

            private Dictionary<string, Type> DynamicProperties
            {
                get
                {
                    if (dynamicProperties == null)
                    {
                        dynamicProperties = new Dictionary<string, Type>();
                    }

                    return dynamicProperties;
                }
                set { dynamicProperties = value; }
            }

            public string DynamicTypeName
            {
                get
                {
                    if (string.IsNullOrEmpty(_dynamicTypeName))
                    {
                        _dynamicTypeName = "DynamicTypeName";
                    }
                    return _dynamicTypeName;
                }
                set { _dynamicTypeName = value; }
            }

            public string DynamicAssemblyName
            {
                get
                {
                    if (string.IsNullOrEmpty(_dynamicAssemblyName))
                    {
                        _dynamicAssemblyName = "DynamicAssembly";
                    }
                    return _dynamicAssemblyName;
                }
                set { _dynamicAssemblyName = value; }
            }

            public string DynamicModuleName
            {
                get
                {
                    if (string.IsNullOrEmpty(_dynamicModuleName))
                    {
                        _dynamicModuleName = "DynamicModule.dll";
                    }
                    return _dynamicModuleName;
                }
                set { _dynamicModuleName = value; }
            }

            #endregion

            #region Constructors

            public DynamicTypeBuilder(string typeName)
            {
                _dynamicTypeName = typeName;
            }

            #endregion

            #region Methods

            public void AddProperty(string propertyName, Type propertyType)
            {
                DynamicProperties.Add(propertyName, propertyType);
            }

            public Type BuildType()
            {
                return BuildType(dynamicProperties);
            }

            public Type BuildType(Dictionary<string, Type> dynamicProperties)
            {
                AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new
                    AssemblyName(DynamicAssemblyName), AssemblyBuilderAccess.Run);

                ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(DynamicModuleName);
                TypeBuilder typeBuilder = moduleBuilder.DefineType(DynamicTypeName, TypeAttributes.Public);
                typeBuilder.SetParent(typeof(DynamicObject));

                foreach (var property in dynamicProperties)
                {
                    FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + property.Key, property.Value, FieldAttributes.Private);
                    PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(property.Key, System.Reflection.PropertyAttributes.HasDefault, property.Value, null);

                    MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
                    MethodBuilder getMethodBuilder = typeBuilder.DefineMethod("get_" + property.Key, getSetAttr, property.Value, Type.EmptyTypes);
                    ILGenerator getter = getMethodBuilder.GetILGenerator();

                    getter.Emit(OpCodes.Ldarg_0);
                    getter.Emit(OpCodes.Ldfld, fieldBuilder);
                    getter.Emit(OpCodes.Ret);

                    MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_" + property.Key, getSetAttr, null, new Type[] { property.Value });
                    ILGenerator setter = setMethodBuilder.GetILGenerator();

                    setter.Emit(OpCodes.Ldarg_0);
                    setter.Emit(OpCodes.Ldarg_1);
                    setter.Emit(OpCodes.Stfld, fieldBuilder);
                    setter.Emit(OpCodes.Ret);

                    propertyBuilder.SetGetMethod(getMethodBuilder);
                    propertyBuilder.SetSetMethod(setMethodBuilder);
                }

                return typeBuilder.CreateType();
            }

            public Type BuildType(params string[] propertyNames)
            {
                return BuildType(null, propertyNames);
            }

            /// <summary>
            /// Tạo dynamic type có các thuộc tính như entityType
            /// và thêm vào 1 số thuộc tính như propertyNames.
            /// </summary>
            /// <param name="entityType"></param>
            /// <param name="propertyNames"></param>
            /// <returns></returns>
            public Type BuildType(Type entityType, params string[] propertyNames)
            {
                var dynamicProperties = new Dictionary<string, Type>();

                if (entityType != null)
                {
                    var listProperty = entityType.GetProperties();

                    foreach (var property in listProperty)
                    {
                        dynamicProperties.Add(property.Name, property.PropertyType);
                    }
                }

                foreach (var propertyName in propertyNames)
                {
                    dynamicProperties.Add(propertyName, typeof(object));
                }

                return BuildType(dynamicProperties);
            }
            #endregion
        }

        #endregion

        #region CopyData methods

        /// <summary>
        /// Copy dữ liệu của một đối tượng có sẵn.
        /// </summary>
        /// <typeparam name="TEntity">Loại dữ liệu trả về.</typeparam>
        /// <param name="objectSource">Đối tượng copy.</param>
        /// <param name="excludedFields">Những fields không copy.
        /// Mặc định sẽ loại trừ các khóa ngoại.</param>
        /// <returns></returns>
        public static TEntity CopyData<TEntity>(this object objectSource,
            params string[] excludedFields)
        {
            TEntity objectResult = Activator.CreateInstance<TEntity>();
            objectSource.CopyData(objectResult, excludedFields);
            return objectResult;
        }

        /// <summary>
        /// Copy dữ liệu của một đối tượng có sẵn.
        /// </summary>
        /// <param name="objectSource">Đối tượng copy.</param>
        /// <param name="excludedFields">Những fields không copy.
        /// Mặc định sẽ loại trừ các khóa ngoại.</param>
        /// <returns></returns>
        public static object CopyData(this object objectSource,
            params string[] excludedFields)
        {
            object objectResult = null;

            if (objectSource != null)
            {
                Type objectType = objectSource.GetType();
                objectResult = objectType.CreateInstance();
                objectSource.CopyData(objectResult, excludedFields);
            }

            return objectResult;
        }

        /// <summary>
        /// Copy dữ liệu của một đối tượng có sẵn.
        /// </summary>
        /// <param name="objectSource">Đối tượng copy.</param>
        /// <param name="objectResult">Kết quả sau copy.</param>
        /// <param name="excludedFields">Những fields không copy.
        /// Mặc định sẽ loại trừ các khóa ngoại.</param>
        public static void CopyData(this object objectSource,
            object objectResult, params string[] excludedFields)
        {
            if (objectSource != null && objectResult != null)
            {
                Type objectSourceType = objectSource.GetType();
                Type objectResultType = objectResult.GetType();

                //Luôn for qua sourceProperties để copy dữ liệu mới chính xác
                PropertyInfo[] sourceProperties = objectSourceType.GetProperties();
                PropertyInfo[] resultProperties = objectResultType.GetProperties();

                foreach (PropertyInfo sourceProperty in sourceProperties)
                {
                    if (sourceProperty != null && sourceProperty.CanRead)
                    {
                        PropertyInfo resultProperty = resultProperties.Where(d => d.Name == sourceProperty.Name
                            && d.CanWrite && d.PropertyType == sourceProperty.PropertyType).FirstOrDefault();

                        if (resultProperty != null)
                        {
                            if (!resultProperty.PropertyType.IsTypeOf(typeof(EntityObject))
                                && !resultProperty.PropertyType.IsTypeOf(typeof(RelatedEnd))
                                && !resultProperty.PropertyType.IsTypeOf(typeof(EntityState))
                                && !resultProperty.PropertyType.IsTypeOf(typeof(EntityKey)))
                            {
                                if (excludedFields != null && excludedFields.Count() > 0)
                                {
                                    if (excludedFields.Contains(sourceProperty.Name))
                                    {
                                        resultProperty = null;
                                    }
                                }

                                if (resultProperty != null && resultProperty.CanWrite)
                                {
                                    var valueSource = objectSource.GetPropertyValue(sourceProperty.Name);
                                    var valueResult = objectResult.GetPropertyValue(resultProperty.Name);

                                    if (valueSource.HasChanged(valueResult))
                                    {
                                        objectResult.SetPropertyValue(resultProperty, valueSource);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        public static DataTable ToUpperColumnName(this DataTable table)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                table.Columns[i].ColumnName = table.Columns[i].ColumnName.ToUpper();
            }
            return table;
        }
        public static string TrimAll(this string value)
        {
            string result = string.Empty;

            if (value != null)
            {
                result = value;

                while (result.Contains(" "))
                {
                    result = result.Replace(" ", "");
                }
            }

            return result;
        }
        #endregion
    }
}

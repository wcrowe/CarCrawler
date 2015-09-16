﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Net.Mail;
using DataAccess;
using DataAccess.Entities;
using NLog;
using System.Linq;

namespace RSSRetrieveService
{
    public class CcDal : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool _disposed;
        public readonly DataAccess.Data dataAccess;
        private readonly List<DataAccess.Entities.CarDetail> emptyList;
        private readonly List<Feed> feeds;
        private readonly List<Predicate> predicates;
        public CcDal()
        {
            dataAccess = new Data();
         //   dataAccess.CleanTables();
            emptyList = dataAccess.GetEmptyDetail();
            feeds = dataAccess.GetActiveFeeds();
            predicates = dataAccess.GetPredicates();
        }
        public void FillCarDetails()
        {
            try
            {
                dataAccess.CleanDB();
                Logger.Debug("Filling Details = {0}", DateTime.Now.ToString("MMM ddd d HH:mm yyyy"));
                FillMillage();
                FillYears();
                FillMake();
                FillModel();
                FillPrice();
                foreach (var car in emptyList)
                {
                    dataAccess.UpdateCarDetail(car);
                    var retval = dataAccess.CheckForDups(car.Id);
           

                }
                SendOutEmails();
                SendMail();

            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

   

        private void FillMillage()
        {

            var regExMiles = dataAccess.GetRegEx("Miles");

            foreach (var regEx in regExMiles)
            {

                var regex = new Regex(
                    regEx.RegExExpression,
                    RegexOptions.IgnoreCase
                    | RegexOptions.Singleline
                    | RegexOptions.CultureInvariant
                    | RegexOptions.Compiled
                    );
                Match m;

                string tempString;
                string intString;
                int miles;
                int tempMiles;
                foreach (var emptyColumn in emptyList)
                {

                    m = regex.Match(emptyColumn.Title);
                    if (m.Success)
                    {

                        tempString = m.Value;
                        tempString = tempString.ToLower().Replace(".5k", "500");
                        tempString = tempString.ToLower().Replace("k", "000");
                        tempString = tempString.ToLower().Replace("xxx", "000");

                        intString = RemoveNonNumeric(tempString);
                        if (Int32.TryParse(intString, out tempMiles))
                        {

                            if (tempMiles > 3000)
                            {
                                miles = tempMiles;
                                emptyColumn.Miles = miles;
                                continue;
                            }
                        }
                    }

                }


                foreach (var emptyColumn in emptyList.Where(x => x.Miles == (int?)null))
                {
                    m = regex.Match(emptyColumn.Description);
                    if (m.Success)
                    {
                        tempString = m.Value;
                        tempString = tempString.ToLower().Replace(".5k", "500");
                        tempString = tempString.ToLower().Replace("k", "000");
                        tempString = tempString.ToLower().Replace("xxx", "000");

                        intString = RemoveNonNumeric(tempString);
                        if (Int32.TryParse(intString, out tempMiles))
                        {

                            if (tempMiles > 3000)
                            {
                                miles = tempMiles;
                                emptyColumn.Miles = miles;
                                continue;
                            }
                        }
                    }
                }
            }
        }



        private void FillMake()
        {
            try
            {

                foreach (var car in emptyList.Where(x => x.Year != (short?)null))
                {
                    var makes = dataAccess.GetDistinctMakes(car.Year.Value);
                    if (makes.Count > 0)
                    {
                        foreach (var make in makes)
                        {

                            if (car.Title.ToUpper().Contains(make.ToUpper()))
                            {
                                car.Make = make;

                            }
                        }

                    }
                }

                foreach (var car in emptyList.Where(x => x.Year != (short?)null))
                {
                    var makes = dataAccess.GetDistinctMakes(car.Year.Value);
                    if (makes.Count > 0)
                    {
                        foreach (var make in makes)
                        {

                            if (car.Description.ToUpper().Contains(make.ToUpper()))
                            {
                                car.Make = make;
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        private void FillModel()
        {
            try
            {

                foreach (var car in emptyList.Where(x => x.Year != (short?)null && x.Make != null))
                {
                    var models = dataAccess.GetDistinctModel(car.Year.Value, car.Make);
                    if (models.Count > 0)
                    {

                        foreach (var model in models)
                        {

                            var carmodels = model.Split(' ');
                            var carfound = false;
                            string stringmodel = "";
                            for (int i = 0; i < carmodels.Length; i++)
                            {
                                if (car.Title.ToUpper().Contains(carmodels[i].ToUpper()))
                                {
                                    if (carfound)
                                    {
                                        stringmodel += " " + carmodels[i];
                                    }
                                    else
                                    {
                                        stringmodel += carmodels[0];
                                        carfound = true;

                                    }

                                }

                            }
                            if (carfound)
                            {
                                car.Model = stringmodel;
                                break;
                            }

                        }

                    }
                }

                foreach (var car in emptyList.Where(x => x.Year != (short?)null && x.Make != null))
                {
                    var models = dataAccess.GetDistinctModel(car.Year.Value, car.Make);
                    if (models.Count > 0)
                    {

                        foreach (var model in models)
                        {

                            var carmodels = model.Split(' ');
                            var carfound = false;
                            string stringmodel = "";
                            for (int i = 0; i < carmodels.Length; i++)
                            {
                                if (car.Description.ToUpper().Contains(carmodels[i].ToUpper()))
                                {
                                    if (carfound)
                                    {
                                        stringmodel += " " + carmodels[i];
                                    }
                                    else
                                    {
                                        stringmodel += carmodels[0];
                                        carfound = true;

                                    }

                                }

                            }
                            if (carfound)
                            {
                                car.Model = stringmodel;
                                break;
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        private void FillYears()
        {

            try
            {


                foreach (var emptyColumn in emptyList)
                {
                    var tempstr = emptyColumn.Title.Substring(0, 4);
                    short tempint;
                    if (Int16.TryParse(tempstr, out tempint))
                    {
                        if (tempint > 1930 && tempint <= DateTime.Now.Year)
                        {
                            emptyColumn.Year = tempint;
                            continue;
                        }

                    }
                }


                foreach (var emptyColumn in emptyList)
                {
                    var tempstr = emptyColumn.Title.Substring(0, 2);
                    short tempint;
                    if (Int16.TryParse(tempstr, out tempint))
                    {
                        if (tempint > 30)
                            tempint = (short)(tempint + 1900);
                        else
                        {
                            tempint = (short)(tempint + 2000);
                        }
                        if (tempint <= DateTime.Now.Year)
                        {
                            emptyColumn.Year = tempint;
                            continue;
                        }

                    }
                }

                var regExMiles = dataAccess.GetRegEx("Year");

                foreach (var regEx in regExMiles)
                {

                    var regex = new Regex(
                        regEx.RegExExpression,
                        RegexOptions.IgnoreCase
                        | RegexOptions.Singleline
                        | RegexOptions.CultureInvariant
                        | RegexOptions.Compiled
                        );
                    Match m;

                    short tempInt;
                    foreach (var emptyColumn in emptyList)
                    {


                        m = regex.Match(emptyColumn.Title);
                        if (m.Success)
                        {
                            if (Int16.TryParse(m.Value, out tempInt))
                            {
                                emptyColumn.Year = tempInt;

                            }
                        }

                    }


                    foreach (var emptyColumn in emptyList)
                    {

                        m = regex.Match(emptyColumn.Description);
                        if (m.Success)
                        {
                            if (Int16.TryParse(m.Value, out tempInt))
                            {
                                emptyColumn.Year = tempInt;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        private void FillPrice()
        {

            try
            {
                var regExMiles = dataAccess.GetRegEx("Price");

                foreach (var regEx in regExMiles)
                {

                    var regex = new Regex(
                        regEx.RegExExpression,
                        RegexOptions.IgnoreCase
                        | RegexOptions.Singleline
                        | RegexOptions.CultureInvariant
                        | RegexOptions.Compiled
                        );
                    Match m;

                    decimal tempInt;
                    foreach (var emptyColumn in emptyList)
                    {


                        m = regex.Match(emptyColumn.Title);

                        if (m.Success)
                        {
                            if (decimal.TryParse(m.Value.Replace("$", "").Replace(",", ""), out tempInt))
                            {
                                emptyColumn.Price = tempInt;
                                continue;

                            }
                        }

                    }


                    foreach (var emptyColumn in emptyList)
                    {

                        m = regex.Match(emptyColumn.Description);
                        if (m.Success)
                        {
                            if (decimal.TryParse(m.Value.Replace("$", "").Replace(",", ""), out tempInt))
                            {
                                emptyColumn.Price = tempInt;
                                continue;

                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        private void WriteHtml(IEnumerable<Car> values, string subject)
        {
            
            var strBldr = new StringBuilder();
            var strWriter = new StringWriter(strBldr);
            var writer = new HtmlTextWriter(strWriter);

            writer.RenderBeginTag(HtmlTextWriterTag.Html);

            // <head>
            writer.RenderBeginTag(HtmlTextWriterTag.Head);

            writer.AddAttribute("type", "text/css");
            writer.RenderBeginTag(HtmlTextWriterTag.Style);
            writer.Write("table { border-collapse: separate; }");
            writer.Write("border-spacing: 0 5px; }");
            writer.RenderEndTag();

            // </head>
            writer.RenderEndTag();

            // <body>



            writer.RenderBeginTag(HtmlTextWriterTag.Body);

            writer.AddStyleAttribute("width", " 100%");

            writer.RenderBeginTag(HtmlTextWriterTag.Table); //Main Table
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            var counter = 0;
            foreach (var val in values)
            {
                
                if(val.EmailSent)
                    continue;
 
                writer.AddStyleAttribute("width", " 100%");

                writer.AddStyleAttribute("background-color", counter % 2 == 0 ? " #3399FF" : " #999966");

                writer.RenderBeginTag(HtmlTextWriterTag.Table);

                //writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                //writer.RenderBeginTag(HtmlTextWriterTag.Td);
                //writer.Write("&nbsp;");
                //writer.RenderEndTag();
                //writer.RenderEndTag();

                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                writer.AddStyleAttribute("width", " 100%");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);

                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write(val.DateIn.ToString("MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture));
                writer.RenderEndTag();

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                // To do
                writer.Write(feeds.Where(x => x.Id == val.FeedId).FirstOrDefault().FeedCity);

                writer.RenderEndTag();

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                // To Do
                writer.Write(feeds.Where(x => x.Id == val.FeedId).FirstOrDefault().FeedState);
                writer.RenderEndTag();

                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write(val.Title);
                writer.RenderEndTag();
                writer.RenderEndTag();



                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write(val.Description);
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, val.Link);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(val.Link);
                writer.RenderEndTag(); //A
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderEndTag();
                counter++;
            }


            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag(); //End Main Table



            // </body>
            writer.RenderEndTag();
            // </html>
            writer.RenderEndTag();

            var html = strBldr.ToString();
            var email = new Email
                {
                    EmailGeneratedDate = DateTime.Now,
                    EmailMessage = html,
                    EmailSent = false,
                    EmailSubject = subject
                };
            //  Logger.Debug(html);
            int id = dataAccess.InsertEmail(email);
            foreach (var v in values)
            {
     
                var batch = new EmailBatch { EmailId = id, CarId = v.Id };
                dataAccess.InsertEmailBatch(batch);
                dataAccess.UpdateMailSent(v.Id);
                v.EmailSent = true;
            }


        }

        private void SendOutEmails()
        {
            foreach (var q in dataAccess.GetEmailQueries().FindAll(x => x.Email == true))
            {
                
                var emails = dataAccess.GetEmailsToSend(GenSqlStatementForEmails(q.Id));
                Logger.Debug("Total from query {0} = {1}.", q.Subject, emails.Count.ToString());
                var emailss = emails.Where(e => e.EmailSent == false);
                Logger.Debug("Total after Lambda  {0} = {1}.", q.Subject, emailss.Count().ToString());
                var batches = emailss.Partition(10);
                foreach (var batch in batches)
                {
                    WriteHtml(batch, q.Subject);

                }
            }
        }

        private string GenSqlStatementForEmails(Int16 id)
        {
            var query = dataAccess.GetQueryById(id);
            var keywords = query.TitleAndDescripton.Split(',');
            for (int i = 0; i < keywords.Length; i++)
            {
                if (keywords[i].Contains(" "))
                {
                    keywords[i] = "\"" + keywords[i] + "\"";

                }
            }
            var contains = string.Empty;
            if (!string.IsNullOrEmpty(query.TitleAndDescripton.Trim()))
            {
                contains = "(contains([title],'";

                foreach (var word in keywords)
                {
                    if (query.AndOr == 1)
                    {
                        contains += word.Trim() + " And ";
                    }
                    else
                    {
                        contains += word.Trim() + " Or ";
                    }
                }
                if (contains.EndsWith(" And "))
                    contains = contains.Substring(0, contains.Length - 5);
                if (contains.EndsWith(" Or "))
                    contains = contains.Substring(0, contains.Length - 4);

                contains += "')";
                contains += " or contains([description],'";
                foreach (var word in keywords)
                {
                    if (query.AndOr == 1)
                    {
                        contains += word.Trim() + " And ";
                    }
                    else
                    {
                        contains += word.Trim() + " Or ";
                    }
                }
                if (contains.EndsWith(" And "))
                    contains = contains.Substring(0, contains.Length - 5);
                if (contains.EndsWith(" Or "))
                    contains = contains.Substring(0, contains.Length - 4);
                if (keywords.Length > 0)
                {
                    contains += "'))";
                }

            }
            if (string.IsNullOrEmpty(query.Ignore))
                query.Ignore = string.Empty;
            else
            {
                query.Ignore = query.Ignore.Trim();
            }
            var ignore = query.Ignore.Split(',');
            for (int i = 0; i < ignore.Length; i++)
            {
                if (ignore[i].Contains(" "))
                {
                    ignore[i] = "\"" + ignore[i] + "\"";

                }
            }





            if (!string.IsNullOrEmpty(query.Ignore))
            {
                if (!string.IsNullOrEmpty(contains))
                {
                    contains += " and (not contains([title],'";
                }
                else
                {
                    contains = "(not contains([title],'";
                }
                foreach (var word in ignore)
                {

                    contains += word.Trim() + " And ";


                }
                if (contains.EndsWith(" And "))
                    contains = contains.Substring(0, contains.Length - 5);


                contains += "')";
                contains += " and not contains([description],'";
                foreach (var word in ignore)
                {

                    contains += word.Trim() + " And ";

                }
                if (contains.EndsWith(" And "))
                    contains = contains.Substring(0, contains.Length - 5);
                if (contains.EndsWith(" Or "))
                    contains = contains.Substring(0, contains.Length - 4);
                contains += "'))";


            }
            if (query.MakePredicate > 0)
            {
                contains = BuildStringPredicate(contains, query.MakePredicate.Value, query.MakeValue, "Make", query.MakeAllowNull);

            }
            if (query.ModelPredicate > 0)
            {
                contains = BuildStringPredicate(contains, query.ModelPredicate.Value, query.ModelValue, "Model", query.ModelAllowNull);

            }
            if (query.MilesPredicate > 0)
            {
                contains = BuildNumberPredicate(contains, query.MilesPredicate.Value, query.MilesValue, "Miles", query.MilesAllowNull);

            }
            if (query.PricePredicate > 0)
            {
                contains = BuildNumberPredicate(contains, query.PricePredicate.Value, query.PriceValue, "Price", query.PriceAllowNull);

            }
            if (query.YearPredicate > 0)
            {
                contains = BuildNumberPredicate(contains, query.YearPredicate.Value, query.YearValue, "Year", query.YearAllowNull);

            }

            var sqlStr = "select * from car where " + contains + " and (EmailSent = 0) and (datediff(day, DateIn, getdate()) < " + Properties.Settings.Default.maxemaildate + ")"; //(FeedId in (select Id From Feed where FeedActive = 1))
            Logger.Debug(sqlStr);
            return sqlStr;
        }

        private string BuildStringPredicate(string contains, byte predicate, string value, string name, bool allowNulls)
        {

            var tmpPredicate = predicates.FirstOrDefault(e => e.Id == predicate);
            var tempvalue = value.Split(',');
            var tempcontains = "(";
            if (tempvalue.Length == 2)
            {
                tempcontains += string.Format(tmpPredicate.PredicateFormat, name + "'", tempvalue[0].Trim(), "'" + tempvalue[1].Trim()) +
                                (allowNulls
                                     ? "' or " + name + " is null"
                                     : "'") + ")";
            }
            else
            {
                tempcontains += string.Format(tmpPredicate.PredicateFormat, name, "'" + tempvalue[0] + "'") +
                               (allowNulls
                                    ? " or " + name + " is null"
                                    : "") + ")";
            }

            if (contains.Length > 0)
            {
                contains += "and " + tempcontains;
            }
            else
            {
                contains = tempcontains;
            }
            return contains;
        }
        private string BuildNumberPredicate(string contains, byte predicate, string value, string name, bool allowNulls)
        {

            var tmpPredicate = predicates.FirstOrDefault(e => e.Id == predicate);
            var tempvalue = value.Split(',');
            var tempcontains = "(";
            if (tempvalue.Length == 2)
            {
                tempcontains += string.Format(tmpPredicate.PredicateFormat, name, tempvalue[0].Trim(), tempvalue[1].Trim()) +
                                (allowNulls
                                     ? " or " + name + " is null"
                                     : "") + ")";
            }
            else
            {
                tempcontains += string.Format(tmpPredicate.PredicateFormat, name, tempvalue[0]) +
                               (allowNulls
                                    ? " or " + name + " is null"
                                    : "") + ")";
            }

            if (contains.Length > 0)
            {
                contains += " and " + tempcontains;
            }
            else
            {
                contains = tempcontains;
            }
            return contains;
        }
        private void SendMail()
        {
            Logger.Info("Sending Email {0}",DateTime.Now);
            var batches = dataAccess.GetBatchemailsToSend();
            foreach (var batch in batches)
            {
                if(batch.EmailSent.Value == true)
                    continue;
                var message = new MailMessage(Properties.Settings.Default.mailfrom, Properties.Settings.Default.mailto);
                message.IsBodyHtml = true;
                if (batch.EmailGeneratedDate != null)
                    message.Subject =
                        string.Format(batch.EmailSubject + " " +
                                      batch.EmailGeneratedDate.Value.ToString("MM/dd/yyyy hh:mm:ss tt",
                                                                              CultureInfo.InvariantCulture));
                message.Body = batch.EmailMessage;
                var client = new SmtpClient(Properties.Settings.Default.smptserver);
                client.Port = Properties.Settings.Default.smtpport;

                client.Credentials = new NetworkCredential(Properties.Settings.Default.smtpuser,
                                                           Properties.Settings.Default.smptpassword);
                client.EnableSsl = Properties.Settings.Default.smtpssl;

                //var client = new SmtpClient("localhost");
                //client.Port = 25;

                // Credentials are necessary if the server requires the client 
                // to authenticate before it will send e-mail on the client's behalf.
                if (!Properties.Settings.Default.sendemail)
                {
                    return;
                }
                client.Send(message);
                dataAccess.BatchEmailSent(batch.Id);
                var random = new Random();
                int randomNumber = random.Next(1000, 5000);
                System.Threading.Thread.Sleep(randomNumber);
            }
        }



        /// <summary>
        ///  Using an array and a foreach loop. Fastest in .NET 4.0
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private string RemoveNonNumeric(string word)
        {
            int x = 0;
            var chars = new char[word.Length];
            // ReSharper disable RedundantStringToCharArrayCall
            foreach (char c in word.ToCharArray())
            // ReSharper restore RedundantStringToCharArrayCall
            {
                if (Char.IsDigit(c))
                {
                    chars[x] = c;
                    x++;
                }
            }
            return new string(chars, 0, x);
        }


        // Implement IDisposable. http://msdn.microsoft.com/en-us/library/system.gc.suppressfinalize.aspx
        // Do not make this method virtual. 
        // A derived class should not be able to override this method. 
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue  
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the  
        // runtime from inside the finalizer and you should not reference  
        // other objects. Only unmanaged resources can be disposed. 
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed  
                // and unmanaged resources. 
                if (disposing)
                {
                    dataAccess.Dispose();
                    emptyList.Clear();


                }

                // Call the appropriate methods to clean up  
                // unmanaged resources here. 
                // If disposing is false,  
                // only the following code is executed.
            }
            _disposed = true;
        }

        // Use C# destructor syntax for finalization code. 
        // This destructor will run only if the Dispose method  
        // does not get called. 
        // It gives your base class the opportunity to finalize. 
        // Do not provide destructors in types derived from this class.
        ~CcDal()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability.
            Dispose(false);
        }

    }

    static class DataReaderExtensions
    {
        public static string GetStringOrNull(this IDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        public static string GetStringOrNull(this IDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : reader.GetString(reader.GetOrdinal(columnName));
        }
    }


    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int size)
        {
            int i = 0;
            var list = new List<T>(size);
            foreach (T item in source)
            {
                list.Add(item);
                if (++i == size)
                {
                    yield return list;
                    list = new List<T>(size);
                    i = 0;
                }
            }
            if (list.Count > 0)
                yield return list;
        }
    }

}

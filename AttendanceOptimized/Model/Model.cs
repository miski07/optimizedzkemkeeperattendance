﻿using Attendance;
using AttendanceOptimized.Objects;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AttendanceOptimized.Model
{
    internal class Model
    {
        public Model()
        {

        }

        public Dictionary<String, DatabaseRecord> getRecord(Connection newDB, DateTime dateUsed)
        {
            Dictionary<String, DatabaseRecord> records = new Dictionary<String, DatabaseRecord>();
            try
            {
                newDB.open();
                DateTime dayBefore = dateUsed - TimeSpan.FromDays(1);
                String query = "EXEC getAbsensiPerRoster @startDate = '" + dayBefore.ToString("MM/dd/yyyy") + "', @endDate='" + dateUsed.ToString("MM/dd/yyyy") + "'";
                //MessageBox.Show(query);
                SqlDataReader reader = newDB.Reader(query);
                while (reader.Read())
                {
                    String NIK = reader[0].ToString();
                    DateTime date = reader.GetDateTime(1);
                    String IN = (reader[2] != null) ? reader[2].ToString() : String.Empty;
                    String OUT = (reader[3] != null) ? reader[3].ToString() : String.Empty;
                    String Roster = (reader[4] != null) ? reader[4].ToString() : String.Empty;
                    if (!records.ContainsKey(NIK))
                    {
                        records.Add(NIK, new DatabaseRecord());
                    }
                    records[NIK].addIN(date, IN, dateUsed);
                    records[NIK].addOUT(date, OUT, dateUsed);
                    records[NIK].AddRoster(date, Roster, dateUsed);
                }
                reader.Close();
                return records;
            }
            catch(Exception ex)
            {
                return records;
            }


        }

        public void insertRecordToDatabase(Connection conn, Karyawan karyawan, String entryType, String Site, String KodeABS)
        {
            try
            {
                String IN = (entryType == "IN") ? "'" + karyawan.inTime.ToString("HH:mm") + "'" : "NULL";
                String OUT = "NULL"; 
                DateTime dateForDB = (entryType == "IN") ? karyawan.inTime : (entryType == "OUT") ? karyawan.outTime : (entryType == "PREVOUT") ? karyawan.prevOutTime : DateTime.MaxValue;

                if (entryType == "OUT")
                {
                    OUT = (karyawan.outTime != DateTime.MinValue) ? $"'{karyawan.outTime.ToString("HH:mm")}'" : $"'{karyawan.prevOutTime.ToString("HH:mm")}'";
                    dateForDB = (dateForDB==DateTime.MinValue)? karyawan.prevOutTime : dateForDB;
                }
               IN = (entryType == "PREVOUT") ? "NULL" : IN;
                OUT = (entryType == "PREVOUT") ? "'" + karyawan.prevOutTime.ToString("HH:mm") + "'" : OUT;
                String Query = "EXEC InsertAbsensiByType @NIK = '" + karyawan.NIK + "', @Tanggal = '" + dateForDB.Date.ToString("MM/dd/yyy") + "', @IN= " + IN + ", @OUT= " + OUT + " , @KodeST = '" + Site + "', @KodeABS = '" + KodeABS + "', @DataEntry = '" + entryType + "'";
                if (entryType == "OUT")
                {
                    //MessageBox.Show(OUT);
                }
                conn.executeQuery(Query);
            }
            catch(Exception ex)
            {
                return;
            }
        }
    }
}

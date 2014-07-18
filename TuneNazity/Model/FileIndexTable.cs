using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TuneNazity.Model
{
    public class FileIndexTable
    {

        public FileIndexTable(int id, string path, string file_name, long file_size, DateTime last_date_modified)
        {
            ID=id;
            Path=path;
            FileName=file_name;
            FileSize=file_size;
            LastDateModified=last_date_modified;

        }

        private int m_id;
        public int ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        private string m_path;
        public string Path
        {
            get { return m_path; }
            set { m_path = value; }
        }


        private string m_filename;
        public string FileName
        {
            get { return m_filename; }
            set { m_filename = value; }
        }


        private long m_filesize;
        public long  FileSize
        {
            get { return m_filesize; }
            set { m_filesize = value; }
        }

        private DateTime m_lastdatemodified;
        public DateTime  LastDateModified
        {
            get { return m_lastdatemodified; }
            set { m_lastdatemodified = value; }
        }

       
    }
}

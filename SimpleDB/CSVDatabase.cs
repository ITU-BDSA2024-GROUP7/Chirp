using System.Globalization;
using CsvHelper;

namespace SimpleDB
{
    public sealed class CSVDatabase<T> : IDatabaseRepository<T>
    {
        private readonly string _filepath;

        public CSVDatabase(string filePath)
        {
            _filepath = filePath;
        }

        public IEnumerable<T> Read(int? limit = null)
        {
            IEnumerable<T> records = new List<T>();
            using (StreamReader reader = new StreamReader(_filepath))
            using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = csv.GetRecords<T>().ToList();
            }
            return records;
        }

        public void Store(T record)
        {
            using (StreamWriter writer = new StreamWriter(_filepath, true))
            using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.NextRecord();
                csv.WriteRecord(record);
            }
        }
    }
}
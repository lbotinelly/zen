namespace Zen.Module.Data.Relational {
    public class RelationalStatements
    {
        public string AllFields = "SELECT * FROM {0} WHERE {1}";
        public string CustomSelectQuery = "SELECT {0} FROM {2} WHERE ({1})";
        public string GetAll = "SELECT * FROM {0}";
        public string GetAllSpecified = "SELECT {0} FROM {1}";
        //public string GetSingle = "SELECT * FROM {0} WHERE {1} = {2}Id";
        public string GetSingleByIdentifier = "SELECT * FROM {0} WHERE {1} = {2}";
        public string GetManyByIdentifier = "SELECT * FROM {0} WHERE {1} IN {2}";
        public string InsertSingle = "INSERT INTO {0} ({1}) VALUES ({2})";
        public string InsertSingleWithReturn = "INSERT INTO {0} ({1}) VALUES ({2}); select last_insert_rowid() as {4}newid";
        public string OrderByCommand = "ORDER BY";
        public string PaginationWrapper = "SELECT * FROM(SELECT A.*, ROWNUM C___RN FROM({0}) A) WHERE C___RN BETWEEN {1}+1 AND {2}";
        public string RemoveSingleParametrized = "DELETE FROM {0} WHERE {1} = {2}Id";
        public string ReturnNewIdentifier = "select last_insert_rowid() as newid";
        public string RowCount = "SELECT COUNT(*) FROM {0}";
        public string TableTruncate = "TRUNCATE TABLE {0}";
        public string UpdateSingle = "UPDATE {0} SET {1} WHERE {2} = {3}";
    }
}
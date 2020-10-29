namespace Zen.Module.Data.Relational {
    public class RelationalStatements
    {
        public string PaginationWrapper = "SELECT * FROM(SELECT A.*, ROWNUM C___RN FROM({0}) A) WHERE C___RN BETWEEN {1}+1 AND {2}";

        public string DropSet = "TRUNCATE TABLE {StorageCollectionName}";

        public string GetModelByIdentifier = "SELECT {InlineFieldSet} FROM {StorageCollectionName} WHERE {StorageKeyMemberName} = {ParameterPrefix}{KeyMemberName}";
        public string InsertModel = "INSERT INTO {StorageCollectionName} ({InlineFieldSet}) VALUES ({InlineParameterSet});select last_insert_id();";
        public string UpdateModel = "UPDATE {StorageCollectionName} SET {InterspersedFieldParameterSetSansKey} WHERE {StorageKeyMemberName} = {ParameterPrefix}{KeyMemberName}";
        public string RemoveModel = "DELETE FROM {StorageCollectionName} WHERE {StorageKeyMemberName} = {ParameterPrefix}{KeyMemberName}";

        public string GetSetComplete = "SELECT {InlineFieldSet} FROM {StorageCollectionName}";
        public string GetSetByWhere = "SELECT {InlineFieldSet} FROM {StorageCollectionName} WHERE ({0})";
        public string GetSetByIdentifiers = "SELECT {InlineFieldSet} FROM {StorageCollectionName} WHERE ( {StorageKeyMemberName} IN {ParameterPrefix}{KeyMemberName} )";
        public string RemoveSetByIdentifiers = "DELETE FROM {StorageCollectionName} WHERE ( {StorageKeyMemberName} IN {ParameterPrefix}{KeyMemberName} )";

        public string RowCount = "SELECT COUNT(*) FROM {StorageCollectionName}";
        public string CheckKey = "SELECT COUNT(*) FROM {StorageCollectionName} WHERE {StorageKeyMemberName} = {ParameterPrefix}{KeyMemberName}";
        public string ParametrizedKeyField = "{ParameterPrefix}{StorageKeyMemberName}";
    }
}
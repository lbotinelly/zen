﻿namespace Zen.Base.Module.Data
{
    public class QueryTransform
    {
        public string Filter;
        public string OmniQuery;
        public string OrderBy;
        public string Statement;
        public string OutputMembers;

        public Pagination Pagination = null;
    }
}